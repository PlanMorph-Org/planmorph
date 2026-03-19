using PlanMorph.Application.DTOs.Auth;

namespace PlanMorph.Application.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> RegisterProfessionalWithGoogleAsync(GoogleProfessionalRegisterDto registerDto);
    Task<AuthResponseDto?> LoginProfessionalWithGoogleAsync(GoogleProfessionalLoginDto loginDto);
    Task<AuthResponseDto?> RegisterClientWithGoogleAsync(GoogleClientRegisterDto registerDto);
    Task<AuthResponseDto?> LoginClientWithGoogleAsync(GoogleClientLoginDto loginDto);
}