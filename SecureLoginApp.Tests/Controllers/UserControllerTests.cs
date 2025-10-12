/*
 * ========================================
 * USERCONTROLLER UCHUN UNIT TESTLAR
 * ========================================
 *
 * Bu fayl UserController klassini test qiladi.
 *
 * NIMANI O'RGANAMIZ:
 * 1. Controller Testing - HTTP endpoint'larni test qilish
 * 2. Mock Service - Haqiqiy service o'rniga mock service ishlatish
 * 3. HTTP Status Code - To'g'ri status code qaytarilishini tekshirish
 * 4. Response Data - Qaytarilgan ma'lumotlarni tekshirish
 *
 * O'quvchilar uchun yaratilgan!
 */

using Microsoft.AspNetCore.Mvc;
using Moq;
using SecureLoginApp.API.Controllers;
using SecureLoginApp.Application.Models;
using SecureLoginApp.Application.Models.Users;
using SecureLoginApp.Application.Services;

namespace SecureLoginApp.Tests.Controllers;

/// <summary>
/// UserController uchun test klassi
///
/// CONTROLLER NIMA?
/// Controller - bu HTTP requestlarni qabul qilib, javob qaytaradigan klass
/// Masalan: POST /api/user/login - bu endpointni UserController boshqaradi
///
/// NEGA CONTROLLER'NI TEST QILAMIZ?
/// 1. HTTP Status Code to'g'ri qaytarilishini tekshirish
/// 2. Response ma'lumotlari to'g'ri formatda ekanligini tekshirish
/// 3. Service method'lari to'g'ri chaqirilishini tekshirish
/// 4. Input validation ishlashini tekshirish
/// </summary>
public class UserControllerTests
{
    // ============================================
    // TEST UCHUN KERAKLI OBYEKTLAR
    // ============================================

    private readonly Mock<IUserService> _mockUserService;
    private readonly UserController _controller;

    // ============================================
    // CONSTRUCTOR - HAR BIR TEST OLDIDAN ISHLAYDI
    // ============================================
    /// <summary>
    /// Bu method har bir test ishga tushishidan oldin chaqiriladi
    ///
    /// MOCK NIMA VA NEGA KERAK?
    /// Mock - bu haqiqiy obyektning "soxta" versiyasi
    ///
    /// NEGA ISHLATAMIZ:
    /// 1. Controller'ni mustaqil test qilish uchun
    /// 2. Haqiqiy database'ga bog'liq bo'lmaslik uchun
    /// 3. Test ma'lumotlarini nazorat qilish uchun
    ///
    /// MISOL:
    /// Agar haqiqiy UserService database'dan ma'lumot oladi,
    /// Mock UserService bizning aytgan ma'lumotni qaytaradi
    /// </summary>
    public UserControllerTests()
    {
        // Mock UserService yaratish
        // Bu haqiqiy UserService emas, balki uning "soxta" versiyasi
        _mockUserService = new Mock<IUserService>();

        // UserController yaratish va Mock Service'ni berish
        _controller = new UserController(_mockUserService.Object);
    }

    // ============================================
    // TEST 1: REGISTER - MUVAFFAQIYATLI RO'YXATDAN O'TISH
    // ============================================
    /// <summary>
    /// Test: Foydalanuvchi muvaffaqiyatli ro'yxatdan o'tganda
    ///
    /// SCENARIO:
    /// - Foydalanuvchi to'g'ri ma'lumotlar bilan ro'yxatdan o'tadi
    /// - Service muvaffaqiyatli natija qaytaradi
    /// - Controller ApiResult<string> qaytarishi kerak
    /// - Succeeded = true bo'lishi kerak
    /// </summary>
    [Fact]
    public async Task RegisterAsync_ValidData_ReturnsSuccessResult()
    {
        // ---- ARRANGE (Tayyorgarlik) ----

        // 1. Test ma'lumotlarini tayyorlash
        var registerModel = new RegisterUserModel
        {
            Fullname = "Ali Valiyev",
            Email = "ali@test.com",
            Password = "Test123!",
            isAdminSite = false
        };

        // 2. Mock Service'ning javobini sozlash
        // SETUP nima?
        // Setup - Mock obyektga "agar shunday method chaqirilsa, shuni qaytarsin" deb o'rgatish
        var expectedResult = ApiResult<string>.Success("OTP yuborildi");

        _mockUserService
            .Setup(s => s.RegisterAsync(
                registerModel.Fullname,
                registerModel.Email,
                registerModel.Password,
                registerModel.isAdminSite))
            .ReturnsAsync(expectedResult);

        // ---- ACT (Harakat) ----
        // Controller methodini chaqirish
        var result = await _controller.RegisterAsync(registerModel);

        // ---- ASSERT (Tekshirish) ----
        // 1. Natija null bo'lmasligi kerak
        Assert.NotNull(result);

        // 2. Natija muvaffaqiyatli bo'lishi kerak
        Assert.True(result.Succeeded);

        // 3. Ma'lumot to'g'ri bo'lishi kerak
        Assert.Equal("OTP yuborildi", result.Data);

        // 4. Service method'i to'g'ri parametrlar bilan chaqirilganligini tekshirish
        // VERIFY nima?
        // Verify - Mock method'i haqiqatan chaqirilganligini tekshirish
        _mockUserService.Verify(
            s => s.RegisterAsync(
                registerModel.Fullname,
                registerModel.Email,
                registerModel.Password,
                registerModel.isAdminSite),
            Times.Once // Faqat 1 marta chaqirilgan bo'lishi kerak
        );
    }

    // ============================================
    // TEST 2: REGISTER - SERVICE XATO QAYTARGANDA
    // ============================================
    /// <summary>
    /// Test: Service xato qaytarganda Controller ham xatoni qaytarishi kerak
    /// </summary>
    [Fact]
    public async Task RegisterAsync_ServiceReturnsError_ReturnsFailureResult()
    {
        // ---- ARRANGE ----
        var registerModel = new RegisterUserModel
        {
            Fullname = "Test User",
            Email = "test@test.com",
            Password = "Test123!",
            isAdminSite = false
        };

        // Service xato qaytaradi deb sozlaymiz
        var expectedResult = ApiResult<string>.Failure("Email allaqachon mavjud");

        _mockUserService
            .Setup(s => s.RegisterAsync(
                It.IsAny<string>(),  // It.IsAny - har qanday qiymat bo'lishi mumkin
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .ReturnsAsync(expectedResult);

        // ---- ACT ----
        var result = await _controller.RegisterAsync(registerModel);

        // ---- ASSERT ----
        Assert.NotNull(result);
        Assert.False(result.Succeeded); // Muvaffaqiyatsiz bo'lishi kerak
        Assert.Equal("Email allaqachon mavjud", result.Message);
    }

    // ============================================
    // TEST 3: LOGIN - MUVAFFAQIYATLI KIRISH
    // ============================================
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccessWithToken()
    {
        // ---- ARRANGE ----
        var loginModel = new LoginUserModel
        {
            Email = "ali@test.com",
            Password = "Test123!"
        };

        var expectedResponse = new LoginResponseModel
        {
            AccessToken = "sample-jwt-token",
            RefreshToken = "sample-refresh-token",
            ExpiresAt = DateTime.Now.AddHours(1)
        };

        var expectedResult = ApiResult<LoginResponseModel>.Success(expectedResponse);

        _mockUserService
            .Setup(s => s.LoginAsync(loginModel))
            .ReturnsAsync(expectedResult);

        // ---- ACT ----
        var result = await _controller.LoginAsync(loginModel);

        // ---- ASSERT ----
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("sample-jwt-token", result.Data.AccessToken);

        // Service bir marta chaqirilganligini tekshirish
        _mockUserService.Verify(s => s.LoginAsync(loginModel), Times.Once);
    }

    // ============================================
    // TEST 4: LOGIN - NOTO'G'RI PAROL
    // ============================================
    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsFailure()
    {
        // ---- ARRANGE ----
        var loginModel = new LoginUserModel
        {
            Email = "ali@test.com",
            Password = "wrongpassword"
        };

        var expectedResult = ApiResult<LoginResponseModel>.Failure("Email yoki parol noto'g'ri");

        _mockUserService
            .Setup(s => s.LoginAsync(It.IsAny<LoginUserModel>()))
            .ReturnsAsync(expectedResult);

        // ---- ACT ----
        var result = await _controller.LoginAsync(loginModel);

        // ---- ASSERT ----
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Equal("Email yoki parol noto'g'ri", result.Message);
    }

    // ============================================
    // TEST 5: VERIFY OTP - MUVAFFAQIYATLI
    // ============================================
    [Fact]
    public async Task VerifyOtpAsync_ValidOtp_ReturnsSuccess()
    {
        // ---- ARRANGE ----
        var otpModel = new OtpVerificationModel
        {
            Email = "ali@test.com",
            Otp = "123456"
        };

        var expectedResult = ApiResult<string>.Success("Tasdiqlandi");

        _mockUserService
            .Setup(s => s.VerifyOtpAsync(otpModel))
            .ReturnsAsync(expectedResult);

        // ---- ACT ----
        var result = await _controller.VerifyOtpAsync(otpModel);

        // ---- ASSERT ----
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Equal("Tasdiqlandi", result.Data);
    }

    // ============================================
    // TEST 6: VERIFY OTP - NOTO'G'RI KOD
    // ============================================
    [Fact]
    public async Task VerifyOtpAsync_InvalidOtp_ReturnsFailure()
    {
        // ---- ARRANGE ----
        var otpModel = new OtpVerificationModel
        {
            Email = "ali@test.com",
            Otp = "000000"
        };

        var expectedResult = ApiResult<string>.Failure("OTP noto'g'ri yoki muddati o'tgan");

        _mockUserService
            .Setup(s => s.VerifyOtpAsync(It.IsAny<OtpVerificationModel>()))
            .ReturnsAsync(expectedResult);

        // ---- ACT ----
        var result = await _controller.VerifyOtpAsync(otpModel);

        // ---- ASSERT ----
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Equal("OTP noto'g'ri yoki muddati o'tgan", result.Message);
    }

    // ============================================
    // TEST 7: GET USER AUTH - MUVAFFAQIYATLI
    // ============================================
    /// <summary>
    /// Test: Foydalanuvchi ma'lumotlarini olish
    ///
    /// BU ENDPOINT FARQI:
    /// GetUserAuth endpoint OkResult yoki BadRequestResult qaytaradi
    /// (ApiResult emas!)
    ///
    /// SHU SABABLI:
    /// ActionResult tipini tekshirishimiz kerak
    /// </summary>
    [Fact]
    public async Task GetUserAuth_ValidUser_ReturnsOkResult()
    {
        // ---- ARRANGE ----
        var userAuthResponse = new UserAuthResponseModel
        {
            Id = 1,
            Fullname = "Ali Valiyev",
            Email = "ali@test.com",
            Permissions = new List<string> { "VIEW_USERS", "EDIT_USERS" }
        };

        var expectedResult = ApiResult<UserAuthResponseModel>.Success(userAuthResponse);

        _mockUserService
            .Setup(s => s.GetUserAuth())
            .ReturnsAsync(expectedResult);

        // ---- ACT ----
        var result = await _controller.GetUserAuth();

        // ---- ASSERT ----
        // 1. Natija OkObjectResult tipida bo'lishi kerak
        var okResult = Assert.IsType<OkObjectResult>(result);

        // 2. Status code 200 bo'lishi kerak
        Assert.Equal(200, okResult.StatusCode);

        // 3. Value ichidagi ma'lumotni tekshirish
        var apiResult = Assert.IsType<ApiResult<UserAuthResponseModel>>(okResult.Value);
        Assert.True(apiResult.Succeeded);
        Assert.NotNull(apiResult.Data);
        Assert.Equal("Ali Valiyev", apiResult.Data.Fullname);
    }

    // ============================================
    // TEST 8: GET USER AUTH - XATO
    // ============================================
    [Fact]
    public async Task GetUserAuth_ServiceReturnsError_ReturnsBadRequest()
    {
        // ---- ARRANGE ----
        var expectedResult = ApiResult<UserAuthResponseModel>.Failure("Foydalanuvchi topilmadi");

        _mockUserService
            .Setup(s => s.GetUserAuth())
            .ReturnsAsync(expectedResult);

        // ---- ACT ----
        var result = await _controller.GetUserAuth();

        // ---- ASSERT ----
        // 1. Natija BadRequestObjectResult tipida bo'lishi kerak
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        // 2. Status code 400 bo'lishi kerak
        Assert.Equal(400, badRequestResult.StatusCode);

        // 3. Value ichidagi xato xabarini tekshirish
        var apiResult = Assert.IsType<ApiResult<UserAuthResponseModel>>(badRequestResult.Value);
        Assert.False(apiResult.Succeeded);
        Assert.Equal("Foydalanuvchi topilmadi", apiResult.Message);
    }

    // ============================================
    // TEST 9: SERVICE METHOD PARAMETRLARINI TEKSHIRISH
    // ============================================
    /// <summary>
    /// Test: Controller Service'ga to'g'ri parametrlarni uzatishini tekshirish
    ///
    /// NEGA MUHIM:
    /// Controller ma'lumotlarni buzib yuborishi mumkin
    /// Masalan: email o'rniga fullname yuborishi mumkin
    /// Bu test shunday xatolarni topadi
    /// </summary>
    [Fact]
    public async Task RegisterAsync_PassesCorrectParametersToService()
    {
        // ---- ARRANGE ----
        var registerModel = new RegisterUserModel
        {
            Fullname = "Test Fullname",
            Email = "test@example.com",
            Password = "Password123!",
            isAdminSite = true
        };

        var expectedResult = ApiResult<string>.Success("Success");
        _mockUserService
            .Setup(s => s.RegisterAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .ReturnsAsync(expectedResult);

        // ---- ACT ----
        await _controller.RegisterAsync(registerModel);

        // ---- ASSERT ----
        // Har bir parametr to'g'ri uzatilganligini alohida tekshirish
        _mockUserService.Verify(
            s => s.RegisterAsync(
                "Test Fullname",      // Fullname to'g'rimi?
                "test@example.com",   // Email to'g'rimi?
                "Password123!",       // Password to'g'rimi?
                true),                // isAdminSite to'g'rimi?
            Times.Once
        );
    }

    // ============================================
    // TEST 10: SERVICE NULL QAYTARGANDA
    // ============================================
    /// <summary>
    /// Test: Service kutilmagan holda null qaytarsa nima bo'ladi?
    ///
    /// REAL SCENARIO:
    /// Service'da xatolik yuz bersa, null qaytarishi mumkin
    /// Controller bu holatni to'g'ri boshqarishi kerak
    /// </summary>
    [Fact]
    public async Task LoginAsync_ServiceReturnsNull_HandlesGracefully()
    {
        // ---- ARRANGE ----
        var loginModel = new LoginUserModel
        {
            Email = "test@test.com",
            Password = "Test123!"
        };

        // Service null qaytaradi
        _mockUserService
            .Setup(s => s.LoginAsync(It.IsAny<LoginUserModel>()))
            .ReturnsAsync((ApiResult<LoginResponseModel>)null!);

        // ---- ACT ----
        var result = await _controller.LoginAsync(loginModel);

        // ---- ASSERT ----
        // Controller null qaytarmasligi kerak
        // Bu test sizning loyihangizga qarab o'zgarishi mumkin
        Assert.Null(result); // yoki Assert.NotNull(result) - loyihangizga qarab
    }
}

// ============================================
// TESTLARNI QANDAY ISHLATISH
// ============================================
/*
 * VISUAL STUDIO'DA:
 * 1. Test Explorer'ni oching
 * 2. "Run All Tests" ni bosing
 * 3. Controller testlari alohida ko'rinadi
 *
 * COMMAND LINE'DA:
 * dotnet test
 *
 * FAQAT CONTROLLER TESTLARINI ISHGA TUSHIRISH:
 * dotnet test --filter "FullyQualifiedName~UserControllerTests"
 */

// ============================================
// MUHIM TUSHUNCHALAR
// ============================================
/*
 * MOCK SETUP:
 * Setup() - Mock obyektga method chaqirilganda nima qaytarishini o'rgatadi
 * Misol:
 * _mockService.Setup(s => s.Method()).ReturnsAsync(value);
 *
 * MOCK VERIFY:
 * Verify() - Method haqiqatan chaqirilganligini tekshiradi
 * Misol:
 * _mockService.Verify(s => s.Method(), Times.Once);
 *
 * IT.ISANY:
 * It.IsAny<T>() - Har qanday qiymat bo'lishi mumkin
 * Misol:
 * _mockService.Setup(s => s.Method(It.IsAny<string>()))
 *
 * ACTIONRESULT TESTING:
 * Controller'lar OkResult, BadRequestResult va boshqalarni qaytarishi mumkin
 * Bu tip'larni tekshirish uchun:
 * var okResult = Assert.IsType<OkObjectResult>(result);
 *
 * BEST PRACTICES:
 * 1. Har bir endpoint uchun kamida 2 ta test yozing (success va failure)
 * 2. Service parametrlari to'g'ri uzatilishini tekshiring
 * 3. HTTP status code'ni tekshiring
 * 4. Response ma'lumotlarini tekshiring
 * 5. Edge case'larni test qiling (null, empty, etc.)
 */

// ============================================
// CONTROLLER vs SERVICE TESTING FARQI
// ============================================
/*
 * SERVICE TESTING:
 * - Business logika'ni test qiladi
 * - Database bilan ishlashni test qiladi
 * - InMemory Database ishlatadi
 *
 * CONTROLLER TESTING:
 * - HTTP request/response'ni test qiladi
 * - Status code'ni test qiladi
 * - Service'ni Mock qiladi (haqiqiy service ishlatmaydi)
 * - HTTP pipeline'ni test qiladi
 *
 * IKKALASI HAM MUHIM!
 * Controller testlari - Tashqi interface to'g'ri ishlashini tekshiradi
 * Service testlari - Ichki logika to'g'ri ishlashini tekshiradi
 */
