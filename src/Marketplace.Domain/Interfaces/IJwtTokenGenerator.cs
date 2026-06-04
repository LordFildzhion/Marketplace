using Marketplace.Domain.Entities;

namespace Marketplace.Domain.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
