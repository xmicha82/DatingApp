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
public class MessagesController(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper) : BaseApiController
{
  private readonly IMessageRepository _messageRepository = messageRepository;
  private readonly IUserRepository _userRepository = userRepository;
  private readonly IMapper _mapper = mapper;

  [HttpPost]
  public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
  {
    var username = User.GetUsername();

    if (username == createMessageDto.RecipientUsername.ToLower()) return BadRequest("You cannot message yourself");

    var sender = await _userRepository.GetUserByUsernameAsync(username);
    var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

    if (sender == null || recipient == null) return BadRequest("Cannot send message at this time");

    var message = new Message
    {
      Sender = sender,
      Recipient = recipient,
      Content = createMessageDto.Content,
      RecipientUsername = recipient.UserName,
      SenderUsername = sender.UserName
    };

    _messageRepository.AddMessage(message);

    if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));

    return BadRequest("Failed to save Message");
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
  {
    messageParams.Username = User.GetUsername();

    var messages = await _messageRepository.GetMessagesForUser(messageParams);

    Response.AddPaginationHeader(messages);

    return messages;
  }

  [HttpGet("threads/{username}")]
  public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesThread(string username)
  {
    var currentUsername = User.GetUsername();

    return Ok(await _messageRepository.GetMessageThread(currentUsername, username));
  }

  [HttpDelete("{id:int}")]
  public async Task<ActionResult> DeleteMessage(int id)
  {
    var username = User.GetUsername();

    var message = await _messageRepository.GetMessage(id);

    if (message == null) return BadRequest("Cannot delete this message");

    if (message.SenderUsername != username && message.RecipientUsername != username) return Forbid();

    if (message.SenderUsername == username) message.SenderDeleted = true;
    if (message.RecipientUsername == username) message.RecipientDeleted = true;

    if (message is { SenderDeleted: true, RecipientDeleted: true })
    {
      _messageRepository.DeleteMessage(message);
    }

    if (await _messageRepository.SaveAllAsync()) return Ok();

    return BadRequest("Problem deleting message");
  }
}
