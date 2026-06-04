using Marketplace.Application.DTOs.Common;
using Marketplace.Application.DTOs.Users;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Interfaces;
using Marketplace.Application.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.Web.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;

    public AdminController(IUserService userService, IUserRepository userRepository)
    {
        _userService = userService;
        _userRepository = userRepository;
    }

    [HttpGet("users")]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] UserFilterRequest filter)
    {
        var users = await _userService.GetUsersAsync(filter);
        return Ok(users);
    }

    [HttpDelete("users/{userId:guid}")]
    public async Task<ActionResult> DeleteUser(Guid userId)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        if (currentUserId == userId)
            return BadRequest(new { message = "Cannot delete yourself." });

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException("User", userId);
        
        if (user.Role == Domain.Enums.UserRole.Admin)
        {
            var adminCount = (await _userRepository.GetAllAsync()).Count(u => u.Role == Domain.Enums.UserRole.Admin);
            if (adminCount <= 1)
                return BadRequest(new { message = "Cannot delete the last admin." });
        }

        await _userRepository.DeleteAsync(user);
        await _userRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("users/{userId:guid}/reset-password")]
    public async Task<ActionResult> ResetPassword(Guid userId)
    {
        var newPassword = await _userService.ResetUserPasswordAsync(userId);
        return Ok(new { newPassword });
    }

    [HttpPatch("users/{userId:guid}/approve")]
    public async Task<ActionResult> ApproveSeller(Guid userId)
    {
        await _userService.ApproveSellerAsync(userId);
        return Ok();
    }

    [HttpPatch("users/{userId:guid}/disapprove")]
    public async Task<ActionResult> DisapproveSeller(Guid userId)
    {
        await _userService.DisapproveSellerAsync(userId);
        return Ok();
    }
}
