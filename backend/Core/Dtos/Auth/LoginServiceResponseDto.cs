using backend.Core.Dtos.Auth;

namespace backend;

public class LoginServiceResponseDto
{
  public string NewToken { get; set; }

  // This would be returned to front-end
  public UserInfoResultDto UserInfo { get; set; }
}