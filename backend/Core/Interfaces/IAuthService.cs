using backend.Core.Dtos.Auth;
using backend.Core.Dtos.General;
using System.Security.Claims;

namespace backend.Interfaces.IAuthService;

public interface IAuthService
{
  Task<GeneralServiceResponseDto> SeedRolesAsync();

  Task<GeneralServiceResponseDto> RegisterAsync(RegisterDto registerDto);

  Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);

  // ClaimsPrincipal in .NET plays a crucial role, especially in handling authentication and authorization. It represents the security context of the current user, allowing access to the user's authentication information, including the user's identities (ClaimsIdentity) and the claims associated with those identities.
  Task<GeneralServiceResponseDto> UpdateRoleAsync(ClaimsPrincipal User, UpdateRoleDto updateRoleDto);

  Task<LoginResponseDto?> MeAsync(MeDto meDto);

  Task<IEnumerable<UserInfoResult>> GetUserListAsync();

  Task<UserInfoResult?> GetUserDetailsByUserName(string userName);

  Task<IEnumerable<string>> GetUsernamesListAsync();

}
