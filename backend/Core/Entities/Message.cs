namespace backend.Core.Entities;

public class Message : BaseEntity<long>
{
  public string SenderUserName { get; set; } = string.Empty;

  public string ReceiverUserName { get; set; } = string.Empty;

  public string Text { get; set; } = string.Empty;

}
