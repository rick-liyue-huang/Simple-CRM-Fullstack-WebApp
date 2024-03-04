namespace backend.Core.Dtos.Auth;

public class LoginResponseDto
{
  public string NewToken { get; set; } = string.Empty;

  // return to the client
  public UserInfoResult UserInfo { get; set; } = new UserInfoResult();

}
