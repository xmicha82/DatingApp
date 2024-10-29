namespace API.Helpers;

public class UserParams
{
  private const int MaxPageSize = 100;
  private int _pageSize = 10;

  public int PageNumber { get; set; }
  public int PageSize
  {
    get => _pageSize;
    set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
  }

  public string? Gender { get; set; }
  public string? CurrentUsername { get; set; }
  public int MinAge { get; set; } = 18;
  public int MaxAge { get; set; } = 100;

  public string OrderBy { get; set; } = "lastActive";
}
