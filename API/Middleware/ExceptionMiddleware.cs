using System.Net;
using System.Text.Json;
using API.Errors;

namespace API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
{
  public async Task InvokeAsync(HttpContext ctx)
  {
    try
    {
      await next(ctx);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, ex.Message);
      ctx.Response.ContentType = "application/json";
      ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

      var response = env.IsDevelopment()
        ? new ApiException(ctx.Response.StatusCode, ex.Message, ex.StackTrace)
        : new ApiException(ctx.Response.StatusCode, ex.Message, "Internal server error");

      var options = new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };

      var json = JsonSerializer.Serialize(response);

      await ctx.Response.WriteAsync(json);
    }
  }
}
