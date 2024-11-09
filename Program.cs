using Microsoft.AspNetCore.ResponseCompression;

namespace TelevisionSimulatorGuideData;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddHostedService<ListingFileService>();
        builder.Services.AddOutputCache();
        builder.Services.AddCors();
        builder.Services.AddSingleton<ProgramGuide>();
        builder.Services.AddResponseCompression(options =>
        {
            options.Providers.Add<GzipCompressionProvider>();
            options.EnableForHttps = true;
        });

        var app = builder.Build();
        app.UseResponseCompression();
        app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        app.UseOutputCache();

        // Configure the HTTP request pipeline.

        app.MapGet("/guide", (ProgramGuide guide) => guide.GetData())
            .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(5)));

        app.Run();
    }
}