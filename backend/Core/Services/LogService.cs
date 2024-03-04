using System.Security.Claims;
using backend.Core.DBContext;
using backend.Core.Dtos.Log;
using backend.Core.Entities;
using backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class LogService : ILogService
{

  private readonly ApplicationDBContext _context;

  public LogService(ApplicationDBContext context)
  {
    _context = context;
  }

  public async Task<IEnumerable<GetLogDto>> GetLogsAsync()
  {
    var logs = await _context.Logs.Select(log => new GetLogDto
    {
      UserName = log.UserName,
      Description = log.Description,
      CreatedAt = log.CreatedAt
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
      CreatedAt = log.CreatedAt
    }).OrderByDescending(log => log.CreatedAt).ToListAsync();

    return logs;
  }

  // this is the method that will save the log to the database, and it will be called from the controller, this is async because it will be saving to the database.
  public async Task SaveNewLog(string UserName, string Description)
  {
    var log = new Log
    {
      UserName = UserName,
      Description = Description,
      CreatedAt = DateTime.Now
    };

    await _context.Logs.AddAsync(log);
    await _context.SaveChangesAsync();
  }
}
