using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  public class BuggyController(DataContext dataContext) : BaseApiController
  {
    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetAuth()
    {
      return "secret text";
    }

    [HttpGet("not-found")]
    public ActionResult<string> GetNotFound()
    {
      return NotFound();
    }

    [HttpGet("server-error")]
    public ActionResult<string> GetServerError()
    {
      var thing = dataContext.Users.Find(-1) ?? throw new Exception("Bad thing has happened");
      return "secret text";
    }

    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest()
    {
      return BadRequest("This was not a good request");
    }
  }
}