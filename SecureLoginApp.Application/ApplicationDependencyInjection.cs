
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecureLoginApp.Application.Common;
using SecureLoginApp.Application.Helpers.GenerateJwt;
using SecureLoginApp.Application.Helpers.PasswordHashers;
using SecureLoginApp.Application.Security;
using SecureLoginApp.Application.Services;
using SecureLoginApp.Application.Services.Impl;


namespace SecureLoginApp.Application
{
    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddServices();
            services.RegisterCashing();
            services.Configure<JwtOption>(configuration.GetSection("JwtOption"));
            return services;
        }

        private static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IJwtTokenHandler, JwtTokenHandler>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFileStorageService, MinioFileStorageService>();
        }

        private static void RegisterCashing(this IServiceCollection services)
        {
            services.AddMemoryCache();
        }

        public static void AddEmailConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration.GetSection("SmtpSettings").Get<SmtpSettings>());
        }
    }
}