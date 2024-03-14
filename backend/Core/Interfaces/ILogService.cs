using System.Security.Claims;
using backend.Core.Dtos.Log;

namespace backend.Core.Interfaces;

public interface ILogService
{
  Task SaveNewLogAsync(string UserName, string Description);

  Task<IEnumerable<GetLogDto>> GetLogsAsync();

  Task<IEnumerable<GetLogDto>> GetMyLogsAsync(ClaimsPrincipal User);


}
