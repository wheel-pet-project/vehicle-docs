using Api.Adapters.Grpc;
using Api.Interceptors;
using Application.UseCases.Queries.DapperMappingExtensions;
using Dapper;

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
            .RegisterPostgresContextAndDataSource()
            .RegisterS3Storage()
            .RegisterMediatorAndHandlers()
            .RegisterDomainServices()
            .RegisterInboxAndOutboxBackgroundJobs()
            .RegisterSerilog()
            .RegisterRepositories()
            .RegisterUnitOfWork()
            .RegisterInbox()
            .RegisterEnumMappers()
            .RegisterMassTransit()
            .RegisterTelemetry()
            .RegisterHealthCheckV1()
            .RegisterImageValidator()
            .RegisterTimeProvider();

        var app = builder.Build();

        app.MapGrpcService<VehicleDocumentsV1>();
        app.MapGrpcHealthChecksService();

        RegisterDapperMapping();

        app.Run();

        void RegisterDapperMapping()
        {
            SqlMapper.AddTypeHandler(new DateOnlyMapper());
        }
    }
}