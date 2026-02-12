using PlanMorph.Core.Entities;

namespace PlanMorph.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user, string role);
}