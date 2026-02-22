using Microsoft.OpenApi.Models;
using Repository.Interfaces;
using Repository.Repositories;
using Service.Services;
using DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Prism
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. הגדרות בסיסיות
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Prism API", Version = "v1" });

                // הוספת הגדרת האבטחה לסווגר
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
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // 2. חיבור למסד הנתונים - משיכת הכתובת מה-appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("database-work");
            // רישום ה-DbContext בצורה נכונה (Scoped)
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
            // 3. שימוש בקיצורי הדרך שיצרת בשכבות האחרות
            builder.Services.AddRepository(); // רושם את כל ה-Repositories בשורה אחת
            builder.Services.AddServices();   // רושם את ה-Services ואת ה-AutoMapper

            var app = builder.Build();

            // 4. הגדרות Swagger
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Prism API V1");
                    c.RoutePrefix = "swagger"; // מאפשר גישה לסווגר דרך הכתובת הראשית
                });
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // אימות והרשאות (כרגע כבויים עד שתצטרכי אותם)
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
