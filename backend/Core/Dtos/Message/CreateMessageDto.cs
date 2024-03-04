namespace backend.Core.Dtos.Message;

public class CreateMessageDto
{
  public string ReceiverUserName { get; set; } = string.Empty;

  public string Text { get; set; } = string.Empty;

}
