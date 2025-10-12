using Microsoft.EntityFrameworkCore;
using SecureLoginApp.Application.Helpers.GenerateJwt;
using SecureLoginApp.Application.Models.Users;
using SecureLoginApp.Application.Services.Impl;
using SecureLoginApp.Application.Services;
using SecureLoginApp.Core.Entities;
using SecureLoginApp.DataAcces.Persistence;
using SecureLoginApp.Application.Helpers.PasswordHashers;
using Moq;
using FluentAssertions;
using Xunit;
using static System.Net.WebRequestMethods;

namespace TestProject;

public class UserServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IOtpService> _otpServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IJwtTokenHandler> _jwtTokenHandlerMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        _passwordHasherMock = new Mock<IPasswordHasher>();
        _otpServiceMock = new Mock<IOtpService>();
        _emailServiceMock = new Mock<IEmailService>();
        _jwtTokenHandlerMock = new Mock<IJwtTokenHandler>();
        _authServiceMock = new Mock<IAuthService>();

        _userService = new UserService(
            _context,
            _passwordHasherMock.Object,
            _otpServiceMock.Object,
            _emailServiceMock.Object,
            _jwtTokenHandlerMock.Object,
            _authServiceMock.Object
        );

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var userRole = new Role { Id = 1, Name = "User" };
        var adminRole = new Role { Id = 2, Name = "Admin" };

        _context.Roles.AddRange(userRole, adminRole);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ============================================
    // REGISTER TESTLARI
    // ============================================


    [Fact]
    public async Task RegisterAsync_AsAdmin_ShouldAssignAdminRole()
    {
        // ARRANGE
        var fullname = "Admin User";
        var email = "admin@example.com";
        var password = "Admin123!";
        var isAdminSite = true;

        _passwordHasherMock
            .Setup(x => x.Encrypt(password, It.IsAny<string>()))
            .Returns("hashed_password");

        _otpServiceMock
            .Setup(x => x.GenerateAndSaveOtpAsync(It.IsAny<int>()))
            .ReturnsAsync("123456");

        // ACT
        var result = await _userService.RegisterAsync(fullname, email, password, isAdminSite);

        // ASSERT
        result.Succeeded.Should().BeTrue();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        user.Should().NotBeNull();

        var userRole = await _context.UserRoles
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.UserId == user!.Id);
        userRole.Should().NotBeNull();
        userRole!.Role.Name.Should().Be("Admin");
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnFailure()
    {
        // ARRANGE
        var email = "existing@example.com";

        var existingUser = new User
        {
            Fullname = "Existing User",
            Email = email,
            PasswordHash = "hash",
            Salt = "salt",
            CreatedAt = DateTime.Now,
            IsVerified = false
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        // ACT
        var result = await _userService.RegisterAsync("New User", email, "password", false);

        // ASSERT
        result.Succeeded.Should().BeFalse(); // ⬅️ Succeeded
        result.Errors.Should().Contain("Email allaqachon mavjud");

        var userCount = await _context.Users.CountAsync(u => u.Email == email);
        userCount.Should().Be(1);
    }

    [Fact]
    public async Task RegisterAsync_WhenRoleNotFound_ShouldReturnFailure()
    {
        // ARRANGE
        _context.Roles.RemoveRange(_context.Roles);
        await _context.SaveChangesAsync();

        _passwordHasherMock
            .Setup(x => x.Encrypt(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("hashed_password");

        // ACT
        var result = await _userService.RegisterAsync("Test", "test@test.com", "password", false);

        // ASSERT
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("roli topilmadi"));
    }

    // ============================================
    // LOGIN TESTLARI
    // ============================================

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccessWithTokens()
    {
        // ARRANGE
        var email = "user@example.com";
        var password = "Password123!";
        var salt = "test_salt";
        var hashedPassword = "hashed_password";

        var user = new User
        {
            Fullname = "Test User",
            Email = email,
            PasswordHash = hashedPassword,
            Salt = salt,
            CreatedAt = DateTime.Now,
            IsVerified = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var userRoleEntity = new UserRole
        {
            UserId = user.Id,
            RoleId = 1
        };
        _context.UserRoles.Add(userRoleEntity);
        await _context.SaveChangesAsync();

        _passwordHasherMock
            .Setup(x => x.Verify(hashedPassword, password, salt))
            .Returns(true);

        _jwtTokenHandlerMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<User>(), It.IsAny<string>()))
            .Returns("access_token");

        _jwtTokenHandlerMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        var loginModel = new LoginUserModel
        {
            Email = email,
            Password = password
        };

        // ACT
        var result = await _userService.LoginAsync(loginModel);

        // ASSERT - Result ishlatish ✅
        result.Succeeded.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Email.Should().Be(email);
        result.Result.Fullname.Should().Be("Test User");
        result.Result.AccessToken.Should().Be("access_token");
        result.Result.RefreshToken.Should().Be("refresh_token");
        result.Result.Roles.Should().Contain("User");

        _passwordHasherMock.Verify(x => x.Verify(hashedPassword, password, salt), Times.Once);
        _jwtTokenHandlerMock.Verify(x => x.GenerateAccessToken(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
        _jwtTokenHandlerMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ShouldReturnFailure()
    {
        // ARRANGE
        var loginModel = new LoginUserModel
        {
            Email = "nonexistent@example.com",
            Password = "password"
        };

        // ACT
        var result = await _userService.LoginAsync(loginModel);

        // ASSERT
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Foydalanuvchi topilmadi");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldReturnFailure()
    {
        // ARRANGE
        var email = "user@example.com";
        var wrongPassword = "WrongPassword123!";
        var salt = "test_salt";
        var hashedPassword = "hashed_password";

        var user = new User
        {
            Fullname = "Test User",
            Email = email,
            PasswordHash = hashedPassword,
            Salt = salt,
            CreatedAt = DateTime.Now,
            IsVerified = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _passwordHasherMock
            .Setup(x => x.Verify(hashedPassword, wrongPassword, salt))
            .Returns(false);

        var loginModel = new LoginUserModel
        {
            Email = email,
            Password = wrongPassword
        };

        // ACT
        var result = await _userService.LoginAsync(loginModel);

        // ASSERT
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Parol noto'g'ri");
    }

    [Fact]
    public async Task LoginAsync_WithUnverifiedEmail_ShouldReturnFailure()
    {
        // ARRANGE
        var email = "unverified@example.com";
        var password = "Password123!";
        var salt = "test_salt";
        var hashedPassword = "hashed_password";

        var user = new User
        {
            Fullname = "Unverified User",
            Email = email,
            PasswordHash = hashedPassword,
            Salt = salt,
            CreatedAt = DateTime.Now,
            IsVerified = false
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _passwordHasherMock
            .Setup(x => x.Verify(hashedPassword, password, salt))
            .Returns(true);

        var loginModel = new LoginUserModel
        {
            Email = email,
            Password = password
        };

        // ACT
        var result = await _userService.LoginAsync(loginModel);

        // ASSERT
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Email tasdiqlanmagan");
    }

    // ============================================
    // VERIFY OTP TESTLARI
    // ============================================


    [Fact]
    public async Task VerifyOtpAsync_WithNonExistentUser_ShouldReturnFailure()
    {
        // ARRANGE
        var model = new OtpVerificationModel
        {
            Email = "nonexistent@example.com",
            Code = "123456"
        };

        // ACT
        var result = await _userService.VerifyOtpAsync(model);

        // ASSERT
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Foydalanuvchi topilmadi.");
    }


}
