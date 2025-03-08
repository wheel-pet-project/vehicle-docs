using Api.Interceptors;
using Api.Services;

namespace Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;
        
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<ExceptionHandlerInterceptor>();
            options.Interceptors.Add<TracingInterceptor>();
            options.Interceptors.Add<LoggingInterceptor>();
        });

        services
            .RegisterPostgresContextAndDataSource();

        var app = builder.Build();
        
        app.MapGrpcService<GreeterService>();

        app.Run();
    }
}