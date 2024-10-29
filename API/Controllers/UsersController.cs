using System.Net;
using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService) : BaseApiController
{
  private readonly IUserRepository _userRepository = userRepository;
  private readonly IMapper _mapper = mapper;
  private readonly IPhotoService _photoService = photoService;

  [HttpGet]
  public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
  {
    userParams.CurrentUsername = User.GetUsername();
    var users = await _userRepository.GetMembersAsync(userParams);

    Response.AddPaginationHeader(users);

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
    var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

    if (user == null) return NotFound("No user found");

    _mapper.Map(memberUpdateDto, user);

    if (await _userRepository.SaveAllAsync()) return NoContent();

    return BadRequest("Failed to update the user");
  }

  [HttpPost("add-photo")]
  public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
  {
    var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

    if (user == null) return BadRequest("Cannot update user");

    var result = await _photoService.AddPhotoAsync(file);

    if (result.Error != null) BadRequest(result.Error.Message);

    var photo = new Photo
    {
      Url = result.SecureUrl.AbsoluteUri,
      PublicId = result.PublicId
    };

    if (user.Photos.Count == 0)
    {
      photo.IsMain = true;
    }

    user.Photos.Add(photo);

    if (await _userRepository.SaveAllAsync()) return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));

    return BadRequest("Problem adding photo");
  }

  [HttpPut("set-main-photo/{photoId:int}")]
  public async Task<ActionResult> SetMainPhoto(int photoId)
  {
    var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

    if (user == null) return BadRequest("Cannot update user");

    var photo = user.Photos.FirstOrDefault((p) => p.Id == photoId);

    if (photo == null || photo.IsMain) return BadRequest("No such photo for this user");

    var currentMainPhoto = user.Photos.FirstOrDefault((p) => p.IsMain);
    if (currentMainPhoto != null) currentMainPhoto.IsMain = false;
    photo.IsMain = true;

    if (await _userRepository.SaveAllAsync()) return NoContent();

    return BadRequest("Problem setting main photo");

  }

  [HttpDelete("delete-photo/{photoId:int}")]
  public async Task<ActionResult> DeletePhoto(int photoId)
  {
    var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

    if (user == null) return BadRequest("Unable to find user");

    var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

    if (photo == null || photo.IsMain) return BadRequest("This photo cannot be deleted");

    if (photo.PublicId != null)
    {
      var result = await _photoService.DeletePhotoAsync(photo.PublicId);
      if (result.Error != null) return BadRequest(result.Error.Message);
    }

    user.Photos.Remove(photo);

    if (await _userRepository.SaveAllAsync()) return Ok();

    return BadRequest("Problem deleting photo");
  }
}
