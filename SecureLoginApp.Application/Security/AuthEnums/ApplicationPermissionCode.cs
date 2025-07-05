namespace SecureLoginApp.Application.Security.AuthEnums;

[ApplicationEnumDescription]
public enum ApplicationPermissionCode
{
    #region Document

    #region User

    [ApplicationPermissionDescription(ApplicationPermissionGroupCode.User, "User Create")]
    UserCreate,

    [ApplicationPermissionDescription(ApplicationPermissionGroupCode.User, "User Read")]
    UserRead,

    #endregion

    #region Permission

    [ApplicationPermissionDescription(ApplicationPermissionGroupCode.Permission, "Get Permissions")]
    GetPermissions,

    #endregion


    #endregion
}
