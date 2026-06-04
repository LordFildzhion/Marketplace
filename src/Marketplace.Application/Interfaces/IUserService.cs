using Marketplace.Application.DTOs.Common;
using Marketplace.Application.DTOs.Users;

namespace Marketplace.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<PagedResult<UserDto>> GetUsersAsync(UserFilterRequest filter, CancellationToken ct = default);
    Task ApproveSellerAsync(Guid userId, CancellationToken ct = default);
    Task DisapproveSellerAsync(Guid userId, CancellationToken ct = default);
    Task<string> ResetUserPasswordAsync(Guid userId, CancellationToken ct = default);
}
