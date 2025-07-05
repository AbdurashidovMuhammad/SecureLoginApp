using Microsoft.EntityFrameworkCore;
using SecureLoginApp.Application.Helpers.PasswordHashers;
using SecureLoginApp.Application.Models.Users;
using SecureLoginApp.Application.Models;
using SecureLoginApp.Core.Entities;
using SecureLoginApp.DataAcces.Persistence;
using SecureLoginApp.Application.Helpers.GenerateJwt;

namespace SecureLoginApp.Application.Services.Impl;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenHandler _jwtTokenHandler;
    private readonly IAuthService _authService;

    public UserService(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        IOtpService otpService,
        IEmailService emailService,
        IJwtTokenHandler jwtTokenHandler,
        IAuthService authService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _otpService = otpService;
        _emailService = emailService;
        _jwtTokenHandler = jwtTokenHandler;
        _authService = authService;
    }

    public async Task<ApiResult<string>> RegisterAsync(string fullname, string email, string password, bool isAdminSite)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
            return ApiResult<string>.Failure(new[] { "Email allaqachon mavjud" });

        var salt = Guid.NewGuid().ToString();
        var hash = _passwordHasher.Encrypt(password, salt);

        var user = new User
        {
            Fullname = fullname,
            Email = email,
            PasswordHash = hash,
            Salt = salt,
            CreatedAt = DateTime.Now,
            IsVerified = false // Yangi foydalanuvchilar odatda tasdiqlanmagan holda boshlanadi
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // --- Rolni isAdminSite ga qarab belgilash ---
        string roleName = isAdminSite ? "Admin" : "User";
        var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

        if (defaultRole == null)
        {
            // Agar kerakli rol topilmasa, xato qaytaramiz
            return ApiResult<string>.Failure(new[] { $"Tizimda '{roleName}' roli topilmadi. Admin bilan bog'laning." });
        }

        _context.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = defaultRole.Id
        });
        await _context.SaveChangesAsync();
        // --- Rolni belgilash qismi tugadi ---

        var otp = await _otpService.GenerateAndSaveOtpAsync(user.Id);
        await _emailService.SendOtpAsync(email, otp);

        return ApiResult<string>.Success("Ro'yxatdan o'tdingiz. Email orqali tasdiqlang.");
    }

    public async Task<ApiResult<LoginResponseModel>> LoginAsync(LoginUserModel model)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user is null)
            return ApiResult<LoginResponseModel>.Failure(new[] { "Foydalanuvchi topilmadi" });

        if (!_passwordHasher.Verify(user.PasswordHash, model.Password, user.Salt))
            return ApiResult<LoginResponseModel>.Failure(new[] { "Parol noto‘g‘ri" });

        if (!user.IsVerified)
            return ApiResult<LoginResponseModel>.Failure(new[] { "Email tasdiqlanmagan" });

        var accessToken = _jwtTokenHandler.GenerateAccessToken(user, Guid.NewGuid().ToString());
        var refreshToken = _jwtTokenHandler.GenerateRefreshToken();

        return ApiResult<LoginResponseModel>.Success(new LoginResponseModel
        {
            Email = user.Email,
            Fullname = user.Fullname,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            Permissions = user.UserRoles
                 .SelectMany(ur => ur.Role.RolePermissions)
                 .Select(p => p.Permission.ShortName)
                 .Distinct()
                 .ToList()
        });
    }

    public async Task<ApiResult<string>> VerifyOtpAsync(OtpVerificationModel model)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user is null)
            return ApiResult<string>.Failure(new[] { "Foydalanuvchi topilmadi." });

        var otp = await _otpService.GetLatestOtpAsync(user.Id, model.Code);
        if (otp is null || otp.ExpiredAt < DateTime.Now)
            return ApiResult<string>.Failure(new[] { "Kod noto‘g‘ri yoki muddati tugagan." });

        user.IsVerified = true;
        await _context.SaveChangesAsync();

        return ApiResult<string>.Success("OTP muvaffaqiyatli tasdiqlandi.");
    }

    public async Task<ApiResult<UserAuthResponseModel>> GetUserAuth()
    {
        if (_authService.User == null)
        {
            return ApiResult<UserAuthResponseModel>.Failure(new List<string> { "User not found" });
        }

        UserAuthResponseModel userPermissions = new UserAuthResponseModel
        {
            Id = _authService.User.Id,
            FullName = _authService.User.FullName,
            Permissions = _authService.User.Permissions
        };

        return ApiResult<UserAuthResponseModel>.Success(userPermissions);
    }


}
