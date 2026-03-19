namespace PlanMorph.Application.DTOs.Auth;

public class GoogleProfessionalLoginDto
{
    public string GoogleIdToken { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
