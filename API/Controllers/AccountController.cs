using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper) : BaseApiController
{
  private readonly UserManager<AppUser> _userManager = userManager;
  private readonly ITokenService _tokenService = tokenService;
  private readonly IMapper _mapper = mapper;

  [HttpPost("register")]
  public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
  {
    if (await UserExist(registerDto.Username))
    {
      return BadRequest("Username is taken");
    }

    var user = _mapper.Map<AppUser>(registerDto);

    user.UserName = registerDto.Username.ToLower();

    // var user = new AppUser
    // {
    //   UserName = registerDto.Username.ToLower(),
    //   PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
    //   PasswordSalt = hmac.Key
    // };

    var result = await _userManager.CreateAsync(user, registerDto.Password);

    if (!result.Succeeded) return BadRequest(result.Errors);

    return new UserDto
    {
      Username = registerDto.Username,
      KnownAs = user.KnownAs,
      Gender = user.Gender,
      Token = await _tokenService.CreateToken(user),
    };
  }

  [HttpPost("login")]
  public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
  {
    var user = await _userManager.Users.Include(x => x.Photos).FirstOrDefaultAsync(x => x.NormalizedUserName == loginDto.Username.ToUpper());

    if (user == null || user.UserName == null)
    {
      return Unauthorized("Invalid username");
    }

    var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

    if (!result) return Unauthorized("Invalid password");

    var mainPhoto = user.Photos.FirstOrDefault(p => p.IsMain);

    return new UserDto
    {
      Username = user.UserName,
      KnownAs = user.KnownAs,
      Gender = user.Gender,
      Token = await _tokenService.CreateToken(user),
      MainPhotoUrl = mainPhoto?.Url
    };

  }

  private async Task<bool> UserExist(string username)
  {
    return await _userManager.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper());
  }
}
