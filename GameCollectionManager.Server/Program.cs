using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using GameCollectionManagerAPI.Services;
using GameCollectionManagerAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GameCollectionManagerAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var config = builder.Configuration;
            builder.Services.AddDbContext<DataContext>();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddSingleton<IDB_Service, DB_Services>();
            builder.Services.AddSingleton<IMetaCritic_Services, MetaCritic_Services>();
            builder.Services.AddSingleton<IIGDB_Service, IGDB_Service>();
            builder.Services.AddSingleton<StaticVariables>();
            builder.Services.AddSwaggerGen();

            // Add JWT Authentication
            var jwtSettings = config.GetSection("JwtSettings");
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
                            jwtSettings["SecretKey"] ?? 
                            throw new InvalidOperationException("JWT SecretKey not configured");
            
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? jwtSettings["Issuer"],
                        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                    };
                });

            builder.Services.AddAuthorization();

            // Update CORS for production
            var allowedOrigins = new List<string>
            {
                "https://localhost:7176", 
                "http://localhost:5272", 
                "https://localhost:5000", 
                "http://localhost:5000"
            };
            
            // Add production client URL from environment
            var productionClientUrl = Environment.GetEnvironmentVariable("CLIENT_URL");
            if (!string.IsNullOrEmpty(productionClientUrl))
            {
                allowedOrigins.Add(productionClientUrl);
            }

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowClient", builder =>
                    builder.WithOrigins(allowedOrigins.ToArray())
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials());
            });

            var app = builder.Build();
            
            app.UseCors("AllowClient");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            // Render uses HTTP internally, terminates SSL at proxy
            // app.UseHttpsRedirection(); // Comment out for Render
            
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
