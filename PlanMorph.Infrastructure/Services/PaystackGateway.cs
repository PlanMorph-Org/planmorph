using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlanMorph.Application.DTOs.Payments;
using PlanMorph.Application.Services;

namespace PlanMorph.Infrastructure.Services;

public class PaystackGateway : IPaystackGateway
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaystackGateway> _logger;
    private readonly string _secretKey;
    private readonly string _webhookSecret;
    private readonly string? _callbackUrl;
    private readonly string _currency;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PaystackGateway(HttpClient httpClient, IConfiguration configuration, ILogger<PaystackGateway> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _secretKey = NormalizeValue(configuration["Paystack:SecretKey"]) ?? string.Empty;
        _webhookSecret = NormalizeValue(configuration["Paystack:WebhookSecret"]) ?? _secretKey;
        _callbackUrl = NormalizeValue(configuration["Paystack:CallbackUrl"]);
        _currency = NormalizeValue(configuration["Paystack:Currency"]) ?? configuration["Currency:Base"] ?? "KES";

        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            _logger.LogWarning("Paystack secret key is not configured.");
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _secretKey);
        }
    }

    public async Task<PaymentInitializationResult?> InitializePaymentAsync(
        string email,
        decimal amount,
        string reference,
        string? subaccountCode = null,
        int? transactionChargeKobo = null,
        string? splitCode = null)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
            return null;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(reference))
            return null;

        var amountKobo = Convert.ToInt32(Math.Round(amount * 100m, MidpointRounding.AwayFromZero));

        var payload = new Dictionary<string, object?>
        {
            ["email"] = email,
            ["amount"] = amountKobo,
            ["reference"] = reference,
            ["currency"] = _currency
        };

        if (!string.IsNullOrWhiteSpace(subaccountCode))
        {
            payload["subaccount"] = subaccountCode;
            payload["bearer"] = "subaccount";
        }

        if (transactionChargeKobo.HasValue && transactionChargeKobo.Value > 0)
            payload["transaction_charge"] = transactionChargeKobo.Value;

        if (!string.IsNullOrWhiteSpace(splitCode))
            payload["split_code"] = splitCode;

        if (!string.IsNullOrWhiteSpace(_callbackUrl))
            payload["callback_url"] = _callbackUrl;

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("transaction/initialize", content);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Paystack initialize failed with status {Status}", response.StatusCode);
            return null;
        }

        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaystackInitializeResponse>(body, _jsonOptions);

        if (result?.Status != true || result.Data == null)
            return null;

        return new PaymentInitializationResult
        {
            AuthorizationUrl = result.Data.AuthorizationUrl ?? string.Empty,
            AccessCode = result.Data.AccessCode ?? string.Empty,
            Reference = result.Data.Reference ?? reference
        };
    }

    public async Task<PaymentVerificationResult?> VerifyPaymentAsync(string reference)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
            return null;

        if (string.IsNullOrWhiteSpace(reference))
            return null;

        var response = await _httpClient.GetAsync($"transaction/verify/{reference}");

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Paystack verify failed with status {Status}", response.StatusCode);
            return null;
        }

        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaystackVerifyResponse>(body, _jsonOptions);

        if (result?.Status != true || result.Data == null)
            return null;

        var status = result.Data.Status ?? string.Empty;

        return new PaymentVerificationResult
        {
            IsSuccessful = string.Equals(status, "success", StringComparison.OrdinalIgnoreCase),
            Reference = result.Data.Reference ?? reference,
            AmountKobo = result.Data.Amount,
            Currency = result.Data.Currency ?? string.Empty,
            Status = status
        };
    }

    public async Task<IReadOnlyList<PaystackBankOptionDto>> GetTransferBanksAsync(string country = "kenya", string? currency = null)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
            return Array.Empty<PaystackBankOptionDto>();

        var resolvedCurrency = string.IsNullOrWhiteSpace(currency) ? _currency : currency;
        var url = $"bank?country={Uri.EscapeDataString(country)}&currency={Uri.EscapeDataString(resolvedCurrency)}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Paystack bank lookup failed with status {Status}", response.StatusCode);
            return Array.Empty<PaystackBankOptionDto>();
        }

        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaystackBankListResponse>(body, _jsonOptions);
        if (result?.Status != true || result.Data == null)
            return Array.Empty<PaystackBankOptionDto>();

        return result.Data
            .Where(x => !string.IsNullOrWhiteSpace(x.Name) && !string.IsNullOrWhiteSpace(x.Code) && x.Active != false)
            .OrderBy(x => x.Name)
            .Select(x => new PaystackBankOptionDto
            {
                Name = x.Name!,
                Code = x.Code!,
                Type = x.Type
            })
            .ToList();
    }

    public async Task<PaystackTransferRecipientResult?> CreateTransferRecipientAsync(PaystackTransferRecipientRequest request)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
            return null;

        if (string.IsNullOrWhiteSpace(request.Type)
            || string.IsNullOrWhiteSpace(request.Name)
            || string.IsNullOrWhiteSpace(request.AccountNumber))
            return null;

        var payload = new Dictionary<string, object?>
        {
            ["type"] = request.Type,
            ["name"] = request.Name,
            ["account_number"] = request.AccountNumber,
            ["currency"] = request.Currency
        };

        if (!string.IsNullOrWhiteSpace(request.BankCode))
            payload["bank_code"] = request.BankCode;

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("transferrecipient", content);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Paystack transfer recipient creation failed with status {Status}", response.StatusCode);
            return null;
        }

        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaystackRecipientResponse>(body, _jsonOptions);

        if (result?.Status != true || result.Data == null || string.IsNullOrWhiteSpace(result.Data.RecipientCode))
            return null;

        return new PaystackTransferRecipientResult
        {
            IsSuccessful = true,
            RecipientCode = result.Data.RecipientCode,
            Message = result.Message
        };
    }

    public async Task<PaystackTransferResult?> InitiateTransferAsync(PaystackTransferRequest request)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
            return null;

        if (request.Amount <= 0
            || string.IsNullOrWhiteSpace(request.RecipientCode)
            || string.IsNullOrWhiteSpace(request.Reference))
            return null;

        var amountKobo = Convert.ToInt32(Math.Round(request.Amount * 100m, MidpointRounding.AwayFromZero));

        var payload = new Dictionary<string, object?>
        {
            ["source"] = "balance",
            ["amount"] = amountKobo,
            ["recipient"] = request.RecipientCode,
            ["reason"] = request.Reason,
            ["reference"] = request.Reference
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("transfer", content);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Paystack transfer initiation failed with status {Status}", response.StatusCode);
            return null;
        }

        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaystackTransferResponse>(body, _jsonOptions);
        if (result?.Status != true || result.Data == null)
            return null;

        var status = result.Data.Status?.Trim().ToLowerInvariant() ?? string.Empty;

        return new PaystackTransferResult
        {
            IsSuccessful = status == "success",
            IsPending = status == "pending" || status == "otp",
            TransferCode = result.Data.TransferCode,
            Message = result.Message
        };
    }

    public bool IsSignatureValid(string payload, string signature)
    {
        if (string.IsNullOrWhiteSpace(_webhookSecret) || string.IsNullOrWhiteSpace(signature))
            return false;

        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_webhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computed = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

        return computed == signature.Trim().ToLowerInvariant();
    }

    private static string? NormalizeValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return string.Equals(trimmed, "<set-in-env>", StringComparison.OrdinalIgnoreCase) ? null : trimmed;
    }

    private class PaystackInitializeResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("data")]
        public PaystackInitializeData? Data { get; set; }
    }

    private class PaystackInitializeData
    {
        [JsonPropertyName("authorization_url")]
        public string? AuthorizationUrl { get; set; }

        [JsonPropertyName("access_code")]
        public string? AccessCode { get; set; }

        [JsonPropertyName("reference")]
        public string? Reference { get; set; }
    }

    private class PaystackVerifyResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("data")]
        public PaystackVerifyData? Data { get; set; }
    }

    private class PaystackVerifyData
    {
        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("reference")]
        public string? Reference { get; set; }
    }

    private class PaystackRecipientResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public PaystackRecipientData? Data { get; set; }
    }

    private class PaystackRecipientData
    {
        [JsonPropertyName("recipient_code")]
        public string? RecipientCode { get; set; }
    }

    private class PaystackTransferResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public PaystackTransferData? Data { get; set; }
    }

    private class PaystackTransferData
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("transfer_code")]
        public string? TransferCode { get; set; }
    }

    private class PaystackBankListResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("data")]
        public List<PaystackBankData>? Data { get; set; }
    }

    private class PaystackBankData
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("active")]
        public bool? Active { get; set; }
    }
}
