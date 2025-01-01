using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext(DbContextOptions options) : IdentityDbContext<
  AppUser,
  AppRole,
  int,
  IdentityUserClaim<int>,
  AppUserRole,
  IdentityUserLogin<int>,
  IdentityRoleClaim<int>,
  IdentityUserToken<int>
>(options)
{
  public DbSet<UserLike> Likes { get; set; }
  public DbSet<Message> Messages { get; set; }
  public DbSet<Group> Groups { get; set; }
  public DbSet<Connection> Connections { get; set; }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);

    builder.Entity<AppUser>()
      .HasMany(u => u.UserRoles)
      .WithOne(ur => ur.User)
      .HasForeignKey(ur => ur.UserId)
      .IsRequired();

    builder.Entity<AppRole>()
      .HasMany(ur => ur.UserRoles)
      .WithOne(u => u.Role)
      .HasForeignKey(ur => ur.RoleId)
      .IsRequired();

    builder.Entity<UserLike>()
      .HasKey(k => new { k.SourceUserId, k.TargetUserId });

    builder.Entity<UserLike>()
      .HasOne(s => s.SourceUser)
      .WithMany(l => l.LikedUsers)
      .HasForeignKey(s => s.SourceUserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Entity<UserLike>()
      .HasOne(s => s.TargetUser)
      .WithMany(l => l.LikedByUsers)
      .HasForeignKey(s => s.TargetUserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Entity<Message>()
      .HasOne(m => m.Recipient)
      .WithMany(u => u.MessagesReceived)
      .OnDelete(DeleteBehavior.Restrict);

    builder.Entity<Message>()
      .HasOne(m => m.Sender)
      .WithMany(u => u.MessagesSent)
      .OnDelete(DeleteBehavior.Restrict);
  }
}