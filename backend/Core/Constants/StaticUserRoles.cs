namespace backend.Core.Constants;


// the class will provide predefined roles type for the application
public static class StaticUserRoles
{
  public const string OWNER = "OWNER";
  public const string ADMIN = "ADMIN";

  public const string MANAGER = "MANAGER";

  public const string USER = "USER";

  public const string OWNER_OR_ADMIN = "OWNER,ADMIN";

  public const string OWNER_OR_ADMIN_OR_MANAGER = "OWNER,ADMIN,MANAGER";

  public const string OWNER_OR_ADMIN_OR_MANAGER_OR_USER = "OWNER,ADMIN,MANAGER,USER";

}
