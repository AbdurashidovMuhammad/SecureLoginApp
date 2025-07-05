using Microsoft.AspNetCore.Http;
using SecureLoginApp.Application.Helpers.GenerateJwt;
using SecureLoginApp.Application.Security;
using SecureLoginApp.Core.Entities;
using SecureLoginApp.DataAccess.Persistence;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.CompilerServices;
using SecureLoginApp.DataAcces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SecureLoginApp.Application.Services.Impl;

public class AuthService : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _context;
    private bool? _isAuthenticated; // Bu field IsAuthenticated property'si uchun kesh bo'ladi.
    private HashSet<string>? _permissions; // Bu field Permissions property'si uchun kesh bo'ladi.
    private IUser? _user; // Bu field User property'si uchun kesh bo'ladi.

    public AuthService(IHttpContextAccessor httpContextAccessor, AppDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    // --- Muhim o'zgarishlar bu yerda ---
    public bool IsAuthenticated
    {
        get
        {
            // Agar _isAuthenticated allaqachon hisoblangan bo'lsa, uni qaytar.
            if (_isAuthenticated == null)
            {
                var httpContext = _httpContextAccessor.HttpContext;

                // Avval HTTP kontekstida foydalanuvchi autentifikatsiya qilinganmi, tekshiramiz.
                // Bu joyda User property'sini chaqirmaymiz, faqat Identity ni tekshiramiz.
                bool baseAuthenticated = httpContext?.User?.Identity?.IsAuthenticated ?? false;

                if (baseAuthenticated)
                {
                    // Agar bazaviy autentifikatsiya muvaffaqiyatli bo'lsa,
                    // endi User property'sini chaqirib, uning IsVerified holatini tekshiramiz.
                    // User property'si esa o'z ichida _user keshini boshqaradi va bazadan ma'lumotni yuklaydi.
                    var currentUser = User;

                    // Faqat autentifikatsiya qilingan va tasdiqlangan bo'lsa haqiqiy deb hisoblaymiz.
                    _isAuthenticated = (currentUser != null && currentUser.IsVerified);
                }
                else
                {
                    _isAuthenticated = false;
                }
            }
            return _isAuthenticated.GetValueOrDefault();
        }
    }

    public IUser User
    {
        get
        {
            // Agar _user allaqachon yuklangan bo'lsa, uni qaytar.
            if (_user != null)
            {
                return _user;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            // Agar HTTP konteksti va foydalanuvchi identiteti mavjud bo'lsa va autentifikatsiya qilingan bo'lsa
            if (httpContext?.User?.Identity != null && httpContext.User.Identity.IsAuthenticated)
            {
                var userIdClaim = httpContext.User.FindFirst(CustomClaimNames.Id);

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    _user = _context.Set<User>().AsQueryable().Select(a => new UserAuthModel
                    {
                        Id = a.Id,
                        FullName = a.Fullname, // Entity'dagi 'Fullname' property'sini ishlatamiz
                        IsVerified = a.IsVerified, // IsVerified ni yuklaymiz
                        // Foydalanuvchining rollari orqali ularga berilgan barcha ruxsatlarni olamiz
                        Permissions = a.UserRoles
                                         .SelectMany(ur => ur.Role.RolePermissions) // UserRole -> Role -> RolePermission
                                         .Select(rp => rp.Permission.ShortName)     // RolePermission -> Permission -> ShortName
                                         .Distinct() // Takrorlanuvchi ruxsat nomlarini olib tashlaymiz
                                         .ToHashSet(),
                    }).FirstOrDefault(a => a.Id == userId);
                }
            }

            return _user; // Yuklangan _user'ni qaytaradi (autentifikatsiya qilinmagan bo'lsa yoki topilmasa null bo'ladi)
        }
    }


    public int GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(a => a.Type == CustomClaimNames.Id);

        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
        {
            Console.WriteLine("⚠️ User ID not found in claims.");
            return 0;
        }
        return int.Parse(userIdClaim.Value);
    }

    public HashSet<string> Permissions
    {
        get
        {
            if (_permissions == null)
            {
                var currentUser = User; // User property'sini chaqiramiz. Bu _user ni yuklaydi.
                _permissions = currentUser?.Permissions?.ToHashSet() ?? new HashSet<string>();
            }
            return _permissions;
        }
    }

    public bool HasPermission(params string[] permissionCodes) =>
         permissionCodes.All(a => Permissions.Contains(a));
}