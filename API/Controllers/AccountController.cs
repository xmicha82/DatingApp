using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService, IMapper mapper) : BaseApiController
{
  private readonly DataContext _context = context;
  private readonly ITokenService _tokenService = tokenService;
  private readonly IMapper _mapper = mapper;

  [HttpPost("register")]
  public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
  {
    if (await UserExist(registerDto.Username))
    {
      return BadRequest("Username is taken");
    }

    using var hmac = new HMACSHA512();

    var user = _mapper.Map<AppUser>(registerDto);

    user.UserName = registerDto.Username.ToLower();
    user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
    user.PasswordSalt = hmac.Key;

    // var user = new AppUser
    // {
    //   UserName = registerDto.Username.ToLower(),
    //   PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
    //   PasswordSalt = hmac.Key
    // };

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    return new UserDto
    {
      Username = registerDto.Username,
      KnownAs = user.KnownAs,
      Token = _tokenService.CreateToken(user)
    };
  }

  [HttpPost("login")]
  public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
  {
    var user = await _context.Users.Include(x => x.Photos).FirstOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

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

    var mainPhoto = user.Photos.FirstOrDefault(p => p.IsMain);

    return new UserDto
    {
      Username = user.UserName,
      KnownAs = user.KnownAs,
      Token = _tokenService.CreateToken(user),
      MainPhotoUrl = mainPhoto?.Url
    };

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
