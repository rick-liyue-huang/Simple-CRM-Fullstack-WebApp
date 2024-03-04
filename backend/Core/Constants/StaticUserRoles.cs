namespace backend.Core.Constants;

public static class StaticUserRoles
{
  public const string OWNER = "OWNER";

  public const string ADMIN = "ADMIN";

  public const string USER = "USER";

  public const string MANAGER = "MANAGER";


  public const string OWNER_OR_ADMIN = "OWNER,ADMIN";
  public const string OWNER_OR_ADMIN_OR_MANAGER = "OWNER,ADMIN,MANAGER";

  public const string OWNER_OR_ADMIN_OR_MANAGER_OR_USER = "OWNER,ADMIN,MANAGER,USER";
}
