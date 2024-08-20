using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context) : BaseApiController
{
  private readonly DataContext _context = context;

  [HttpPost("register")]
  public async Task<ActionResult<AppUser>> Register(RegisterDto registerDto)
  {
    if (await UserExist(registerDto.Username))
    {
      return BadRequest("Username is taken");
    }

    using var hmac = new HMACSHA512();

    var user = new AppUser
    {
      UserName = registerDto.Username.ToLower(),
      PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
      PasswordSalt = hmac.Key
    };

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    return Ok(user);
  }

  [HttpPost("login")]
  public async Task<ActionResult<AppUser>> Login(LoginDto loginDto)
  {
    var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

    if (user == null)
    {
      return Unauthorized("Invalid username");
    }

    using var hmac = new HMACSHA512(user.PasswordSalt);

    var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

    if (!PasswordMatch(computed, user.PasswordHash))
    {
      return Unauthorized("Invalid Password");
    }

    return user;

  }

  private async Task<bool> UserExist(string username)
  {
    return await _context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower());
  }

  private bool PasswordMatch(byte[] incoming, byte[] stored)
  {
    for (int i = 0; i < incoming.Length; i++)
    {
      if (incoming[i] != stored[i]) return false;
    }

    return true;
  }
}
