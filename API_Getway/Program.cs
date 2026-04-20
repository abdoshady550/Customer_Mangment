
using Scalar.AspNetCore;

namespace API_Getway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();


        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        var app = builder.Build();

        app.MapDefaultEndpoints();

        app.MapOpenApi();
        app.MapScalarApiReference();


        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapReverseProxy();

        app.MapControllers();

        app.Run();
    }
}
