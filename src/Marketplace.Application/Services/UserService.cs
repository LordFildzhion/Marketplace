using AutoMapper;
using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.DTOs.Common;
using Marketplace.Application.DTOs.Users;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Marketplace.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, IMapper mapper, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null) throw new NotFoundException("User", userId);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null) throw new NotFoundException("User", userId);
        user.UpdateProfile(request.FirstName ?? user.FirstName, request.LastName ?? user.LastName, request.Phone ?? user.Phone);
        await _userRepository.UpdateAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(UserFilterRequest filter, CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllAsync(ct);

        if (!string.IsNullOrWhiteSpace(filter.Role))
        {
            users = users.Where(u => u.Role.ToString().Equals(filter.Role, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            users = users.Where(u =>
                (u.Email?.Value?.ToLower()?.Contains(search) ?? false) ||
                (u.FirstName?.ToLower()?.Contains(search) ?? false) ||
                (u.LastName?.ToLower()?.Contains(search) ?? false) ||
                ($"{u.FirstName} {u.LastName}".ToLower().Contains(search))
            ).ToList();
        }

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;
        var total = users.Count;
        var paged = users.Skip((page-1)*pageSize).Take(pageSize).ToList();
        return new PagedResult<UserDto>(_mapper.Map<List<UserDto>>(paged), total, page, pageSize);
    }

    public async Task ApproveSellerAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null) throw new NotFoundException("User", userId);
        if (user.Role != Domain.Enums.UserRole.Seller)
            throw new AppException("User is not a seller.");
        user.Approve();
        await _userRepository.UpdateAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);
    }

    public async Task DisapproveSellerAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null) throw new NotFoundException("User", userId);
        if (user.Role != Domain.Enums.UserRole.Seller)
            throw new AppException("User is not a seller.");
        user.Disapprove();
        await _userRepository.UpdateAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);
    }

    public async Task<string> ResetUserPasswordAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null) throw new NotFoundException("User", userId);
        var newPassword = GenerateRandomPassword();
        user.ChangePassword(_passwordHasher.Hash(newPassword));
        await _userRepository.UpdateAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);
        return newPassword;
    }

    private string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";
        var random = new Random();
        return new string(Enumerable.Range(0, 12).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}
