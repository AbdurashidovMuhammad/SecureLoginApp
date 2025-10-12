/*
 * ========================================
 * AUTHSERVICE UCHUN UNIT TESTLAR
 * ========================================
 *
 * Bu fayl AuthService klassini test qiladi.
 *
 * NIMANI O'RGANAMIZ:
 * 1. Mock Database (InMemory Database) - Haqiqiy database o'rniga xotirada test database
 * 2. Mock HttpContext - HTTP so'rovlarini simulyatsiya qilish
 * 3. Service Testing - Biznes logikani test qilish
 * 4. AAA Pattern - Arrange, Act, Assert
 *
 * O'quvchilar uchun yaratilgan!
 */

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using SecureLoginApp.Application.Helpers.GenerateJwt;
using SecureLoginApp.Application.Services.Impl;
using SecureLoginApp.Core.Entities;
using SecureLoginApp.DataAcces.Persistence;
using System.Security.Claims;

namespace SecureLoginApp.Tests.Services;

/// <summary>
/// AuthService uchun test klassi
///
/// MOCK NIMA?
/// Mock - bu haqiqiy obyektning "soxta" versiyasi.
/// Test paytida real database, HTTP request va boshqalar o'rniga
/// ularning mock versiyalaridan foydalanamiz.
///
/// NEGA MOCK ISHLATAMIZ?
/// 1. Tez ishlaydi (real database'ga ulanmaydi)
/// 2. Test mustaqil (boshqa servicelardan bog'liq emas)
/// 3. Xavfsiz (haqiqiy ma'lumotlarni buzmaymiz)
/// </summary>
public class AuthServiceTests : IDisposable
{
    // ============================================
    // TEST UCHUN KERAKLI OBYEKTLAR
    // ============================================

    private readonly AppDbContext _context;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly AuthService _authService;

    // ============================================
    // CONSTRUCTOR - HAR BIR TEST OLDIDAN ISHLAYDI
    // ============================================
    /// <summary>
    /// Bu method har bir test ishga tushishidan oldin chaqiriladi
    ///
    /// QANDAY ISHLAYDI:
    /// 1. InMemory Database yaratamiz (xotirada)
    /// 2. Test ma'lumotlarni qo'shamiz
    /// 3. Mock HttpContext yaratamiz
    /// 4. AuthService'ni yaratamiz
    /// </summary>
    public AuthServiceTests()
    {
        // -----------------------------------------------
        // 1. INMEMORY DATABASE YARATISH
        // -----------------------------------------------
        // InMemory database - bu xotirada ishlaydigan soxta database
        // Haqiqiy SQL Server, PostgreSQL kabi databaselar bilan ishlamaydi
        // Faqat test paytida ishlatiladi va juda tez ishlaydi

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Har bir test uchun yangi database
            .Options;

        _context = new AppDbContext(options);

        // -----------------------------------------------
        // 2. TEST MA'LUMOTLARINI QO'SHISH (SEED DATA)
        // -----------------------------------------------
        // Bu yerda test uchun kerakli ma'lumotlarni database'ga qo'shamiz
        SeedTestData();

        // -----------------------------------------------
        // 3. MOCK HTTPCONTEXTACCESSOR YARATISH
        // -----------------------------------------------
        // HttpContextAccessor orqali AuthService joriy foydalanuvchi haqida ma'lumot oladi
        // Test paytida haqiqiy HTTP request yo'q, shuning uchun Mock ishlatamiz
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        // -----------------------------------------------
        // 4. AUTHSERVICE YARATISH
        // -----------------------------------------------
        _authService = new AuthService(_mockHttpContextAccessor.Object, _context);
    }

    // ============================================
    // TEST MA'LUMOTLARINI QO'SHISH
    // ============================================
    /// <summary>
    /// Database'ga test uchun kerakli ma'lumotlarni qo'shamiz
    ///
    /// BU YERDA NIMALAR YARATAMIZ:
    /// 1. Permission (ruxsatlar)
    /// 2. Role (rollar)
    /// 3. RolePermission (rol-ruxsat bog'lanishi)
    /// 4. User (foydalanuvchilar)
    /// 5. UserRole (foydalanuvchi-rol bog'lanishi)
    /// </summary>
    private void SeedTestData()
    {
        // PermissionGroup yaratish (Permission uchun kerak)
        var permissionGroup = new PermissionGroup
        {
            Id = 1,
            Name = "User Management"
        };
        _context.PermissionGroups.Add(permissionGroup);

        // Permission (Ruxsatlar) yaratish
        var viewPermission = new Permission
        {
            Id = 1,
            FullName = "View Users",
            ShortName = "VIEW_USERS",
            PermissionGroupId = 1
        };
        var editPermission = new Permission
        {
            Id = 2,
            FullName = "Edit Users",
            ShortName = "EDIT_USERS",
            PermissionGroupId = 1
        };
        var deletePermission = new Permission
        {
            Id = 3,
            FullName = "Delete Users",
            ShortName = "DELETE_USERS",
            PermissionGroupId = 1
        };

        _context.Permissions.AddRange(viewPermission, editPermission, deletePermission);

        // Role (Rollar) yaratish
        var adminRole = new Role { Id = 1, Name = "Admin" };
        var userRole = new Role { Id = 2, Name = "User" };

        _context.Roles.AddRange(adminRole, userRole);

        // RolePermission (Rol-Ruxsat bog'lanishi)
        // Admin rollida barcha ruxsatlar bor
        _context.RolePermissions.AddRange(
            new RolePermission { RoleId = 1, PermissionId = 1 }, // Admin -> VIEW_USERS
            new RolePermission { RoleId = 1, PermissionId = 2 }, // Admin -> EDIT_USERS
            new RolePermission { RoleId = 1, PermissionId = 3 }  // Admin -> DELETE_USERS
        );

        // User rollida faqat ko'rish ruxsati bor
        _context.RolePermissions.Add(
            new RolePermission { RoleId = 2, PermissionId = 1 } // User -> VIEW_USERS
        );

        // User (Foydalanuvchilar) yaratish
        var verifiedUser = new User
        {
            Id = 1,
            Fullname = "Ali Valiyev",
            Email = "ali@test.com",
            PasswordHash = "hash123",
            Salt = "salt123",
            IsVerified = true, // Bu foydalanuvchi tasdiqlangan
            CreatedAt = DateTime.Now
        };

        var unverifiedUser = new User
        {
            Id = 2,
            Fullname = "Vali Aliyev",
            Email = "vali@test.com",
            PasswordHash = "hash456",
            Salt = "salt456",
            IsVerified = false, // Bu foydalanuvchi tasdiqlanmagan
            CreatedAt = DateTime.Now
        };

        _context.Users.AddRange(verifiedUser, unverifiedUser);

        // UserRole (Foydalanuvchi-Rol bog'lanishi)
        _context.UserRoles.AddRange(
            new UserRole { UserId = 1, RoleId = 1 }, // Ali - Admin
            new UserRole { UserId = 2, RoleId = 2 }  // Vali - User
        );

        // Barcha o'zgarishlarni saqlash
        _context.SaveChanges();
    }

    // ============================================
    // MOCK HTTPCONTEXT YARATISH
    // ============================================
    /// <summary>
    /// Test uchun HTTP Context yaratamiz
    ///
    /// HTTP CONTEXT NIMA?
    /// Har bir HTTP request kelganda ASP.NET HttpContext yaratadi
    /// U ichida foydalanuvchi ma'lumotlari, cookie, header va boshqalar bor
    ///
    /// BU METHODDA:
    /// Foydalanuvchining ID'sini Claims ichiga qo'yamiz
    /// AuthService shu Claims'dan foydalanuvchini topadi
    /// </summary>
    private void SetupHttpContext(int userId, bool isAuthenticated = true)
    {
        // Claims - bu foydalanuvchi haqidagi ma'lumotlar
        // Masalan: ID, Email, Role va boshqalar
        var claims = new List<Claim>
        {
            new Claim(CustomClaimNames.Id, userId.ToString()), // Foydalanuvchi ID'si
            new Claim(ClaimTypes.Name, "Test User")            // Foydalanuvchi nomi
        };

        // ClaimsIdentity - foydalanuvchining identifikatsiyasi
        var identity = new ClaimsIdentity(
            claims,
            isAuthenticated ? "TestAuthentication" : null // Agar null bo'lsa - autentifikatsiya qilinmagan
        );

        // ClaimsPrincipal - joriy foydalanuvchi
        var claimsPrincipal = new ClaimsPrincipal(identity);

        // HttpContext yaratish
        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal // Foydalanuvchini HttpContext'ga biriktirish
        };

        // Mock HttpContextAccessor'ga HttpContext'ni o'rnatish
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    }

    // ============================================
    // TEST 1: TASDIQLANGAN FOYDALANUVCHI AUTENTIFIKATSIYA QILINGAN
    // ============================================
    /// <summary>
    /// Test: Agar foydalanuvchi tasdiqlangan bo'lsa, IsAuthenticated = true bo'lishi kerak
    ///
    /// SCENARIO:
    /// - Foydalanuvchi ID = 1 (Ali Valiyev)
    /// - IsVerified = true
    /// - Natija: IsAuthenticated = true
    /// </summary>
    [Fact]
    public void IsAuthenticated_VerifiedUser_ReturnsTrue()
    {
        // ---- ARRANGE (Tayyorgarlik) ----
        // Tasdiqlangan foydalanuvchi uchun HTTP Context yaratish
        SetupHttpContext(userId: 1, isAuthenticated: true);

        // ---- ACT (Harakat) ----
        // AuthService'ning IsAuthenticated property'sini olish
        var isAuthenticated = _authService.IsAuthenticated;

        // ---- ASSERT (Tekshirish) ----
        // Natija true bo'lishi kerak
        Assert.True(isAuthenticated, "Tasdiqlangan foydalanuvchi autentifikatsiya qilingan bo'lishi kerak");
    }

    // ============================================
    // TEST 2: TASDIQLANMAGAN FOYDALANUVCHI AUTENTIFIKATSIYA QILINMAGAN
    // ============================================
    [Fact]
    public void IsAuthenticated_UnverifiedUser_ReturnsFalse()
    {
        // ---- ARRANGE ----
        SetupHttpContext(userId: 2, isAuthenticated: true); // ID=2 -> Tasdiqlanmagan

        // ---- ACT ----
        var isAuthenticated = _authService.IsAuthenticated;

        // ---- ASSERT ----
        Assert.False(isAuthenticated, "Tasdiqlanmagan foydalanuvchi autentifikatsiya qilinmasligi kerak");
    }

    // ============================================
    // TEST 3: FOYDALANUVCHI MA'LUMOTLARI TO'G'RI YUKLANADI
    // ============================================
    /// <summary>
    /// Test: User property foydalanuvchi ma'lumotlarini to'g'ri yuklaydi
    /// </summary>
    [Fact]
    public void User_ValidUserId_ReturnsUserWithCorrectData()
    {
        // ---- ARRANGE ----
        SetupHttpContext(userId: 1, isAuthenticated: true);

        // ---- ACT ----
        var user = _authService.User;

        // ---- ASSERT ----
        Assert.NotNull(user); // User null bo'lmasligi kerak
        Assert.Equal(1, user.Id); // ID to'g'ri bo'lishi kerak
        Assert.Equal("Ali Valiyev", user.FullName); // Ism to'g'ri bo'lishi kerak
        Assert.True(user.IsVerified); // IsVerified = true bo'lishi kerak
    }

    // ============================================
    // TEST 4: FOYDALANUVCHI RUXSATLARI TO'G'RI YUKLANADI
    // ============================================
    /// <summary>
    /// Test: Permissions to'g'ri yuklanadi
    ///
    /// SCENARIO:
    /// - Ali (ID=1) - Admin roli bor
    /// - Admin'da 3 ta ruxsat bor: VIEW, EDIT, DELETE
    /// - Natija: Permissions ichida 3 ta element bo'lishi kerak
    /// </summary>
    [Fact]
    public void Permissions_AdminUser_ReturnsAllPermissions()
    {
        // ---- ARRANGE ----
        SetupHttpContext(userId: 1, isAuthenticated: true);

        // ---- ACT ----
        var permissions = _authService.Permissions;

        // ---- ASSERT ----
        Assert.NotNull(permissions);
        Assert.Equal(3, permissions.Count); // 3 ta ruxsat bo'lishi kerak
        Assert.Contains("VIEW_USERS", permissions);
        Assert.Contains("EDIT_USERS", permissions);
        Assert.Contains("DELETE_USERS", permissions);
    }

    // ============================================
    // TEST 5: ODDIY FOYDALANUVCHI CHEKLANGAN RUXSATLARGA EGA
    // ============================================
    [Fact]
    public void Permissions_RegularUser_ReturnsLimitedPermissions()
    {
        // ---- ARRANGE ----
        SetupHttpContext(userId: 2, isAuthenticated: true); // ID=2 -> User roli

        // ---- ACT ----
        var permissions = _authService.Permissions;

        // ---- ASSERT ----
        Assert.NotNull(permissions);
        Assert.Single(permissions); // Faqat 1 ta ruxsat bo'lishi kerak
        Assert.Contains("VIEW_USERS", permissions);
        Assert.DoesNotContain("EDIT_USERS", permissions);
        Assert.DoesNotContain("DELETE_USERS", permissions);
    }

    // ============================================
    // TEST 6: HASPERMISSION METHODI TO'G'RI ISHLAYDI
    // ============================================
    /// <summary>
    /// Test: HasPermission methodi ruxsatni to'g'ri tekshiradi
    /// </summary>
    [Fact]
    public void HasPermission_UserHasPermission_ReturnsTrue()
    {
        // ---- ARRANGE ----
        SetupHttpContext(userId: 1, isAuthenticated: true); // Admin

        // ---- ACT ----
        var hasViewPermission = _authService.HasPermission("VIEW_USERS");
        var hasEditPermission = _authService.HasPermission("EDIT_USERS");

        // ---- ASSERT ----
        Assert.True(hasViewPermission);
        Assert.True(hasEditPermission);
    }

    [Fact]
    public void HasPermission_UserDoesNotHavePermission_ReturnsFalse()
    {
        // ---- ARRANGE ----
        SetupHttpContext(userId: 2, isAuthenticated: true); // Oddiy user

        // ---- ACT ----
        var hasEditPermission = _authService.HasPermission("EDIT_USERS");
        var hasDeletePermission = _authService.HasPermission("DELETE_USERS");

        // ---- ASSERT ----
        Assert.False(hasEditPermission);
        Assert.False(hasDeletePermission);
    }

    // ============================================
    // TEST 7: BIR NECHTA RUXSATNI TEKSHIRISH
    // ============================================
    [Fact]
    public void HasPermission_MultiplePermissions_ChecksAll()
    {
        // ---- ARRANGE ----
        SetupHttpContext(userId: 1, isAuthenticated: true); // Admin

        // ---- ACT ----
        var hasAllPermissions = _authService.HasPermission("VIEW_USERS", "EDIT_USERS", "DELETE_USERS");

        // ---- ASSERT ----
        Assert.True(hasAllPermissions, "Admin barcha ruxsatlarga ega bo'lishi kerak");
    }

    [Fact]
    public void HasPermission_MultiplePermissionsButMissingOne_ReturnsFalse()
    {
        // ---- ARRANGE ----
        SetupHttpContext(userId: 2, isAuthenticated: true); // Oddiy user

        // ---- ACT ----
        // User VIEW ruxsatiga ega, lekin EDIT ruxsatiga ega emas
        var hasAllPermissions = _authService.HasPermission("VIEW_USERS", "EDIT_USERS");

        // ---- ASSERT ----
        Assert.False(hasAllPermissions, "Barcha ruxsatlar bo'lmasa false qaytarishi kerak");
    }

    // ============================================
    // TEST 8: GETUSERID METHODI TO'G'RI ISHLAYDI
    // ============================================
    [Fact]
    public void GetUserId_AuthenticatedUser_ReturnsCorrectId()
    {
        // ---- ARRANGE ----
        SetupHttpContext(userId: 1, isAuthenticated: true);

        // ---- ACT ----
        var userId = _authService.GetUserId();

        // ---- ASSERT ----
        Assert.Equal(1, userId);
    }

    [Fact]
    public void GetUserId_NoHttpContext_ReturnsZero()
    {
        // ---- ARRANGE ----
        // HttpContext'ni null qilib qo'yamiz
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);

        // ---- ACT ----
        var userId = _authService.GetUserId();

        // ---- ASSERT ----
        Assert.Equal(0, userId);
    }

    // ============================================
    // TEST 9: AUTENTIFIKATSIYA QILINMAGAN FOYDALANUVCHI
    // ============================================
    [Fact]
    public void IsAuthenticated_NotAuthenticatedUser_ReturnsFalse()
    {
        // ---- ARRANGE ----
        SetupHttpContext(userId: 1, isAuthenticated: false); // isAuthenticated = false

        // ---- ACT ----
        var isAuthenticated = _authService.IsAuthenticated;

        // ---- ASSERT ----
        Assert.False(isAuthenticated);
    }

    [Fact]
    public void User_NotAuthenticatedUser_ReturnsNull()
    {
        // ---- ARRANGE ----
        SetupHttpContext(userId: 1, isAuthenticated: false);

        // ---- ACT ----
        var user = _authService.User;

        // ---- ASSERT ----
        Assert.Null(user);
    }

    // ============================================
    // CLEANUP - TEST TUGAGANDAN KEYIN ISHLAYDI
    // ============================================
    /// <summary>
    /// Har bir test tugagandan keyin database'ni tozalash
    /// Bu xotirada joy ochadi va boshqa testlarga ta'sir qilmaydi
    /// </summary>
    public void Dispose()
    {
        _context.Database.EnsureDeleted(); // Database'ni o'chirish
        _context.Dispose(); // Context'ni yopish
    }
}

// ============================================
// TESTLARNI QANDAY ISHLATISH
// ============================================
/*
 * VISUAL STUDIO'DA:
 * 1. Test Explorer'ni oching (Test > Test Explorer)
 * 2. "Run All Tests" ni bosing
 * 3. Yashil belgi - test o'tdi ✓
 * 4. Qizil belgi - test muvaffaqiyatsiz ✗
 *
 * COMMAND LINE'DA:
 * cd SecureLoginApp.Tests
 * dotnet test
 *
 * BITTA TESTNI ISHGA TUSHIRISH:
 * dotnet test --filter "IsAuthenticated_VerifiedUser_ReturnsTrue"
 */

// ============================================
// MUHIM TUSHUNCHALAR
// ============================================
/*
 * MOCK NIMA?
 * Mock - bu haqiqiy obyektning "soxta" versiyasi.
 * Moq kutubxonasi yordamida Mock obyektlar yaratamiz.
 *
 * INMEMORY DATABASE NIMA?
 * InMemory database - xotirada ishlaydigan soxta database.
 * Real database'dan farqi:
 * - Juda tez ishlaydi
 * - Disk'ga yozmaydi
 * - Test tugagach yo'qoladi
 *
 * AAA PATTERN:
 * - ARRANGE (Tayyorgarlik) - Test ma'lumotlarini tayyorlash
 * - ACT (Harakat) - Testni bajarish
 * - ASSERT (Tekshirish) - Natijani tekshirish
 *
 * BEST PRACTICES:
 * 1. Har bir test mustaqil bo'lishi kerak
 * 2. Test nomlari aniq va tushunarli bo'lishi kerak
 * 3. Bir test - bitta kontseptsiya
 * 4. Seed data barcha testlar uchun bir xil bo'lishi kerak
 */
