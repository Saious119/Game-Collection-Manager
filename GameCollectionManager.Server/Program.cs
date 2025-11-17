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

            // Remove session - not needed for Blazor WASM
            // builder.Services.AddDistributedMemoryCache();
            // builder.Services.AddSession(...);

            // Add JWT Authentication
            var jwtSettings = config.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowClient", builder =>
                    builder.WithOrigins("https://localhost:7176", "http://localhost:5272", "https://localhost:5000", "http://localhost:5000")
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
            
            app.UseHttpsRedirection();
            app.UseAuthentication(); // Add before UseAuthorization
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
