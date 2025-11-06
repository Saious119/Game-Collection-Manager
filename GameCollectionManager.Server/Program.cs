using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using GameCollectionManagerAPI.Services;
using GameCollectionManagerAPI.Data;

namespace GameCollectionManagerAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

            var builder = WebApplication.CreateBuilder(args);

            var config = builder.Configuration;
            builder.Services.AddDbContext<DataContext>();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSingleton<IDB_Service, DB_Services>();
            builder.Services.AddSingleton<IMetaCritic_Services, MetaCritic_Services>();
            builder.Services.AddSingleton<IIGDB_Service, IGDB_Service>();
            builder.Services.AddSingleton<StaticVariables>();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("*")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors(MyAllowSpecificOrigins);
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();

        }
    }
}
