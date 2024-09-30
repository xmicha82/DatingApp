using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository, IMapper mapper) : BaseApiController
{
  private readonly IUserRepository _userRepository = userRepository;
  private readonly IMapper _mapper = mapper;

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

  [HttpPut]
  public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
  {
    var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (username == null) return BadRequest("No useranme found in token");

    var user = await _userRepository.GetUserByUsernameAsync(username);

    if (user == null) return NotFound("No user found");

    _mapper.Map(memberUpdateDto, user);

    if (await _userRepository.SaveAllAsync()) return NoContent();

    return BadRequest("Failed to update the user");
  }

}
