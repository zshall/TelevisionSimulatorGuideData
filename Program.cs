using Microsoft.AspNetCore.ResponseCompression;

namespace TelevisionSimulatorGuideData;

public class Program
{
    public static void Main(string[] args)
    {
        try {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            // Add services to the container.
            builder.Services.AddHostedService<ListingFileService>();
            builder.Services.AddOutputCache();
            builder.Services.AddCors();
            builder.Services.AddSingleton<ProgramGuide>();
            builder.Services.AddResponseCompression(options => {
                options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = true;
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            app.UseResponseCompression();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseOutputCache();

            // Configure the HTTP request pipeline.

            app.MapGet("/guide", (
                    ProgramGuide guide,
                    DateTimeOffset? now,
                    int? numberOfTimeslots,
                    int? minutesPerTimeslot,
                    int? lowerChannelLimitInclusive,
                    int? upperChannelLimitExclusive) => {
                    return guide.GetData(
                        now,
                        numberOfTimeslots ?? 3,
                        minutesPerTimeslot ?? 30,
                        lowerChannelLimitInclusive,
                        upperChannelLimitExclusive);
                })
                .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(5)));

            app.Run();
        } catch (Exception ex) {
            // Log the exception
            Console.WriteLine($"Unhandled exception: {ex}");
            throw;
        }
    }
}