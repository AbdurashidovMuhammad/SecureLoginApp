using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecureLoginApp.Core.Entities;
using System.Runtime.CompilerServices;

namespace SecureLoginApp.DataAccess.Persistence.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.Property(ur => ur.Id)
                   .ValueGeneratedOnAdd();

            // Seed data
            builder.HasData(GenerateUserRoles());
        }

        private static List<UserRole> GenerateUserRoles()
        {
            var userRoles = new List<UserRole>();

            // SuperAdmin user (ID=1) ga Admin rolini (ID=1) biriktiramiz
            userRoles.Add(new UserRole
            {
                Id = 1, // UserRole bog'lanishi uchun noyob ID
                UserId = 1, // SuperAdmin user ID'si
                RoleId = 1// Admin roli ID'si
            });

            // Agar keyinchalik boshqa default user-role bog'lanishlarini ham seed qilmoqchi bo'lsangiz, shu yerga qo'shishingiz mumkin.

            return userRoles;
        }
    }
}
