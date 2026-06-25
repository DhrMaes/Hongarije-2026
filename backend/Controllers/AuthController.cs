using HongarijePlanner.Api.Data;
using HongarijePlanner.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace HongarijePlanner.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<User>> GetMe()
    {
        // Authentik forward auth injects X-authentik-username on every request.
        // Fall back to X-authentik-name (display name) if username is not set.
        var name = Request.Headers["X-authentik-username"].FirstOrDefault()
                ?? Request.Headers["X-authentik-name"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(name))
            return NoContent(); // No Authentik header — show login screen

        var user = await dbContext.Users.FindAsync(name);
        if (user is null)
        {
            user = new User { Name = name, IsAdmin = false };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        await DataSeeder.SeedPackingForUserAsync(dbContext, name);

        return Ok(user);
    }
}
