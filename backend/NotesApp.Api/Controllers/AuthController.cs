using Microsoft.AspNetCore.Mvc;
using NotesApp.Api.Common;
using NotesApp.Core.DTOs;
using NotesApp.Services;

namespace NotesApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
    {
        var token = await _authService.RegisterAsync(dto);
        var payload = new { Token = token };
        return Ok(ApiResponse.Success(payload, "User registered successfully.", HttpContext.TraceIdentifier));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        var token = await _authService.LoginAsync(dto);
        var payload = new { Token = token };
        return Ok(ApiResponse.Success(payload, "Login successful.", HttpContext.TraceIdentifier));
    }
}
