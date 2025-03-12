using Api.Interceptors;
using Application.UseCases.Queries.DapperMappingExtensions;

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

        // app.MapGrpcService<GreeterService>();

        RegisterDapperMapping();

        app.Run();

        void RegisterDapperMapping()
        {
            Dapper.SqlMapper.AddTypeHandler(new DateOnlyMapper());
        }
    }
}