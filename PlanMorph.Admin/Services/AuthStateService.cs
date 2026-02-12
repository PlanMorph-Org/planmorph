using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using PlanMorph.Core.Entities;

namespace PlanMorph.Admin.Services;

public class AuthStateService
{
    public event Action? OnChange;

    public bool IsAuthenticated { get; private set; }
    public string? UserEmail { get; private set; }
    public string? UserName { get; private set; }
    public string? Token { get; private set; }
    public UserRole UserRole { get; private set; } = UserRole.Client;

    private readonly ProtectedLocalStorage _storage;
    private const string StorageKey = "planmorph.admin.auth";
    private bool _isInitialized;

    public AuthStateService(ProtectedLocalStorage storage)
    {
        _storage = storage;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        try
        {
            var result = await _storage.GetAsync<AuthSnapshot>(StorageKey);
            var snapshot = result.Success ? result.Value : null;
            _isInitialized = true;

            if (snapshot != null && !string.IsNullOrWhiteSpace(snapshot.Token))
            {
                Token = snapshot.Token;
                UserEmail = snapshot.Email;
                UserName = $"{snapshot.FirstName} {snapshot.LastName}".Trim();
                UserRole = snapshot.Role;
                IsAuthenticated = true;
                NotifyStateChanged();
            }
        }
        catch
        {
            _isInitialized = false;
            // Ignore storage failures (e.g., during prerender)
        }
    }

    public async Task LoginAsync(string token, string email, string firstName, string lastName, UserRole role = UserRole.Admin)
    {
        Token = token;
        UserEmail = email;
        UserName = $"{firstName} {lastName}";
        UserRole = role;
        IsAuthenticated = true;

        await _storage.SetAsync(StorageKey, new AuthSnapshot
        {
            Token = token,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = role
        });

        NotifyStateChanged();
    }

    public async Task LogoutAsync()
    {
        Token = null;
        UserEmail = null;
        UserName = null;
        UserRole = UserRole.Client;
        IsAuthenticated = false;

        await _storage.DeleteAsync(StorageKey);

        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    private sealed class AuthSnapshot
    {
        public string Token { get; set; } = "";
        public string Email { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public UserRole Role { get; set; } = UserRole.Client;
    }
}
