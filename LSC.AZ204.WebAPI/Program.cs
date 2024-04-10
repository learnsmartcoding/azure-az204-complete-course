using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using LSC.AZ204.WebAPI.Common.Filters;
using LSC.AZ204.WebAPI.Data;
using LSC.AZ204.WebAPI.Middleware;
using LSC.AZ204.WebAPI.RefreshService;
using LSC.AZ204.WebAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace Planner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configuration = builder.Configuration;
            // Add services to the container.       

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add(typeof(HttpGlobalExceptionFilter));
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            builder.Services.AddDbContext<AZ204DemoDbContext>(options =>
                 options.UseSqlServer(configuration.GetConnectionString("DbContext"))
                 .EnableSensitiveDataLogging() // Only for development purpose, comment this as it is sensitive
                 );

            builder.Services.AddSingleton<IKeyVaultSecretService, KeyVaultSecretService>(); // Register the original configuration
            builder.Services.AddScoped<IKeyVaultSecretRefreshService, KeyVaultSecretService>();

            builder.Services.AddApplicationInsightsTelemetry();

            builder.Services.AddCors(o => o.AddPolicy("default", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            // Register the KeyVaultSecretRefreshService as a hosted background service
            //builder.Services.AddHostedService<KeyVaultSecretRefreshService>();

            var app = builder.Build();

            // Use the KeyVaultSecretsMiddleware to invoke the RefreshSecrets method during application startup
            //app.UseMiddleware<KeyVaultSecretsMiddleware>();

            // Call the RefreshSecrets method during application startup
            var keyVaultSecretService = app.Services.GetRequiredService<IKeyVaultSecretService>();
            //keyVaultSecretService.RefreshSecrets();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("default");

            app.MapControllers();

            app.Run();
        }
    }
}