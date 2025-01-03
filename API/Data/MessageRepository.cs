using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository(DataContext dataContext, IMapper mapper) : IMessageRepository
{
  private readonly DataContext _dataContext = dataContext;

  public void AddMessage(Message message)
  {
    _dataContext.Messages.Add(message);
  }

  public void DeleteMessage(Message message)
  {
    _dataContext.Messages.Remove(message);
  }

  public async Task<Message?> GetMessage(int id)
  {
    return await _dataContext.Messages.FindAsync(id);
  }

  public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
  {
    var query = _dataContext.Messages
      .OrderByDescending((m) => m.Sent)
      .AsQueryable();

    query = messageParams.Container switch
    {
      "Inbox" => query.Where(x => x.Recipient.UserName == messageParams.Username && !x.RecipientDeleted),
      "Outbox" => query.Where(x => x.Sender.UserName == messageParams.Username && !x.SenderDeleted),
      _ => query.Where(x => x.Recipient.UserName == messageParams.Username && x.DateRead == null && !x.RecipientDeleted)
    };

    var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

    return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
  }

  public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
  {
    var messagesQuery = _dataContext.Messages
      .Where(x =>
        x.RecipientUsername == currentUsername && !x.RecipientDeleted && x.SenderUsername == recipientUsername ||
        x.RecipientUsername == recipientUsername && !x.SenderDeleted && x.SenderUsername == currentUsername
      )
      .OrderBy(x => x.Sent)
      .AsQueryable();

    var unreadMessages = messagesQuery
      .Where(x => x.DateRead == null && x.RecipientUsername == currentUsername)
      .ToList();

    if (unreadMessages.Count != 0)
    {
      unreadMessages.ForEach(x => x.DateRead = DateTime.UtcNow);
    }

    return await messagesQuery.ProjectTo<MessageDto>(mapper.ConfigurationProvider).ToListAsync();
  }

  public void AddGroup(Group group)
  {
    _dataContext.Groups.Add(group);
  }

  public async Task<Connection?> GetConnection(string connectionId)
  {
    return await _dataContext.Connections.FindAsync(connectionId);
  }

  public async Task<Group?> GetMessageGroup(string groupName)
  {
    return await _dataContext.Groups
      .Include(x => x.Connections)
      .FirstOrDefaultAsync(x => x.Name == groupName);
  }

  public void RemoveConnection(Connection connection)
  {
    _dataContext.Connections.Remove(connection);
  }

  public async Task<Group?> GetGroupForConnection(string connectionId)
  {
    return await _dataContext.Groups
      .Include(g => g.Connections)
      .Where(g => g.Connections.Any(c => c.ConnectionId == connectionId))
      .FirstOrDefaultAsync();
  }
}
