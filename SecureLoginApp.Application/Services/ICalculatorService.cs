namespace SecureLoginApp.Application.Services;

public interface ICalculatorService
{
    int Add(int a, int b);

    int Subtract(int a, int b);

    int Multiply(int a, int b);

    double Divide(int a, int b);

    double CalculatePercentage(int value, int percentage);

    int Square(int number);
}
