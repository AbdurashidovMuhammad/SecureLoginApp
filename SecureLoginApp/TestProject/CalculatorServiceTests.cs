using SecureLoginApp.Application.Services;
using SecureLoginApp.Application.Services.Impl;
using FluentAssertions;

namespace TestProject;


// ============================================
// UNIT TEST NIMA?
// ============================================
// Unit Test - bu kodning kichik qismini (bir funksiyani) test qilish
//
// NIMA UCHUN KERAK?
// ✅ Kod to'g'ri ishlashini tekshirish
// ✅ Xatolarni erta topish
// ✅ Kodni o'zgartirsak, eski qismlar buzilganini bilish
// ✅ Ishonchli dastur yaratish
// ✅ Dokumentatsiya vazifasini bajaradi - test o'qib, kod qanday ishlashini tushunish mumkin
//
// MASALAN:
// Add(2, 3) = 5 bo'lishi kerakmi? Test orqali tekshiramiz!

public class CalculatorServiceTests
{
    // ============================================
    // TEST UCHUN KERAKLI OBYEKT
    // ============================================
    // Bu yerda Mock kerak emas - CalculatorService juda oddiy!
    // Hech qanday dependency yo'q (database, email service, va h.k.)

    private readonly ICalculatorService _calculator;

    // ============================================
    // CONSTRUCTOR - Har bir test boshlanishidan oldin ishlaydi
    // ============================================
    public CalculatorServiceTests()
    {
        // HAQIQIY CalculatorService yaratamiz
        // Mock kerak emas - chunki dependency yo'q
        _calculator = new CalculatorService();
    }

    // ============================================
    // AAA PATTERN - Test yozish namunasi
    // ============================================
    // Arrange  (Tayyorgarlik) - Ma'lumotlarni tayyorlash
    // Act      (Harakat)      - Metodini chaqirish
    // Assert   (Tekshirish)   - Natijani tekshirish

    // ============================================
    // ADD (QO'SHISH) TESTLARI
    // ============================================

    [Fact] // ⬅️ Bu oddiy test (parametrsiz)
    public void Add_TwoPositiveNumbers_ShouldReturnSum()
    {
        // ============================================
        // ARRANGE (Tayyorgarlik)
        // ============================================
        // Test uchun ma'lumotlarni tayyorlaymiz
        int a = 5;
        int b = 3;
        int expectedResult = 8; // Kutilayotgan natija

        // ============================================
        // ACT (Harakat)
        // ============================================
        // Test qilinadigan metodini chaqiramiz
        int actualResult = _calculator.Add(a, b);

        // ============================================
        // ASSERT (Tekshirish)
        // ============================================
        // Natija kutilgandek ekanligini tekshiramiz
        actualResult.Should().Be(expectedResult);

        // Yoki oddiy usul:
        // Assert.Equal(expectedResult, actualResult);
    }

    [Fact]
    public void Add_TwoNegativeNumbers_ShouldReturnNegativeSum()
    {
        // ARRANGE
        int a = -5;
        int b = -3;
        int expectedResult = -8;

        // ACT
        int actualResult = _calculator.Add(a, b);

        // ASSERT
        actualResult.Should().Be(expectedResult);
    }

    [Fact]
    public void Add_PositiveAndNegative_ShouldReturnCorrectResult()
    {
        // ARRANGE
        int a = 10;
        int b = -3;
        int expectedResult = 6;

        // ACT
        int actualResult = _calculator.Add(a, b);

        // ASSERT
        actualResult.Should().Be(expectedResult);
    }

    [Fact]
    public void Add_ZeroAndNumber_ShouldReturnNumber()
    {
        // ARRANGE
        int a = 0;
        int b = 5;
        int expectedResult = 5;

        // ACT
        int actualResult = _calculator.Add(a, b);

        // ASSERT
        actualResult.Should().Be(expectedResult);
    }

    // ============================================
    // THEORY - Ko'p ma'lumot bilan bir test
    // ============================================
    // [Theory] - Bu bir xil testni turli ma'lumotlar bilan yuborish
    // [InlineData] - Har bir qator = 1 ta test

    [Theory]
    [InlineData(2, 3, 5)]      // Test 1: 2 + 3 = 5
    [InlineData(10, 20, 30)]   // Test 2: 10 + 20 = 30
    [InlineData(-5, 5, 0)]     // Test 3: -5 + 5 = 0
    [InlineData(0, 0, 0)]      // Test 4: 0 + 0 = 0
    [InlineData(100, -50, 50)] // Test 5: 100 + (-50) = 50
    public void Add_VariousInputs_ShouldReturnCorrectSum(int a, int b, int expected)
    {
        // ACT
        int result = _calculator.Add(a, b);

        // ASSERT
        result.Should().Be(expected);
    }

    // ============================================
    // SUBTRACT (AYIRISH) TESTLARI
    // ============================================

    [Fact]
    public void Subtract_TwoPositiveNumbers_ShouldReturnDifference()
    {
        // ARRANGE
        int a = 10;
        int b = 3;
        int expectedResult = 7;

        // ACT
        int actualResult = _calculator.Subtract(a, b);

        // ASSERT
        actualResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(10, 5, 5)]     // 10 - 5 = 5
    [InlineData(5, 10, -5)]    // 5 - 10 = -5
    [InlineData(0, 5, -5)]     // 0 - 5 = -5
    [InlineData(-5, -3, -2)]   // -5 - (-3) = -2
    public void Subtract_VariousInputs_ShouldReturnCorrectDifference(int a, int b, int expected)
    {
        // ACT
        int result = _calculator.Subtract(a, b);

        // ASSERT
        result.Should().Be(expected);
    }

    // ============================================
    // MULTIPLY (KO'PAYTIRISH) TESTLARI
    // ============================================

    [Fact]
    public void Multiply_TwoPositiveNumbers_ShouldReturnProduct()
    {
        // ARRANGE
        int a = 4;
        int b = 5;
        int expectedResult = 20;

        // ACT
        int actualResult = _calculator.Multiply(a, b);

        // ASSERT
        actualResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(3, 4, 12)]      // 3 * 4 = 12
    [InlineData(-3, 4, -12)]    // -3 * 4 = -12
    [InlineData(-3, -4, 12)]    // -3 * -4 = 12
    [InlineData(0, 100, 0)]     // 0 * 100 = 0
    [InlineData(1, 999, 999)]   // 1 * 999 = 999
    public void Multiply_VariousInputs_ShouldReturnCorrectProduct(int a, int b, int expected)
    {
        // ACT
        int result = _calculator.Multiply(a, b);

        // ASSERT
        result.Should().Be(expected);
    }

    // ============================================
    // DIVIDE (BO'LISH) TESTLARI
    // ============================================

    [Fact]
    public void Divide_TwoPositiveNumbers_ShouldReturnQuotient()
    {
        // ARRANGE
        int a = 10;
        int b = 2;
        double expectedResult = 5.0;

        // ACT
        double actualResult = _calculator.Divide(a, b);

        // ASSERT
        actualResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(10, 2, 5.0)]      // 10 / 2 = 5
    [InlineData(10, 3, 3.333)]    // 10 / 3 = 3.333... (taxminan)
    [InlineData(7, 2, 3.5)]       // 7 / 2 = 3.5
    [InlineData(-10, 2, -5.0)]    // -10 / 2 = -5
    [InlineData(10, -2, -5.0)]    // 10 / -2 = -5
    public void Divide_VariousInputs_ShouldReturnCorrectQuotient(int a, int b, double expected)
    {
        // ACT
        double result = _calculator.Divide(a, b);

        // ASSERT
        // BeApproximately - taxminan teng (float/double uchun)
        result.Should().BeApproximately(expected, 0.01); // 0.01 - xatolik chegarasi
    }

    // ============================================
    // XATO HOLAT TESTLARI - Exception (Istisno)
    // ============================================
    // Ba'zi holatlar XATO berishi kerak!
    // Masalan: 0 ga bo'lish

    [Fact]
    public void Divide_ByZero_ShouldThrowException()
    {
        // ARRANGE
        int a = 10;
        int b = 0; // ⬅️ 0 ga bo'lish - XATO!

        // ACT & ASSERT
        // FluentAssertions - Exception kutamiz
        Action act = () => _calculator.Divide(a, b);

        // Bu xato bo'lishi KERAK!
        act.Should().Throw<DivideByZeroException>()
           .WithMessage("0 ga bo'lish mumkin emas!");
    }

    // ============================================
    // PERCENTAGE (FOIZ) TESTLARI
    // ============================================

    [Fact]
    public void CalculatePercentage_ValidInputs_ShouldReturnCorrectPercentage()
    {
        // ARRANGE
        int value = 200;      // Asosiy qiymat
        int percentage = 15;  // Foiz
        double expectedResult = 30.0; // 200 ning 15% = 30

        // ACT
        double actualResult = _calculator.CalculatePercentage(value, percentage);

        // ASSERT
        actualResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(100, 10, 10.0)]    // 100 ning 10% = 10
    [InlineData(200, 50, 100.0)]   // 200 ning 50% = 100
    [InlineData(150, 20, 30.0)]    // 150 ning 20% = 30
    [InlineData(1000, 5, 50.0)]    // 1000 ning 5% = 50
    [InlineData(75, 100, 75.0)]    // 75 ning 100% = 75
    public void CalculatePercentage_VariousInputs_ShouldReturnCorrectPercentage(
        int value, int percentage, double expected)
    {
        // ACT
        double result = _calculator.CalculatePercentage(value, percentage);

        // ASSERT
        result.Should().BeApproximately(expected, 0.01);
    }

    // ============================================
    // SQUARE (KVADRAT) TESTLARI
    // ============================================

    [Fact]
    public void Square_PositiveNumber_ShouldReturnSquare()
    {
        // ARRANGE
        int number = 5;
        int expectedResult = 25; // 5² = 25

        // ACT
        int actualResult = _calculator.Square(number);

        // ASSERT
        actualResult.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(0, 0)]       // 0² = 0
    [InlineData(1, 1)]       // 1² = 1
    [InlineData(2, 4)]       // 2² = 4
    [InlineData(3, 9)]       // 3² = 9
    [InlineData(10, 100)]    // 10² = 100
    [InlineData(-5, 25)]     // (-5)² = 25
    public void Square_VariousInputs_ShouldReturnCorrectSquare(int input, int expected)
    {
        // ACT
        int result = _calculator.Square(input);

        // ASSERT
        result.Should().Be(expected);
    }
}
