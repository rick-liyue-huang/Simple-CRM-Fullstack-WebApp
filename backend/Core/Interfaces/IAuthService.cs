using System.Security.Claims;
using backend.Core.Dtos.Auth;
using backend.Core.Dtos.General;

namespace backend.Core.Interfaces;

public interface IAuthService
{
  Task<GeneralServiceResponseDto> SeedRolesAsync();

  Task<GeneralServiceResponseDto> RegisterAsync(RegisterDto registerDto);

  Task<LoginServiceResponseDto?> LoginAsync(LoginDto loginDto);

  Task<GeneralServiceResponseDto> UpdateRoleAsync(ClaimsPrincipal User, UpdateRoleDto updateRoleDto);

  Task<LoginServiceResponseDto?> MeAsync(MeDto meDto);

  Task<IEnumerable<UserInfoResultDto>> GetUsersListAsync();

  Task<UserInfoResultDto?> GetUserDetailsByUserNameAsync(string userName);


  Task<IEnumerable<string>> GetUsernamesListAsync();


}
