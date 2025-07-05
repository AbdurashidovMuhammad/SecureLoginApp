using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SecureLoginApp.Application.Helpers.GenerateJwt;
using SecureLoginApp.Application;
using SecureLoginApp.DataAcces; // To'g'ri nom bo'lishi kerak, ehtimol DataAccess emas, DataAcces. Iltimos, tekshiring.
using SecureLoginApp.Application.Common;
using System.Text;
using SecureLoginApp.Application.Helpers;

namespace SecureLoginApp.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwagger();

            builder.Services.AddHttpContextAccessor(); // Faqat bir marta ro'yxatdan o'tkaziladi
            builder.Services.Configure<EmailConfiguration>(configuration.GetSection("EmailConfiguration"));
            builder.Services.Configure<JwtOption>(configuration.GetSection("JwtOption"));

            builder.Services.AddApplication(configuration);
            builder.Services.AddDataAccess(configuration);

            builder.Services.AddAuth(builder.Configuration);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyOrigin()//WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            using var scope = app.Services.CreateScope();

            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI(c =>
            //    {
            //        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Secure Login API v1");
            //    });
            //}

            app.UseSwagger(options => options.OpenApiVersion =
Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            // Apply CORS middleware before authentication
            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}