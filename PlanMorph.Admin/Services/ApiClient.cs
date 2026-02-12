using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PlanMorph.Admin.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly AuthStateService _authState;
    private string? _token;

    public ApiClient(HttpClient httpClient, IConfiguration configuration, AuthStateService authState)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _authState = authState;
        var baseUrl = configuration["ApiBaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl) || string.Equals(baseUrl.Trim(), "<set-in-env>", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = "http://localhost:5038";
        }
        _httpClient.BaseAddress = new Uri(baseUrl);
        EnsureAuthHeader();
    }

    public void SetAuthToken(string token)
    {
        _token = token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        EnsureAuthHeader();
        var response = await _httpClient.GetAsync(endpoint);
        
        if (!response.IsSuccessStatusCode)
            return default;

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        EnsureAuthHeader();
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(endpoint, content);
        
        if (!response.IsSuccessStatusCode)
            return default;

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<bool> PutAsync<T>(string endpoint, T data)
    {
        EnsureAuthHeader();
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PutAsync(endpoint, content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        EnsureAuthHeader();
        var response = await _httpClient.DeleteAsync(endpoint);
        return response.IsSuccessStatusCode;
    }

    private void EnsureAuthHeader()
    {
        var token = _authState.Token ?? _token;
        if (string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return;
        }

        if (_httpClient.DefaultRequestHeaders.Authorization?.Parameter != token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
