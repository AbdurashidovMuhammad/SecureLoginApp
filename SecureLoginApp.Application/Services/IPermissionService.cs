using SecureLoginApp.Application.Models;
using SecureLoginApp.Application.Models.Permissions;
using SecureLoginApp.Application.Security;
using SecureLoginApp.Application.Security.AuthEnums;

namespace SecureLoginApp.Application.Services;

public interface IPermissionService
{
    List<PermissionCodeDescription> GetAllPermissionDescriptions();
    string GetPermissionShortName(ApplicationPermissionCode permissionCode);
    Task<ApiResult<List<PermissionGroupListModel>>> GetPermissionsFromDbAsync();
}
