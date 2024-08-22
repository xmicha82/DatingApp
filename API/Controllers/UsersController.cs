using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class UsersController(DataContext context) : BaseApiController
{
  private readonly DataContext _context = context;

  [HttpGet]
  [AllowAnonymous]
  public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
  {
    var users = await _context.Users.ToListAsync();

    return Ok(users);
  }

  [HttpGet("{id:int}")]
  [Authorize]
  public async Task<ActionResult<AppUser>> GetUser(int id)
  {
    var user = await _context.Users.FindAsync(id);

    if (user is null) return NotFound();

    return Ok(user);
  }

}
