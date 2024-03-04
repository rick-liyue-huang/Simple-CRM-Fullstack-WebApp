using System.Security.Claims;
using backend.Core.DBContext;
using backend.Core.Dtos.General;
using backend.Core.Dtos.Message;
using backend.Core.Entities;
using backend.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class MessageService : IMessageService
{

  private readonly ApplicationDBContext _context;
  private readonly ILogService _logService;
  private readonly UserManager<ApplicationUser> _userManager;

  public MessageService(ApplicationDBContext context, ILogService logService, UserManager<ApplicationUser> userManager)
  {
    _context = context;
    _logService = logService;
    _userManager = userManager;
  }


  public async Task<GeneralServiceResponseDto> CreateNewMessageAsync(ClaimsPrincipal User, CreateMessageDto createMessageDto)
  {
    if (User.Identity.Name == createMessageDto.ReceiverUserName)
    {
      return new GeneralServiceResponseDto
      {
        IsSucceed = false,
        StatusCode = 400,
        Message = "You can't send a message to yourself"
      };
    }

    var isReceiverUserNameValid = _userManager.Users.Any(user => user.UserName == createMessageDto.ReceiverUserName);

    if (!isReceiverUserNameValid)
    {
      return new GeneralServiceResponseDto
      {
        IsSucceed = false,
        StatusCode = 400,
        Message = "The receiver username is not valid"
      };
    }

    Message newMessage = new Message
    {
      SenderUserName = User.Identity.Name,
      ReceiverUserName = createMessageDto.ReceiverUserName,
      Text = createMessageDto.Text,
    };

    await _context.Messages.AddAsync(newMessage);
    await _context.SaveChangesAsync();
    await _logService.SaveNewLog(User.Identity.Name, "Sent a new message to " + createMessageDto.ReceiverUserName);

    return new GeneralServiceResponseDto
    {
      IsSucceed = true,
      StatusCode = 200,
      Message = "Message sent successfully"
    };

  }

  public async Task<IEnumerable<GetMessageDto>> GetMessagesAsync()
  {
    var messages = await _context.Messages.Select(message => new GetMessageDto
    {
      Id = message.Id,
      SenderUserName = message.SenderUserName,
      ReceiverUserName = message.ReceiverUserName,
      Text = message.Text,
      CreatedAt = message.CreatedAt
    }).OrderByDescending(message => message.CreatedAt).ToListAsync();

    return messages;
  }

  public async Task<IEnumerable<GetMessageDto>> GetMyMessagesAsync(ClaimsPrincipal User)
  {
    var messages = await _context.Messages
    .Where(message => message.ReceiverUserName == User.Identity.Name || message.SenderUserName == User.Identity.Name)
    .Select(message => new GetMessageDto
    {
      Id = message.Id,
      SenderUserName = message.SenderUserName,
      ReceiverUserName = message.ReceiverUserName,
      Text = message.Text,
      CreatedAt = message.CreatedAt
    }).OrderByDescending(message => message.CreatedAt).ToListAsync();

    return messages;
  }
}
