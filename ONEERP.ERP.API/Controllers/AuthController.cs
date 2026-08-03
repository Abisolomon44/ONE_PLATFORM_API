using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ONEERP.ERP.API.DTOs;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Models;

namespace ONEERP.ERP.API.Controllers;

[Route("api/auth")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshValidator;

    public AuthController(
        IAuthService authService,
        IValidator<LoginRequest> loginValidator,
        IValidator<RefreshTokenRequest> refreshValidator)
    {
        _authService = authService;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var errors = await ValidateAsync(_loginValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<LoginResponse>.Fail("Validation failed", errors));

        var response = await _authService.LoginAsync(request.Username, request.Password);
        return Ok(ApiResponse<LoginResponse>.Ok(response, "Login successful"));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var errors = await ValidateAsync(_refreshValidator, request);
        if (errors.Count > 0)
            return BadRequest(ApiResponse<LoginResponse>.Fail("Validation failed", errors));

        var response = await _authService.RefreshAsync(request.Username, request.RefreshToken);
        return Ok(ApiResponse<LoginResponse>.Ok(response, "Token refreshed"));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return Ok(ApiResponse.Ok("Logged out successfully"));
    }
}
