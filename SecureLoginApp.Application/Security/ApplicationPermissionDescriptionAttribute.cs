using SecureLoginApp.Application.Security.AuthEnums;

namespace SecureLoginApp.Application.Security;

[AttributeUsage(AttributeTargets.Field)]
public class ApplicationPermissionDescriptionAttribute : Attribute
{
    public ApplicationPermissionDescriptionAttribute(ApplicationPermissionGroupCode modulePermissionGroup, string shortName, string fullName)
    {
        ModulePermissionGroup = modulePermissionGroup;
        ShortName = shortName;
        FullName = fullName;
    }

    public ApplicationPermissionDescriptionAttribute(ApplicationPermissionGroupCode moduleGroup, string shortFullName)
        : this(moduleGroup, shortFullName, shortFullName)
    { }

    public ApplicationPermissionGroupCode ModulePermissionGroup { get; private set; }
    public string FullName { get; private set; }
    public string ShortName { get; private set; }

    public string GetModulePermissionGroupName()
    {
        // Enum bo'lsa nomini, aks holda ToString() ni qaytaradi
        return ModulePermissionGroup.ToString();
    }
}

// Sizning bergan PermissionCodeDescription class'i
public class PermissionCodeDescription
{
    internal PermissionCodeDescription()
    { }

    public string Code { get; set; }
    public string ShortName { get; set; }
    public string FullName { get; set; }
}