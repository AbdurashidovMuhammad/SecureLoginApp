using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SecureLoginApp.Application.Helpers.GenerateJwt;
using SecureLoginApp.Application;
using SecureLoginApp.DataAcces;
using SecureLoginApp.Application.Common;
using System.Text;
using SecureLoginApp.Application.Helpers;
using Microsoft.Extensions.Options;
using Minio;
using SecureLoginApp.Application.Services;

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
            builder.Services.AddHttpContextAccessor();

            builder.Services.Configure<EmailConfiguration>(configuration.GetSection("EmailConfiguration"));
            builder.Services.Configure<JwtOption>(configuration.GetSection("JwtOption"));
            builder.Services.Configure<MinioSettings>(configuration.GetSection("MinioSettings"));

            builder.Services.AddSingleton<IMinioClient>(sp =>
            {
                var minioSettings = sp.GetRequiredService<IOptions<MinioSettings>>().Value;
                var client = new MinioClient()
                    .WithEndpoint(minioSettings.Endpoint)
                    .WithCredentials(minioSettings.AccessKey, minioSettings.SecretKey);

                if (minioSettings.UseSsl)
                {
                    client = client.WithSSL();
                }
                return client.Build();
            });

            builder.Services.AddApplication(configuration);
            builder.Services.AddDataAccess(configuration);
            builder.Services.AddAuth(builder.Configuration);

            // ============================================
            // CORS (Cross-Origin Resource Sharing) SOZLAMALARI
            // ============================================
            // CORS - bu brauzerning xavfsizlik mexanizmi bo'lib, bir domendan 
            // (masalan: http://localhost:3000) boshqa domenga 
            // (masalan: https://api.myapp.com) so'rov yuborishni nazorat qiladi.

            // Misol: Agar frontend React ilovangiz localhost:3000 da ishlayotgan bo'lsa
            // va backend API localhost:5000 da bo'lsa, brauzer sukut bo'yicha 
            // bunday so'rovlarni bloklaydi. CORS shu muammoni hal qiladi.

            builder.Services.AddCors(options =>
            {
                // "CorsPolicy" nomli yangi policy yaratamiz
                options.AddPolicy("CorsPolicy", policy =>
                {
                    // AllowAnyOrigin() - BARCHA domenlardan so'rovlarga ruxsat beradi
                    // ⚠️ DIQQAT: Production muhitida bu xavfli!
                    // Production uchun aniq domenlarni ko'rsating:
                    // policy.WithOrigins("https://myapp.com", "https://admin.myapp.com")
                    policy.AllowAnyOrigin()

                    // AllowAnyMethod() - barcha HTTP metodlarga ruxsat (GET, POST, PUT, DELETE va h.k.)
                    .AllowAnyMethod()

                    // AllowAnyHeader() - barcha HTTP headerslarga ruxsat 
                    // (Content-Type, Authorization va boshqalar)
                    .AllowAnyHeader();
                });


                // QOSHIMCHA: Xavfsizroq CORS sozlamasi namunasi (o'quvchilarga ko'rsatish uchun)
                // Production muhiti uchun tavsiya etiladi:

                options.AddPolicy("StrictCorsPolicy", policy =>
                {
                    // Faqat aniq domenlardan so'rovlarga ruxsat
                    policy.WithOrigins(
                        "https://myapp.com",
                        "https://www.myapp.com",
                        "http://localhost:3000"
                    )
                    // Faqat kerakli metodlarga ruxsat
                    .WithMethods("GET", "POST", "PUT", "DELETE")
                    // Faqat kerakli headerlarga ruxsat
                    .WithHeaders("Content-Type", "Authorization")
                    // Credentials (Cookie, Authorization headers) bilan ishlash
                    .AllowCredentials();
                });


            });

            var app = builder.Build();

            using var scope = app.Services.CreateScope();

            app.UseSwagger(options => options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);
            app.UseSwaggerUI();
            app.UseHttpsRedirection();

            // ============================================
            // MIDDLEWARE PIPELINE TARTIBI MUHIM!
            // ============================================
            // CORS middleware Authentication va Authorization dan OLDIN chaqirilishi kerak
            // Chunki preflight OPTIONS so'rovlari autentifikatsiyadan o'tmaydi
            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // ============================================
            // MINIMAL API NAMUNALARI
            // ============================================
            // Minimal API - bu Controller sinflarisiz oddiy endpoint yaratish usuli.
            // Kichik mikroservislar va oddiy API'lar uchun juda qulay.

            // NAMUNA 1: Oddiy GET so'rovi
            // URL: GET /hello
            app.MapGet("/hello", () => "Salom Dunyo!")
                .WithName("GetHello");                    // Endpoint nomi (Swagger uchun)

            // NAMUNA 2: Parametr bilan GET so'rovi
            // URL: GET /hello/Aziz -> natija: "Salom, Aziz!"
            app.MapGet("/hello/{name}", (string name) => $"Salom, {name}!")
                .WithName("GetHelloWithName");

            // NAMUNA 3: POST so'rovi - Ma'lumot qabul qilish
            // URL: POST /users
            // Body: { "name": "Aziz", "email": "aziz@example.com" }
            app.MapPost("/users", (UserDto user) =>
            {
                // Bu yerda odatda ma'lumotlar bazasiga saqlash logikasi bo'ladi
                return Results.Ok(new
                {
                    Message = "Foydalanuvchi muvaffaqiyatli yaratildi",
                    User = user
                });
            })
            .WithName("CreateUser");

            // NAMUNA 4: Dependency Injection bilan ishlash
            // Service'lardan foydalanish
            app.MapGet("/time", (TimeProvider timeProvider) =>
            {
                return Results.Ok(new
                {
                    ServerTime = DateTime.Now,
                    Message = "Hozirgi server vaqti"
                });
            })
            .WithName("GetServerTime");

            // NAMUNA 5: Autentifikatsiya talab qiladigan endpoint
            // Faqat tizimga kirgan foydalanuvchilar uchun
            app.MapGet("/secure-data", () => "Bu maxfiy ma'lumot!")
                .RequireAuthorization()                   // JWT token talab qiladi
                .WithName("GetSecureData");

            // NAMUNA 6: PUT so'rovi - Ma'lumotni yangilash
            // URL: PUT /users/123
            app.MapPut("/users/{id}", (int id, UserDto user) =>
            {
                return Results.Ok(new
                {
                    Message = $"Foydalanuvchi {id} yangilandi",
                    User = user
                });
            })
            .WithName("UpdateUser");

            // NAMUNA 7: DELETE so'rovi
            // URL: DELETE /users/123
            app.MapDelete("/users/{id}", (int id) =>
            {
                return Results.Ok(new
                {
                    Message = $"Foydalanuvchi {id} o'chirildi"
                });
            })
            .WithName("DeleteUser");

            // NAMUNA 8: Guruh (Group) bilan ishlash
            // Bir xil yo'ldan boshlanadigan endpointlarni guruhlash
            var apiGroup = app.MapGroup("/api/v1");

            apiGroup.MapGet("/products", () => "Mahsulotlar ro'yxati")
                .WithName("GetProducts");

            apiGroup.MapGet("/products/{id}", (int id) => $"Mahsulot ID: {id}")
                .WithName("GetProductById");

            // NAMUNA 9: Xatoliklarni qaytarish
            app.MapGet("/error-demo", () =>
            {
                // 404 Not Found qaytarish
                return Results.NotFound(new { Error = "Ma'lumot topilmadi" });

                // Yoki 400 Bad Request:
                // return Results.BadRequest(new { Error = "Noto'g'ri so'rov" });

                // Yoki 500 Internal Server Error:
                // return Results.Problem("Serverda xatolik yuz berdi");
            })
            .WithName("ErrorDemo");

            // ============================================
            // MINIMAL API vs CONTROLLER
            // ============================================
            /*
             * MINIMAL API AFZALLIKLARI:
             * ✅ Kam kod yozish kerak
             * ✅ Tezroq ishlaydi (kichik overhead)
             * ✅ Oddiy endpoint'lar uchun juda qulay
             * ✅ Mikroservislar uchun ideal
             * 
             * CONTROLLER AFZALLIKLARI:
             * ✅ Murakkab biznes logika uchun yaxshi
             * ✅ Kod tashkiloti yaxshiroq (bir nechta metodlar bitta sinfda)
             * ✅ Attribute-based routing (masalan: [HttpGet], [Authorize])
             * ✅ Model validation avtomatik ishlaydi
             * 
             * QACHON QAYSI BIRINI ISHLATISH KERAK?
             * - Kichik API, oddiy CRUD operatsiyalar -> Minimal API
             * - Katta enterprise loyiha, murakkab logika -> Controller
             * - Ikkalasini birgalikda ham ishlatish mumkin!
             */

            app.Run();
        }
    }

    // Minimal API uchun yordamchi DTO klassi (o'quvchilarga ko'rsatish uchun)
    public record UserDto(string Name, string Email);
}