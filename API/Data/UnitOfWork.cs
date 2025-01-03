using API.Interfaces;

namespace API.Data;

public class UnitOfWork(
  DataContext dataContext,
  IUserRepository userRepository,
  IMessageRepository messageRepository,
  ILikesRepository likesRepository
) : IUnitOfWork
{
  public IUserRepository UserRepository => userRepository;

  public IMessageRepository MessageRepository => messageRepository;

  public ILikesRepository LikesRepository => likesRepository;

  public async Task<bool> Complete()
  {
    return await dataContext.SaveChangesAsync() > 0;
  }

  public bool HasChanges()
  {
    return dataContext.ChangeTracker.HasChanges();
  }
}
