using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Asp.Versioning;
using Adra.Application.DTOs;
using Adra.Application.Interfaces;
using Adra.Infrastructure.Identity;

namespace Adra.Api.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public AuthController(UserManager<ApplicationUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new { message = "Account is inactive" });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var domainUser = user.ToDomainUser();
        var response = _jwtService.GenerateToken(domainUser, roles.ToList());

        return Ok(response);
    }
}
