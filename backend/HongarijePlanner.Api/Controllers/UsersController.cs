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
    public async Task<ActionResult<IEnumerable<string>>> GetUsers()
    {
        var users = await dbContext.Users
            .OrderBy(user => user.Name)
            .Select(user => user.Name)
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> UpsertUser([FromBody] UpsertUserRequest request)
    {
        var exists = await dbContext.Users.AnyAsync(user => user.Name == request.Name);
        if (!exists)
        {
            dbContext.Users.Add(new User { Name = request.Name });
            await dbContext.SaveChangesAsync();
        }

        return Ok();
    }

    public class UpsertUserRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
