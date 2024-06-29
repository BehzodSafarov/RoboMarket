using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RoboMarketPro.Models;
using Microsoft.AspNetCore.Identity;
using RoboMarketPro.Data;

namespace RoboMarketPro.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(ApplicationDbContext context, 
                       ILogger<UserService> logger, 
                       IHttpContextAccessor httpContextAccessor, 
                       IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _passwordHasher = passwordHasher;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all users");
            return await _context.Users.Include(u => u.Orders).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            throw;
        }
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation($"Retrieving user with ID {id}");
            var user = await _context.Users.Include(u => u.Orders)
                                            .ThenInclude(o => o.OrderItems)
                                            .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {id} not found");
                throw new Exception("User not found");
            }
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving user with ID {id}");
            throw;
        }
    }

    public async Task UpdateUserAsync(User user)
    {
        try
        {
            _logger.LogInformation($"Updating user with ID {user.Id}");
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                _logger.LogWarning($"User with ID {user.Id} not found");
                throw new Exception("User not found");
            }

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"User with ID {user.Id} updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            throw;
        }
    }

    public async Task DeleteUserAsync(int id)
    {
        try
        {
            _logger.LogInformation($"Deleting user with ID {id}");
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"User with ID {id} deleted successfully");
            }
            else
            {
                _logger.LogWarning($"User with ID {id} not found");
                throw new Exception("User not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting user with ID {id}");
            throw;
        }
    }

    public async Task<bool> LoginAsync(string phoneNumber, string password)
    {
        _logger.LogInformation($"Login attempt for {phoneNumber}");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

        if (user != null && _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Success)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, phoneNumber),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                IsPersistent = true,
                IssuedUtc = DateTimeOffset.UtcNow
            };

            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return true;
        }

        _logger.LogWarning($"Login failed for {phoneNumber}");
        return false;
    }

    public async Task<bool> RegisterAsync(string phoneNumber, string password)
    {
        _logger.LogInformation($"Registration attempt for {phoneNumber}");

        if (await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
        {
            _logger.LogWarning($"User with phone number {phoneNumber} already exists.");
            return false;
        }

        var user = new User
        {
            PhoneNumber = phoneNumber,
            PasswordHash = _passwordHasher.HashPassword(null, password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"User {phoneNumber} registered successfully.");

        return await LoginAsync(phoneNumber, password);
    }

    public async Task LogoutAsync()
    {
        await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
