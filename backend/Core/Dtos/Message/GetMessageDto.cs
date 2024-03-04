namespace backend.Core.Dtos.Message;

public class GetMessageDto
{
  public long Id { get; set; }

  public string SenderUserName { get; set; } = string.Empty;

  public string ReceiverUserName { get; set; } = string.Empty;

  public string Text { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; } = DateTime.Now;
}
