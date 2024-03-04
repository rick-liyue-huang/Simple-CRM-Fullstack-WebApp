using System.Security.Claims;
using backend.Core.Dtos.Log;

namespace backend.Interfaces;

public interface ILogService
{
  Task SaveNewLog(string UserName, string Description);

  Task<IEnumerable<GetLogDto>> GetLogsAsync();

  Task<IEnumerable<GetLogDto>> GetMyLogsAsync(ClaimsPrincipal User);
}
