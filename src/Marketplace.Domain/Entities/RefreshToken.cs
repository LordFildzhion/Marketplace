
namespace Marketplace.Domain.Entities;
using Marketplace.Domain.Common;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    private RefreshToken() { } // for EF Core
}
