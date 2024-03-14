using System.Security.Claims;
using backend.Core.Dtos.General;
using backend.Core.Dtos.Message;

namespace backend.Core.Interfaces;

public interface IMessageService
{
  Task<GeneralServiceResponseDto> CreateNewMessageAsync(ClaimsPrincipal User, CreateMessageDto createMessageDto);

  Task<IEnumerable<GetMessageDto>> GetMessagesAsync();

  Task<IEnumerable<GetMessageDto>> GetMyMessagesAsync(ClaimsPrincipal User);
}
