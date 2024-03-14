using System.Security.Claims;
using backend.Core.DBContext;
using backend.Core.Dtos.Log;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Core.Services;

// This service is responsible for user logs, and it implements the ILogService interface.
public class LogService : ILogService
{
  private readonly WebApplicationDBContext _context;

  public LogService(WebApplicationDBContext context)
  {
    _context = context;
  }

  public async Task<IEnumerable<GetLogDto>> GetLogsAsync()
  {
    var logs = await _context.Logs.Select(log => new GetLogDto
    {
      UserName = log.UserName,
      Description = log.Description,
      CreatedAt = log.CreatedAt,
    }).OrderByDescending(log => log.CreatedAt).ToListAsync();

    return logs;
  }

  public async Task<IEnumerable<GetLogDto>> GetMyLogsAsync(ClaimsPrincipal User)
  {
    var logs = await _context.Logs
    .Where(log => log.UserName == User.Identity.Name)
    .Select(log => new GetLogDto
    {
      UserName = log.UserName,
      Description = log.Description,
      CreatedAt = log.CreatedAt,
    }).OrderByDescending(log => log.CreatedAt).ToListAsync();

    return logs;
  }

  public async Task SaveNewLogAsync(string UserName, string Description)
  {
    var newLog = new Log
    {
      UserName = UserName,
      Description = Description,
    };

    await _context.Logs.AddAsync(newLog);
    await _context.SaveChangesAsync();
  }
}
