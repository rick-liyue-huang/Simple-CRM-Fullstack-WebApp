using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Core.Constants;
using backend.Core.DBContext;
using backend.Core.Dtos.Auth;
using backend.Core.Dtos.General;
using backend.Core.Entities;
using backend.Interfaces;
using backend.Interfaces.IAuthService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services;

public class AuthService : IAuthService
{

  private readonly UserManager<ApplicationUser> _userManager;
  private readonly ApplicationDBContext _context;
  private readonly RoleManager<IdentityRole> _roleManager;
  private readonly ILogService _logService;
  private readonly IConfiguration _configuration;

  public AuthService(ApplicationDBContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogService logService, IConfiguration configuration)
  {
    _context = context;
    _userManager = userManager;
    _roleManager = roleManager;
    _logService = logService;
    _configuration = configuration;
  }


  public async Task<UserInfoResult?> GetUserDetailsByUserName(string userName)
  {
    var user = await _userManager.FindByNameAsync(userName);

    if (user == null)
    {
      return null;
    }

    var roles = await _userManager.GetRolesAsync(user);
    var userInfo = GenerateUserInfo(user, roles);

    return userInfo;
  }

  public async Task<IEnumerable<UserInfoResult>> GetUserListAsync()
  {
    var users = await _userManager.Users.ToListAsync();
    List<UserInfoResult> userInfoList = new List<UserInfoResult>();

    foreach (var user in users)
    {
      var roles = await _userManager.GetRolesAsync(user);
      var userInfo = GenerateUserInfo(user, roles);
      userInfoList.Add(userInfo);
    }

    return userInfoList;
  }

  public async Task<IEnumerable<string>> GetUsernamesListAsync()
  {
    var userNames = await _userManager.Users.Select(u => u.UserName).ToListAsync();
    // var users = await _userManager.Users.ToListAsync();
    // List<string> usernames = new List<string>();

    // foreach (var user in users)
    // {
    //   usernames.Add(user.UserName);
    // }

    return userNames;
  }

  public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
  {
    var user = await _userManager.FindByNameAsync(loginDto.UserName);

    if (user == null)
    {
      return null;
    }

    // CheckPasswordAsync method is used to verify the password of the user
    var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);

    if (!isPasswordValid)
    {
      return null;
    }

    // return token and userInfo, if user is valid, to client
    var newToken = await GenerateJWTTokenAsync(user);
    var roles = await _userManager.GetRolesAsync(user);
    var userInfo = GenerateUserInfo(user, roles);

    await _logService.SaveNewLog(user.UserName, "User logged in successfully");

    return new LoginResponseDto
    {
      NewToken = newToken,
      UserInfo = userInfo
    };
  }

  public async Task<LoginResponseDto?> MeAsync(MeDto meDto)
  {
    ClaimsPrincipal handler = new JwtSecurityTokenHandler().ValidateToken(meDto.Token, new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidateAudience = true,
      // ValidateIssuerSigningKey = true,
      ValidIssuer = _configuration["JWT:ValidIssuer"],
      ValidAudience = _configuration["JWT:ValidAudience"],
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]))
    }, out SecurityToken validatedToken);

    string decodedToken = handler.Claims.First(claim => claim.Type == ClaimTypes.Name).Value;

    if (decodedToken == null)
    {
      return null;
    }

    var user = await _userManager.FindByNameAsync(decodedToken);

    if (user == null)
    {
      return null;
    }

    var newToken = await GenerateJWTTokenAsync(user);
    var roles = await _userManager.GetRolesAsync(user);
    var userInfo = GenerateUserInfo(user, roles);
    await _logService.SaveNewLog(user.UserName, "User Me Token Generated");

    return new LoginResponseDto
    {
      NewToken = newToken,
      UserInfo = userInfo
    };

  }

  public async Task<GeneralServiceResponseDto> RegisterAsync(RegisterDto registerDto)
  {
    var isExistingUser = _userManager.FindByNameAsync(registerDto.UserName);

    if (isExistingUser != null)
    {
      return new GeneralServiceResponseDto
      {
        IsSucceed = false,
        StatusCode = 400,
        Message = "User already exists"
      };
    }

    ApplicationUser newUser = new ApplicationUser
    {
      UserName = registerDto.UserName,
      Email = registerDto.Email,
      FirstName = registerDto.FirstName,
      LastName = registerDto.LastName,
      Address = registerDto.Address,
      // Guid is used to generate a random string
      SecurityStamp = Guid.NewGuid().ToString()
    };

    var result = await _userManager.CreateAsync(newUser, registerDto.Password);

    if (!result.Succeeded)
    {
      var errorMsg = "User creation failed, error: ";
      foreach (var error in result.Errors)
      {
        errorMsg += error.Description;
      }

      return new GeneralServiceResponseDto
      {
        IsSucceed = false,
        StatusCode = 400,
        Message = errorMsg
      };

    }

    // add a default USER role to the new user
    await _userManager.AddToRoleAsync(newUser, StaticUserRoles.USER);
    await _logService.SaveNewLog(newUser.UserName, "User created successfully");

    return new GeneralServiceResponseDto
    {
      IsSucceed = true,
      StatusCode = 201,
      Message = "User created successfully"
    };

  }

  public async Task<GeneralServiceResponseDto> SeedRolesAsync()
  {
    bool isOwnerRoleExist = await _roleManager.RoleExistsAsync(StaticUserRoles.OWNER);
    bool isAdminRoleExist = await _roleManager.RoleExistsAsync(StaticUserRoles.ADMIN);
    bool isUserRoleExist = await _roleManager.RoleExistsAsync(StaticUserRoles.USER);
    bool isManagerRoleExist = await _roleManager.RoleExistsAsync(StaticUserRoles.MANAGER);

    if (isOwnerRoleExist && isAdminRoleExist && isManagerRoleExist && isUserRoleExist)
    {
      return new GeneralServiceResponseDto
      {
        IsSucceed = true,
        StatusCode = 200,
        Message = "Roles are seeded"
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
      Message = "Roles are seeded successfully already."
    };

  }

  public async Task<GeneralServiceResponseDto> UpdateRoleAsync(ClaimsPrincipal User, UpdateRoleDto updateRoleDto)
  {
    var user = await _userManager.FindByNameAsync(updateRoleDto.UserName);

    if (user == null)
    {
      return new GeneralServiceResponseDto
      {
        IsSucceed = false,
        StatusCode = 404,
        Message = "User not found"
      };
    }

    var userRoles = await _userManager.GetRolesAsync(user);
    // only owner and admin can update roles
    if (User.IsInRole(StaticUserRoles.ADMIN))
    {
      if (updateRoleDto.NewRole == RoleType.USER || updateRoleDto.NewRole == RoleType.MANAGER)
      {
        // admin role can change user and manager roles
        if (userRoles.Any(u => u.Equals(StaticUserRoles.OWNER) || u.Equals(StaticUserRoles.ADMIN)))
        {
          return new GeneralServiceResponseDto
          {
            IsSucceed = false,
            StatusCode = 403,
            Message = "You are not authorized to change the role of this user"
          };
        }
        else
        {
          await _userManager.RemoveFromRolesAsync(user, userRoles);
          await _userManager.AddToRoleAsync(user, updateRoleDto.NewRole.ToString());
          await _logService.SaveNewLog(user.UserName, "Role updated successfully");
          return new GeneralServiceResponseDto
          {
            IsSucceed = true,
            StatusCode = 200,
            Message = "Role updated successfully"
          };
        }
      }
      else
      {
        return new GeneralServiceResponseDto
        {
          IsSucceed = false,
          StatusCode = 403,
          Message = "Invalid role"
        };
      }


    }

    else
    {
      // user is owner
      if (userRoles.Any(u => u.Equals(StaticUserRoles.OWNER)))
      {
        return new GeneralServiceResponseDto
        {
          IsSucceed = false,
          StatusCode = 403,
          Message = "You are not authorized to change the role of this user"
        };
      }
      else
      {
        await _userManager.RemoveFromRolesAsync(user, userRoles);
        await _userManager.AddToRoleAsync(user, updateRoleDto.NewRole.ToString());
        await _logService.SaveNewLog(user.UserName, "Role updated successfully");
        return new GeneralServiceResponseDto
        {
          IsSucceed = true,
          StatusCode = 200,
          Message = "Role OWNER updated successfully"
        };
      }
    }
  }


  private async Task<string> GenerateJWTTokenAsync(ApplicationUser user)
  {
    var userRoles = await _userManager.GetRolesAsync(user);

    var claims = new List<Claim>
    {
      new Claim(ClaimTypes.NameIdentifier, user.Id),
      new Claim(ClaimTypes.Name, user.UserName),
      new Claim("FirstName", user.FirstName),
      new Claim("LastName", user.LastName),
    };

    foreach (var userRole in userRoles)
    {
      claims.Add(new Claim(ClaimTypes.Role, userRole));
    }

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
      issuer: _configuration["JWT:ValidIssuer"],
      audience: _configuration["WT:ValidAudience"],
      notBefore: DateTime.Now,
      claims: claims,
      expires: DateTime.Now.AddHours(2),
      signingCredentials: creds
    );

    string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    return tokenString;
  }

  private UserInfoResult GenerateUserInfo(ApplicationUser user, IEnumerable<string> roles)
  {
    return new UserInfoResult
    {
      Id = user.Id,
      UserName = user.UserName,
      FirstName = user.FirstName,
      LastName = user.LastName,
      Email = user.Email,
      CreatedAt = user.CreatedAt,
      Roles = roles
    };
  }
}
