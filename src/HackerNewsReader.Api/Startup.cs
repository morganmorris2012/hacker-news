using HackerNewsReader.Core.Interfaces;
using HackerNewsReader.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNewsReader.Api;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Configure memory cache
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1000; // Maximum number of items in cache
            options.ExpirationScanFrequency = TimeSpan.FromMinutes(1); // How often to scan for expired items
            options.CompactionPercentage = 0.2; // Remove 20% of items when size limit is reached
        });

        services.AddHttpClient();
        services.AddScoped<IHackerNewsService, HackerNewsService>();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngularApp",
                builder => builder
                    .WithOrigins("http://localhost:4200")
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAngularApp");
        app.UseAuthorization();
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}