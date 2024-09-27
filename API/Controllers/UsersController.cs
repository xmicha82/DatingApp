using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository) : BaseApiController
{
  private readonly IUserRepository _userRepository = userRepository;

  [HttpGet]
  public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
  {
    var users = await _userRepository.GetMembersAsync();

    return Ok(users);
  }

  [HttpGet("{username}")]
  public async Task<ActionResult<MemberDto>> GetUser(string username)
  {
    var user = await _userRepository.GetMemberAsync(username);

    if (user is null) return NotFound();

    return Ok(user);
  }

}
