namespace SecureLoginApp.Application.Security;

// Ensure there is only one definition of PermissionCodeDescription in this namespace.  
public class PermissionCodeDescription
{
    internal PermissionCodeDescription()
    { }

    public string Code { get; set; }        // Enum a'zosining nomi (masalan, "UserCreate")  
    public string ShortName { get; set; }   // Atributdagi qisqa nom (masalan, "User Create")  
    public string FullName { get; set; }
}
