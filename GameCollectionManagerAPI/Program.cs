using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GameCollectionManagerAPI.Services;

namespace GameCollectionManagerAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
            //builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
            var config = builder.Configuration;
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSingleton<IDB_Service, DB_Services>();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();

        }
    }
}
