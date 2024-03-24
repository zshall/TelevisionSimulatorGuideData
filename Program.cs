namespace TelevisionSimulatorGuideData;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddOutputCache();
        builder.Services.AddSingleton<ProgramGuide>();

        var app = builder.Build();
        app.UseOutputCache();

        // Configure the HTTP request pipeline.

        //app.UseHttpsRedirection();



        app.MapGet("/guide",
            (ProgramGuide guide, IConfiguration config) => guide.GetData(config.GetSection("ListingsFile").Value)).CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(5)));

        app.Run();
    }
}