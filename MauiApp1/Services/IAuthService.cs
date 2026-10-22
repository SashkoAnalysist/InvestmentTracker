using MauiApp1.Models;

namespace MauiApp1.Services;

public interface IAuthService
{
    User? CurrentUser { get; }
    bool IsAdmin { get; }
    Task<bool> LoginAsync(string username, string password);
    void Logout();
}
