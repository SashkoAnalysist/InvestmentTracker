using System.IO;
using System.Security.Cryptography;
using System.Text;
using MauiApp1.Models;
using SQLite;

namespace MauiApp1.Services;

public class AuthService : IAuthService
{
    private readonly SQLiteAsyncConnection _database;
    public User? CurrentUser { get; private set; }
    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;

    public AuthService()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "portfolio.db3");
        _database = new SQLiteAsyncConnection(dbPath);
        Task.Run(async () => await InitializeDefaultUsersAsync());
    }

    private async Task InitializeDefaultUsersAsync()
    {
        try
        {
            await _database.CreateTableAsync<User>();
            var count = await _database.Table<User>().CountAsync();
            if (count == 0)
            {
                await _database.InsertAsync(new User
                {
                    Username = "admin",
                    PasswordHash = HashPassword("admin123"),
                    Role = UserRole.Admin
                });

                await _database.InsertAsync(new User
                {
                    Username = "viewer",
                    PasswordHash = HashPassword("user123"),
                    Role = UserRole.Viewer
                });
            }
        }
        catch
        {
            // Фонова ініціалізація бази даних
        }
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var hash = HashPassword(password);
            var user = await _database.Table<User>()
                                      .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.PasswordHash == hash);

            if (user != null)
            {
                CurrentUser = user;
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    public void Logout() => CurrentUser = null;

    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}