using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Extensions;

public static class ClaimsPrincipalExtensions
{
  public static string GetUsername(this ClaimsPrincipal user)
  {
    return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("Cannot get username from token");
  }
}
