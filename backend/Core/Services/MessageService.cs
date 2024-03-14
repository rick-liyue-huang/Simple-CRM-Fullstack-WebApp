using System.Security.Claims;
using backend.Core.DBContext;
using backend.Core.Dtos.General;
using backend.Core.Dtos.Message;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Core.Services;

public class MessageService : IMessageService
{

  private readonly WebApplicationDBContext _context;
  private readonly ILogService _logService;
  private readonly UserManager<WebApplicationUser> _userManager;

  public MessageService(WebApplicationDBContext context, ILogService logService, UserManager<WebApplicationUser> userManager)
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
        Message = "You can't send a message to yourself",
      };
    }

    var isReceiverUserNameValid = _userManager.Users.Any(user => user.UserName == createMessageDto.ReceiverUserName);

    if (!isReceiverUserNameValid)
    {
      return new GeneralServiceResponseDto
      {
        IsSucceed = false,
        StatusCode = 400,
        Message = "Receiver user name is not valid",
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
    await _logService.SaveNewLogAsync(User.Identity.Name, "New message sent to " + createMessageDto.ReceiverUserName);

    return new GeneralServiceResponseDto
    {
      IsSucceed = true,
      StatusCode = 201,
      Message = "Message sent successfully",
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
      CreatedAt = message.CreatedAt,
    }).OrderByDescending(message => message.CreatedAt).ToListAsync();

    return messages;
  }

  public async Task<IEnumerable<GetMessageDto>> GetMyMessagesAsync(ClaimsPrincipal User)
  {
    var LoggedInUser = User.Identity.Name;
    var messages = await _context.Messages
    .Where(message => message.SenderUserName == LoggedInUser || message.ReceiverUserName == LoggedInUser)
    .Select(message => new GetMessageDto
    {
      Id = message.Id,
      SenderUserName = message.SenderUserName,
      ReceiverUserName = message.ReceiverUserName,
      Text = message.Text,
      CreatedAt = message.CreatedAt,
    }).OrderByDescending(message => message.CreatedAt).ToListAsync();

    return messages;
  }
}
