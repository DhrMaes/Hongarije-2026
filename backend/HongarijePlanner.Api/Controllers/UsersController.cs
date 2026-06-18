using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using HongarijePlanner.Api.Data;
using HongarijePlanner.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HongarijePlanner.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await dbContext.Users
            .OrderBy(user => user.Name)
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<User>> UpsertUser([FromBody] UpsertUserRequest request)
    {
        var user = await dbContext.Users.FindAsync(request.Name);
        if (user is null)
        {
            user = new User { Name = request.Name, IsAdmin = false };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        await DataSeeder.SeedPackingForUserAsync(dbContext, request.Name);

        return Ok(user);
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteUser(string name)
    {
        var user = await dbContext.Users.FindAsync(name);
        if (user is null) return NotFound();

        dbContext.PackingItems.RemoveRange(
            dbContext.PackingItems.Where(p => p.Owner == name));

        dbContext.WishlistVotes.RemoveRange(
            dbContext.WishlistVotes.Where(v => v.UserName == name));

        dbContext.ItineraryVotes.RemoveRange(
            dbContext.ItineraryVotes.Where(v => v.UserName == name));

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{name}/verify-pin")]
    public async Task<IActionResult> VerifyPin(string name, [FromBody] PinRequest request)
    {
        var user = await dbContext.Users.FindAsync(name);
        if (user is null) return NotFound();
        if (string.IsNullOrEmpty(user.PinHash)) return NoContent(); // no PIN set — allow
        if (!CheckPin(request.Pin, user.PinHash)) return Unauthorized("Onjuiste PIN.");
        return NoContent();
    }

    [HttpPost("{name}/pin")]
    public async Task<IActionResult> SetPin(string name, [FromBody] SetPinRequest request)
    {
        var user = await dbContext.Users.FindAsync(name);
        if (user is null) return NotFound();

        if (!string.IsNullOrEmpty(user.PinHash) &&
            !CheckPin(request.CurrentPin ?? string.Empty, user.PinHash))
            return Unauthorized("Huidige PIN is onjuist.");

        if (string.IsNullOrEmpty(request.NewPin) || request.NewPin.Length < 4)
            return BadRequest("PIN moet minimaal 4 tekens zijn.");

        user.PinHash = HashPin(request.NewPin);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{name}/pin")]
    public async Task<IActionResult> RemovePin(string name, [FromBody] PinRequest request)
    {
        var user = await dbContext.Users.FindAsync(name);
        if (user is null) return NotFound();
        if (string.IsNullOrEmpty(user.PinHash)) return NoContent();
        if (!CheckPin(request.Pin, user.PinHash)) return Unauthorized("Huidige PIN is onjuist.");

        user.PinHash = null;
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    private static string HashPin(string pin)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(pin), salt, 100_000, HashAlgorithmName.SHA256, 32);
        return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
    }

    private static bool CheckPin(string pin, string stored)
    {
        var parts = stored.Split('.');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(pin), salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    public class UpsertUserRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }

    public class PinRequest
    {
        public string Pin { get; set; } = string.Empty;
    }

    public class SetPinRequest
    {
        public string? CurrentPin { get; set; }
        [Required]
        public string NewPin { get; set; } = string.Empty;
    }
}
