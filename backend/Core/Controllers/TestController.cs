using backend.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Core.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
  [HttpGet]
  [Route("get-public")]
  public IActionResult GetPublicData()
  {
    return Ok("Public data");
  }

  [HttpGet]
  [Route("get-user-role")]
  [Authorize(Roles = StaticUserRoles.USER)]
  public IActionResult GetUserRoleData()
  {
    return Ok("User role data");
  }

  [HttpGet]
  [Route("get-manager-role")]
  [Authorize(Roles = StaticUserRoles.MANAGER)]
  public IActionResult GetManagerRoleData()
  {
    return Ok("Manager role data");
  }

  [HttpGet]
  [Route("get-admin-role")]
  [Authorize(Roles = StaticUserRoles.ADMIN)]
  public IActionResult GetAdminRoleData()
  {
    return Ok("Admin role data");
  }

  [HttpGet]
  [Route("get-owner-role")]
  [Authorize(Roles = StaticUserRoles.OWNER)]
  public IActionResult GetOwnerRoleData()
  {
    return Ok("Owner role data");
  }

}
