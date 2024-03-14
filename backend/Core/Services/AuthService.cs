using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Core.Constants;
using backend.Core.Dtos.Auth;
using backend.Core.Dtos.General;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Core.Services;

public class AuthService : IAuthService
{

  private readonly UserManager<WebApplicationUser> _userManager;
  private readonly RoleManager<IdentityRole> _roleManager;
  private readonly ILogService _logService;
  private readonly IConfiguration _configuration;


  public AuthService(UserManager<WebApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogService logService, IConfiguration configuration)
  {
    _userManager = userManager;
    _roleManager = roleManager;
    _logService = logService;
    _configuration = configuration;
  }


  public async Task<UserInfoResultDto?> GetUserDetailsByUserNameAsync(string userName)
  {
    var user = await _userManager.FindByNameAsync(userName);
    if (user is null)
    {
      return null;
    }

    var roles = await _userManager.GetRolesAsync(user);
    return GenerateUserInfoObject(user, roles);
  }

  public async Task<IEnumerable<string>> GetUsernamesListAsync()
  {
    var userNames = await _userManager.Users.Select(u => u.UserName).ToListAsync();

    /*
    List<string> usernamesList = new List<string>();

    foreach (var user in users)
    {
      usernamesList.Add(user.UserName);
    }
    */

    return userNames;
  }

  public async Task<IEnumerable<UserInfoResultDto>> GetUsersListAsync()
  {
    var users = await _userManager.Users.ToListAsync();
    List<UserInfoResultDto> usersList = new List<UserInfoResultDto>();

    foreach (var user in users)
    {
      var roles = await _userManager.GetRolesAsync(user);
      usersList.Add(GenerateUserInfoObject(user, roles));
    }

    return usersList;
  }

  public async Task<LoginServiceResponseDto?> LoginAsync(LoginDto loginDto)
  {
    // find the user by username
    var user = await _userManager.FindByNameAsync(loginDto.UserName);
    if (user is null)
    {
      return null;
    }

    // check if the user has the correct password
    var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);
    if (!isPasswordCorrect)
    {
      return null;
    }

    // get the user roles
    var userRoles = await _userManager.GetRolesAsync(user);

    // generate the token
    var newToken = await GenerateJWTTokenAsync(user);
    var roles = await _userManager.GetRolesAsync(user);
    var userInfo = GenerateUserInfoObject(user, roles);

    await _logService.SaveNewLogAsync(user.UserName, "New User Logged in to WebApplication");

    return new LoginServiceResponseDto
    {
      NewToken = newToken,
      UserInfo = userInfo,
    };
  }

  public async Task<LoginServiceResponseDto?> MeAsync(MeDto meDto)
  {
    ClaimsPrincipal handle = new JwtSecurityTokenHandler().ValidateToken(meDto.Token, new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidateAudience = true,
      // ValidateLifetime = true,
      // ValidateIssuerSigningKey = true,
      ValidIssuer = _configuration["JWT:ValidIssuer"],
      ValidAudience = _configuration["JWT:ValidAudience"],
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
    }, out SecurityToken validatedToken);

    string decodedUserName = handle.Claims.First(c => c.Type == ClaimTypes.Name).Value;

    if (decodedUserName is null)
    {
      return null;
    }

    var user = await _userManager.FindByNameAsync(decodedUserName);
    if (user is null)
    {
      return null;
    }

    var newToken = await GenerateJWTTokenAsync(user);
    var roles = await _userManager.GetRolesAsync(user);
    var userInfo = GenerateUserInfoObject(user, roles);
    await _logService.SaveNewLogAsync(user.UserName, "User refreshed token, and Get Me details");

    return new LoginServiceResponseDto
    {
      NewToken = newToken,
      UserInfo = userInfo,
    };
  }

  public async Task<GeneralServiceResponseDto> RegisterAsync(RegisterDto registerDto)
  {
    var isExistingUser = await _userManager.FindByNameAsync(registerDto.UserName);

    if (isExistingUser is not null)
    {
      return new GeneralServiceResponseDto
      {
        IsSucceed = false,
        StatusCode = 409,
        Message = "User already exist",
      };
    }

    WebApplicationUser newUser = new WebApplicationUser
    {
      UserName = registerDto.UserName,
      Email = registerDto.Email,
      FirstName = registerDto.FirstName,
      LastName = registerDto.LastName,
      Address = registerDto.Address,
      SecurityStamp = Guid.NewGuid().ToString(),
    };

    var createUserResult = await _userManager.CreateAsync(newUser, registerDto.Password);

    if (!createUserResult.Succeeded)
    {
      var errorMessages = "User creation failed";
      foreach (var error in createUserResult.Errors)
      {
        errorMessages += "# " + error.Description;
      }
      return new GeneralServiceResponseDto
      {
        IsSucceed = false,
        StatusCode = 400,
        Message = errorMessages,
      };
    }

    // add a default role to the user
    await _userManager.AddToRoleAsync(newUser, StaticUserRoles.USER);
    await _logService.SaveNewLogAsync(newUser.UserName, "Registered to WebApplication");

    return new GeneralServiceResponseDto
    {
      IsSucceed = true,
      StatusCode = 201,
      Message = "User registered successfully",
    };

  }

  public async Task<GeneralServiceResponseDto> SeedRolesAsync()
  {
    bool isOwnerRoleExist = await _roleManager.RoleExistsAsync(StaticUserRoles.OWNER);
    bool isAdminRoleExist = await _roleManager.RoleExistsAsync(StaticUserRoles.ADMIN);
    bool isManagerRoleExist = await _roleManager.RoleExistsAsync(StaticUserRoles.MANAGER);
    bool isUserRoleExist = await _roleManager.RoleExistsAsync(StaticUserRoles.USER);

    if (!isOwnerRoleExist && !isAdminRoleExist && !isManagerRoleExist && !isUserRoleExist)
    {
      return new GeneralServiceResponseDto
      {
        IsSucceed = true,
        StatusCode = 200,
        Message = "Roles already exist",
      };
    }

    await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.OWNER));
    await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.ADMIN));
    await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.MANAGER));
    await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.USER));

    return new GeneralServiceResponseDto
    {
      IsSucceed = true,
      StatusCode = 201,
      Message = "Roles seeded successfully",
    };
  }

  public async Task<GeneralServiceResponseDto> UpdateRoleAsync(ClaimsPrincipal User, UpdateRoleDto updateRoleDto)
  {
    var user = await _userManager.FindByNameAsync(updateRoleDto.UserName);
    if (user is null)
    {
      return new GeneralServiceResponseDto
      {
        IsSucceed = false,
        StatusCode = 404,
        Message = "Invalid Username or User not found",
      };
    }

    var userRoles = await _userManager.GetRolesAsync(user);

    //  only OWNER and ADMIN can update roles
    // user is admin
    if (User.IsInRole(StaticUserRoles.ADMIN))
    {
      if (updateRoleDto.NewRole == RoleType.USER || updateRoleDto.NewRole == RoleType.MANAGER)
      {
        // admin can change the role of everyone except OWNER and ADMIN
        if (userRoles.Any(r => r.Equals(StaticUserRoles.OWNER) || r.Equals(StaticUserRoles.ADMIN)))
        {
          return new GeneralServiceResponseDto
          {
            IsSucceed = false,
            StatusCode = 403,
            Message = "You are not allowed to update this user's role",
          };
        }

        else
        {
          await _userManager.RemoveFromRoleAsync(user, userRoles.FirstOrDefault()); // TODO:?
          await _userManager.AddToRoleAsync(user, updateRoleDto.NewRole.ToString());
          await _logService.SaveNewLogAsync(user.UserName, "Role updated to " + updateRoleDto.NewRole);
          return new GeneralServiceResponseDto
          {
            IsSucceed = true,
            StatusCode = 200,
            Message = "Role updated successfully",
          };
        }
      }

      else
      {
        return new GeneralServiceResponseDto
        {
          IsSucceed = false,
          StatusCode = 403,
          Message = "You are not allowed to update this user's role to OWNER or ADMIN",
        };
      }
    }

    else
    {
      // user is owner
      if (userRoles.Any(r => r.Equals(StaticUserRoles.OWNER)))
      {
        return new GeneralServiceResponseDto
        {
          IsSucceed = false,
          StatusCode = 403,
          Message = "You are not allowed to update this user's role",
        };
      }
      else
      {
        await _userManager.RemoveFromRoleAsync(user, userRoles.FirstOrDefault()); // TODO:?
        await _userManager.AddToRoleAsync(user, updateRoleDto.NewRole.ToString());
        await _logService.SaveNewLogAsync(user.UserName, "Role updated to " + updateRoleDto.NewRole);
        return new GeneralServiceResponseDto
        {
          IsSucceed = true,
          StatusCode = 200,
          Message = "Role updated successfully",
        };
      }
    }
  }

  private async Task<string> GenerateJWTTokenAsync(WebApplicationUser user)
  {
    var userRoles = await _userManager.GetRolesAsync(user);

    var authClaims = new List<Claim>
    {
      new Claim(ClaimTypes.Name, user.UserName),
      new Claim(ClaimTypes.NameIdentifier, user.Id),
      new Claim("FirstName", user.FirstName),
      new Claim("LastName", user.LastName),
    };

    foreach (var userRole in userRoles)
    {
      authClaims.Add(new Claim(ClaimTypes.Role, userRole));
    }

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var tokenObject = new JwtSecurityToken(
      issuer: _configuration["Jwt:ValidIssuer"],
      audience: _configuration["Jwt:ValidAudience"],
      claims: authClaims,
      notBefore: DateTime.Now,
      expires: DateTime.Now.AddMinutes(120),
      signingCredentials: credentials
    );

    string tokenString = new JwtSecurityTokenHandler().WriteToken(tokenObject);
    return tokenString;
  }

  private async Task<string> GenerateJWTTokenAsync1(WebApplicationUser user)
  {
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
      new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
      new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      new Claim(JwtRegisteredClaimNames.Email, user.Email),
      new Claim("id", user.Id),
    };

    var token = new JwtSecurityToken(
      _configuration["Jwt:Issuer"],
      _configuration["Jwt:Audience"],
      claims,
      expires: DateTime.Now.AddMinutes(120),
      signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  private UserInfoResultDto GenerateUserInfoObject(WebApplicationUser user, IEnumerable<string> roles)
  {

    // AutoMapper can be used here
    return new UserInfoResultDto
    {
      Id = user.Id,
      UserName = user.UserName,
      Email = user.Email,
      FirstName = user.FirstName,
      LastName = user.LastName,
      CreatedAt = user.CreatedAt,
      Roles = roles,
    };
  }
}
