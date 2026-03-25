using NotesApp.Core.DTOs;

namespace NotesApp.Services;

public interface IAuthService
{
    Task<string> RegisterAsync(UserRegisterDto dto);
    Task<string> LoginAsync(UserLoginDto dto);
}
