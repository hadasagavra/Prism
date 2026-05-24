using Microsoft.OpenApi.Models;
using Repository.Interfaces;
using Repository.Repositories;
using Service.Services;
using DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Service.Interfaces;
using Repository; // ודאי שזה ה-Namespace של ה-Extension Method ל-Repository
using Service;    // ודאי שזה ה-Namespace של ה-Extension Method ל-Services

namespace Prism
{
    public class Program
    {
        // שינוי 1: הפיכת ה-Main ל-async Task כדי שנוכל להריץ את ה-InitializeModelAsync ב-Startup
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. הגדרות בסיסיות (Swagger וכדומה - ללא שינוי)
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Prism API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Enter 'Bearer' [space] and then your token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new string[] {}
                    }
                });
            });

            // 2. חיבור למסד הנתונים ו-JWT (ללא שינוי)
            var connectionString = builder.Configuration.GetConnectionString("database-work");
            builder.Services.AddDbContext<IContext, PrismDbContext>(options =>
                options.UseSqlServer(connectionString));

            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var secretKey = jwtSettings["SecretKey"] ?? "YourVerySecretKeyThatIsAtLeast32CharsLong";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"] ?? "EmailClassificationAPI",
                    ValidAudience = jwtSettings["Audience"] ?? "EmailClassificationClient",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // 3. רישום השכבות (חשוב לוודא שיש using מתאים למעלה כדי שהשגיאה האדומה תעלם)
            builder.Services.AddRepository();
            builder.Services.AddServices();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReact", policy =>
                {
                    policy.WithOrigins(
                            "https://localhost:3000",
                            "http://localhost:3000"
                          )
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            var app = builder.Build();

            // 4. הגדרות Swagger (ללא שינוי)
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Prism API V1");
                    c.RoutePrefix = "swagger";
                });
            }
            app.UseCors("AllowReact");
            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // שינוי 2: קריאה לטעינת המודל לזיכרון מיד עם עליית השרת
            //
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var naiveBayesService = services.GetRequiredService<INaiveBayesService>();
                    // שימוש ב-await בזכות ה-async Task Main
                    await naiveBayesService.InitializeModelAsync();
                    Console.WriteLine("--- Naive Bayes model initialized successfully in memory ---");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--- Error during model initialization: {ex.Message} ---");
                }
            }

            // שינוי 3: שימוש ב-await app.RunAsync() במקום app.Run()
            await app.RunAsync();
        }
    }
}