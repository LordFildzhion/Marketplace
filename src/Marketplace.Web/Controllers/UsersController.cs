using System.Security.Claims;
using Marketplace.Application.DTOs.Users;
using Marketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Web.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var user = await _userService.GetByIdAsync(UserId);
        return Ok(user);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileRequest request)
    {
        var user = await _userService.UpdateProfileAsync(UserId, request);
        return Ok(user);
    }
}
