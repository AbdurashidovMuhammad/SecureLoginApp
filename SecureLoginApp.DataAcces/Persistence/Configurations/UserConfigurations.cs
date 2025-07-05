using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecureLoginApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SecureLoginApp.DataAccess.Persistence.Configurations
{
    public class UserConfigurations : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Umumiy xususiyatlar konfiguratsiyasi
            builder.Property(u => u.Id)
                .ValueGeneratedOnAdd(); // ID avtomatik generatsiyalansin (agar int bo'lsa)

            builder.Property(u => u.Email)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(u => u.Email)
                .IsUnique(); // Email noyob bo'lishi kerak

            builder.Property(u => u.PasswordHash)
                .HasMaxLength(256) // Hash uzunligi odatda 256 belgidan oshmaydi (Base64 kodlanganda ham)
                .IsRequired();

            builder.Property(u => u.Salt)
               .HasMaxLength(128) // Salt uzunligi odatda 128 belgidan oshmaydi (Guid uchun)
               .IsRequired();

            builder.Property(u => u.Fullname) // Sizning entity'ngizda 'Fullname'
              .HasMaxLength(100) // Ism-sharif uchun 100 belgi yetarli
              .IsRequired();

            builder.Property(u => u.IsVerified)
                .HasDefaultValue(false); // Default false bo'lsin

            // Seed Data
            builder.HasData(GetSeedUser());
        }

        /// <summary>
        /// Parolni hashlash metod. Faqat `HasData` uchun maxsus.
        /// </summary>
        private static string Encrypt(string password, string salt)
        {
            // Rfc2898DeriveBytes - xavfsiz parol hashlash algoritmi
            using var algorithm = new Rfc2898DeriveBytes(
                password: password,
                salt: Encoding.UTF8.GetBytes(salt), // Saltni baytlarga aylantiramiz
                iterations: 10000, // Iteratsiyalar soni. Kamida 10000 yoki undan ko'proq tavsiya etiladi.
                hashAlgorithm: HashAlgorithmName.SHA256); // Hash algoritmi (SHA256)

            return Convert.ToBase64String(algorithm.GetBytes(32)); // 32 baytli hash hosil qilamiz va Base64 ga o'tkazamiz
        }

        /// <summary>
        /// Default SuperAdmin foydalanuvchisini yaratadi.
        /// </summary>
        private static User GetSeedUser()
        {
            // ID ni int turiga moslab to'g'riladim. Sizning IdConst klassingizni ham int ga o'zgartirishingiz kerak.
            const int seedUserId = 1; // SuperAdmin uchun ID, int turida
            const string seedSalt = "9f7d6dc5-34b4-4b66-a65e-0dc2fc17c0db"; // Bu yerda statik salt
            const string seedPassword = "web@1234"; // SuperAdminning boshlang'ich paroli

            return new User
            {
                Id = seedUserId, // IdConst.IdUserConst.SuperAdminId ni shu int qiymatiga moslang
                Fullname = "Adminjon", // Sizning User entity'ngizda 'Fullname'
                Email = "superadmin@example.com",
                Salt = seedSalt,
                PasswordHash = Encrypt(seedPassword, seedSalt),
                CreatedAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsVerified = true // SuperAdmin odatda tasdiqlangan bo'ladi
            };
        }
    }
}
