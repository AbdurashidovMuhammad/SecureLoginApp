// SecureLoginApp.Application.Extensions/DbContextExtensions.cs
using Microsoft.EntityFrameworkCore;
using SecureLoginApp.Application.Security;
using SecureLoginApp.Application.Security.AuthEnums;
using SecureLoginApp.Core.Entities;
using System.Reflection;

namespace SecureLoginApp.Application.Extensions;

public static class DbContextExtensions
{
    public static async Task ResolvePermissions(this DbContext context)
    {
        // 1. PermissionGroup'larni hal qilish (mavjudligini tekshirish va qo'shish)
        var existingGroups = await context.Set<PermissionGroup>().ToDictionaryAsync(g => g.Name, g => g);
        var groupsToSave = new List<PermissionGroup>();

        foreach (ApplicationPermissionGroupCode groupEnum in Enum.GetValues(typeof(ApplicationPermissionGroupCode)))
        {
            string groupName = groupEnum.ToString();
            if (!existingGroups.TryGetValue(groupName, out var existingGroup))
            {
                var newPermissionGroup = new PermissionGroup
                {
                    Name = groupName,
                };
                groupsToSave.Add(newPermissionGroup);
            }
        }

        if (groupsToSave.Any())
        {
            await context.Set<PermissionGroup>().AddRangeAsync(groupsToSave);
            await context.SaveChangesAsync();
            foreach (var group in groupsToSave)
            {
                // Yangi qo'shilgan guruhlarni lug'atga qo'shish
                if (!existingGroups.ContainsKey(group.Name))
                {
                    existingGroups.Add(group.Name, group);
                }
            }
        }

        // 2. Permission'larni hal qilish (mavjudligini tekshirish va qo'shish)
        var existingPermissions = await context.Set<Permission>().ToDictionaryAsync(p => p.ShortName, p => p);
        var permissionsToSave = new List<Permission>();
        var newlyAddedOrUpdatedPermissions = new List<Permission>(); // Yangi qo'shilgan yoki yangilangan permissionlar ro'yxati

        foreach (ApplicationPermissionCode permissionEnum in Enum.GetValues(typeof(ApplicationPermissionCode)))
        {
            var attr = permissionEnum.GetType()
                .GetField(permissionEnum.ToString())?
                .GetCustomAttribute<ApplicationPermissionDescriptionAttribute>();

            if (attr == null) continue;

            // Bu yerda o'zgarish: GetModulePermissionGroupName() dan foydalanamiz
            // Agar `ApplicationPermissionDescriptionAttribute` ni yuqoridagi ko'rsatmalarga asosan to'g'irlagan bo'lsangiz
            // `attr.ModulePermissionGroup.ToString()` ham to'g'ri ishlaydi, chunki u endi ApplicationPermissionGroupCode tipi.
            string groupName = attr.GetModulePermissionGroupName();
            string shortName = attr.ShortName;
            string fullName = attr.FullName;

            if (!existingGroups.TryGetValue(groupName, out var permissionGroup))
            {
                // Agar guruh topilmasa, bu yerda log yozish yoki istisno tashlash mumkin.
                // Hozircha keyingi permissionga o'tib ketadi.
                continue;
            }

            if (!existingPermissions.TryGetValue(shortName, out var existingPermission))
            {
                var newPermission = new Permission
                {
                    ShortName = shortName,
                    FullName = fullName,
                    PermissionGroup = permissionGroup,
                    PermissionGroupId = permissionGroup.Id,
                    CreatedAt = DateTime.UtcNow // UTC vaqtini ishlatish tavsiya etiladi
                };
                permissionsToSave.Add(newPermission);
                newlyAddedOrUpdatedPermissions.Add(newPermission); // Yangi qo'shilgan permissionni ro'yxatga qo'shish
            }
            else
            {
                bool changed = false;
                if (existingPermission.FullName != fullName) { existingPermission.FullName = fullName; changed = true; }
                if (existingPermission.PermissionGroupId != permissionGroup.Id) { existingPermission.PermissionGroupId = permissionGroup.Id; changed = true; }
                if (changed)
                {
                    existingPermission.UpdatedAt = DateTime.UtcNow; // UTC vaqtini ishlatish tavsiya etiladi
                    context.Set<Permission>().Update(existingPermission);
                    newlyAddedOrUpdatedPermissions.Add(existingPermission); // Yangilangan permissionni ro'yxatga qo'shish
                }
            }
        }

        if (permissionsToSave.Any())
        {
            await context.Set<Permission>().AddRangeAsync(permissionsToSave);
        }

        // Barcha permissionlar bazaga saqlanadi yoki yangilanadi
        await context.SaveChangesAsync();

        // 3. Admin rolini topish yoki yaratish
        var adminRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.Name == "Admin");

        if (adminRole == null)
        {
            // Admin roli topilmasa, uni yaratish
            adminRole = new Role { Name = "Admin", CreatedAt = DateTime.UtcNow }; // UTC vaqtini ishlatish
            await context.Set<Role>().AddAsync(adminRole);
            await context.SaveChangesAsync(); // Rolni saqlab, ID ni olish uchun muhim!
        }

        // 4. Admin roli uchun mavjud RolePermission'larni olish
        // Bu bosqichda barcha permissionlar bazada bo'lishi kerak, shuning uchun ularning ID'lari mavjud bo'ladi.
        var existingRolePermissions = await context.Set<RolePermission>()
            .Where(rp => rp.RoleId == adminRole.Id)
            .ToDictionaryAsync(rp => rp.PermissionId, rp => rp);

        // 5. Yangi RolePermission'larni qo'shish
        var rolePermissionsToSave = new List<RolePermission>();

        // `newlyAddedOrUpdatedPermissions` ro'yxatida faqatgina bazaga yangi qo'shilgan yoki yangilangan permissionlar bor.
        // Bizga barcha permissionlar kerak bo'lishi mumkin, chunki agar avvalgi ishga tushirishda qandaydir permission o'tkazib yuborilgan bo'lsa.
        // Shuning uchun, bu yerda barcha mavjud permissionlarni qayta yuklash yaxshiroq bo'lishi mumkin.
        var allCurrentPermissions = await context.Set<Permission>().ToListAsync();

        foreach (var permission in allCurrentPermissions) // Endi barcha mavjud permissionlar bo'yicha yurib chiqamiz
        {
            // Agar Admin rolida bu permission mavjud bo'lmasa, uni qo'shamiz
            if (!existingRolePermissions.ContainsKey(permission.Id))
            {
                rolePermissionsToSave.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
            }
        }

        if (rolePermissionsToSave.Any())
        {
            await context.Set<RolePermission>().AddRangeAsync(rolePermissionsToSave);
            await context.SaveChangesAsync();
        }
    }
}