using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager) : BaseApiController
{
  [HttpGet("users-with-roles")]
  [Authorize(Policy = "RequireAdminRole")]
  public async Task<ActionResult> GetUsersWithRoles()
  {
    var users = await userManager.Users
      .OrderBy(x => x.UserName)
      .Select(x => new
      {
        x.Id,
        Username = x.UserName,
        Roles = x.UserRoles.Select(r => r.Role.Name).ToList()
      })
      .ToListAsync();

    return Ok(users);
  }

  [HttpPost("edit-roles/{username}")]
  [Authorize(Policy = "RequireAdminRole")]
  public async Task<ActionResult> EditRoles(string username, string roles)
  {
    if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

    var selectedRoles = roles.Split(",").ToArray();

    var user = await userManager.FindByNameAsync(username);

    if (user == null) return BadRequest("Unable to find user");

    var userRoles = await userManager.GetRolesAsync(user);

    var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

    if (!result.Succeeded) return BadRequest("Failed to add to roles");

    result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

    if (!result.Succeeded) return BadRequest("Failed to remove from roles");

    return Ok(await userManager.GetRolesAsync(user));
  }

  [HttpGet("photos-to-moderate")]
  [Authorize(Policy = "ModeratePhotoRole")]
  public ActionResult GetPhotosForModeration()
  {
    return Ok("Only moderators can see this");
  }
}
