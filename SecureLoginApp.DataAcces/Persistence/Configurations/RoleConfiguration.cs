
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecureLoginApp.Core.Entities;
using SecureLoginApp.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SecureLoginApp.DataAccess.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.Property(e => e.Name)
                .HasMaxLength(50) // Role names maximum length  
                .IsRequired();

            builder.HasIndex(e => e.Name)
                .IsUnique(); // Role names must be unique  

            // Seed data  
            builder.HasData(GenerateRoles());
        }

        private static List<Role> GenerateRoles()
        {
            // Static date for HasData. This value is hardcoded into the migration.  
            var seedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var roles = new List<Role>();

            // Create a role for each value in the UserRole enum  
            foreach (int roleEnumValue in Enum.GetValues(typeof(Core.Enums.UserRole)))
            {
                roles.Add(new Role
                {
                    Id = roleEnumValue, // Use enum value as Id  
                    Name = Enum.GetName(typeof(Core.Enums.UserRole), roleEnumValue), // Use enum name as Role name  
                    CreatedAt = seedDate // Static date  
                });
            }

            return roles;
        }
    }
}
