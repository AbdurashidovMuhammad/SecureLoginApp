using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureLoginApp.Core.Common;

public interface IApplicationPermission<TModulePermissionGroup>
where TModulePermissionGroup : class, IApplicationPermissionGroup
{
    string ShortName { get; set; }
    string FullName { get; set; }
    TModulePermissionGroup PermissionGroup { get; set; }
}
