using System.ComponentModel.DataAnnotations;
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

    public class UpsertUserRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
