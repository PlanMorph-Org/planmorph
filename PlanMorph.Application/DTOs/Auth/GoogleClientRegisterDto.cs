namespace PlanMorph.Application.DTOs.Auth;

public class GoogleClientRegisterDto
{
    public string GoogleIdToken { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}
