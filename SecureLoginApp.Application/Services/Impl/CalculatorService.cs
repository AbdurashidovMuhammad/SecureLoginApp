namespace SecureLoginApp.Application.Services.Impl;

public class CalculatorService : ICalculatorService
{
    public int Add(int a, int b)
    {
        return a + b;
    }

    public int Subtract(int a, int b)
    {
        return a - b;
    }

    public int Multiply(int a, int b)
    {
        return a * b;
    }

    public double Divide(int a, int b)
    {
        if (b == 0)
        {
            throw new DivideByZeroException("0 ga bo'lish mumkin emas!");
        }

        return (double)a / b;
    }

    public double CalculatePercentage(int value, int percentage)
    {
        return (double)value * percentage / 100;
    }

    public int Square(int number)
    {
        return number * number;
    }
}