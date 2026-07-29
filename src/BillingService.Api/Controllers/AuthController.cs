using BillingService.Api.Services;
using BillingService.Application.DTOs;
using BillingService.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _tokenService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService tokenService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    // Public self-registration always grants the least-privilege role (Cashier).
    // Admin/Manager accounts are created via CreateStaff below, by an existing Admin.
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        await _registerValidator.ValidateAndThrowAsync(request, ct);

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, Roles.Cashier);

        var token = _tokenService.GenerateToken(user, new[] { Roles.Cashier });
        return Ok(new AuthResponse(token, user.Email!, user.FullName, new[] { Roles.Cashier }));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        await _loginValidator.ValidateAndThrowAsync(request, ct);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);
        return Ok(new AuthResponse(token, user.Email!, user.FullName, roles.ToArray()));
    }

    [HttpPost("create-staff")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<AuthResponse>> CreateStaff(CreateStaffRequest request)
    {
        if (!Roles.All.Contains(request.Role))
            return BadRequest($"Role must be one of: {string.Join(", ", Roles.All)}");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, request.Role);

        var token = _tokenService.GenerateToken(user, new[] { request.Role });
        return Ok(new AuthResponse(token, user.Email!, user.FullName, new[] { request.Role }));
    }
}
