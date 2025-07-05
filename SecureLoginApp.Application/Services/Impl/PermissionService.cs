using SecureLoginApp.Application.Security.AuthEnums;
using SecureLoginApp.Application.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecureLoginApp.Application.Models;
using SecureLoginApp.DataAcces.Persistence;
using SecureLoginApp.Application.Extensions;
using SecureLoginApp.Application.Models.Permissions;

namespace SecureLoginApp.Application.Services.Impl
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _dbContext;
        public PermissionService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public List<PermissionCodeDescription> GetAllPermissionDescriptions()
        {
            var permissions = new List<PermissionCodeDescription>();

            foreach (var field in typeof(ApplicationPermissionCode).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attribute = field.GetCustomAttribute<ApplicationPermissionDescriptionAttribute>();

                if (attribute != null)
                {
                    permissions.Add(new PermissionCodeDescription
                    {
                        Code = field.Name,
                        ShortName = attribute.ShortName,
                        FullName = attribute.FullName
                    });
                }
            }
            return permissions;
        }

        public string GetPermissionShortName(ApplicationPermissionCode permissionCode)
        {
            FieldInfo? field = typeof(ApplicationPermissionCode).GetField(permissionCode.ToString());

            if (field != null)
            {
                var attribute = field.GetCustomAttribute<ApplicationPermissionDescriptionAttribute>();
                if (attribute != null)
                {
                    return attribute.ShortName;
                }
            }

            return permissionCode.ToString();
        }

        public async Task<ApiResult<List<PermissionGroupListModel>>> GetPermissionsFromDbAsync()
        {
            // 1. Permissionlarni bazaga sinxronlash (qo'shish/yangilash)
            await _dbContext.ResolvePermissions();

            // 2. Bazadan barcha permissionlarni guruhlari bilan birga yuklash
            var permissions = await _dbContext.Permissions
                .Include(p => p.PermissionGroup)
                .AsNoTracking()
                .ToListAsync();

            // 3. Olingan permissionlarni DTOlarga map qilish va guruhlash
            var groupedPermissions = permissions
                .GroupBy(p => p.PermissionGroup.Name) // PermissionGroup.Name orqali guruhlash
                .Select(g => new PermissionGroupListModel
                {
                    GroupName = g.Key,
                    Permissions = g.Select(p => new PermissionListModel
                    {
                        Id = p.Id,
                        ShortName = p.ShortName, // Permission.ShortName
                        FullName = p.FullName,   // Permission.FullName
                        GroupName = p.PermissionGroup.Name // PermissionGroup.Name
                    }).OrderBy(p => p.ShortName).ToList()
                })
                .OrderBy(pg => pg.GroupName)
                .ToList();

            return ApiResult<List<PermissionGroupListModel>>.Success(groupedPermissions);
        }
    }
}
