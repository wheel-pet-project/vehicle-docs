using Infrastructure.Adapters.Postgres;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterPostgresContextAndDataSource(this IServiceCollection services)
    {
        services.AddScoped<NpgsqlDataSource>(_ =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder
            {
                ConnectionStringBuilder =
                {
                    ApplicationName = "Vehicle_documents#" + Environment.MachineName,
                    Host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost",
                    Port = int.Parse(Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5450"),
                    Database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "vehicledocuments_db",
                    Username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres",
                    Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "password",
                    BrowsableConnectionString = false
                }
            };

            return dataSourceBuilder.Build();
        });

        var serviceProvider = services.BuildServiceProvider();
        var dataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();

        services.AddDbContext<DataContext>(optionsBuilder =>
        {
            optionsBuilder.UseNpgsql(dataSource,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(DataContext).Assembly));
            optionsBuilder.EnableSensitiveDataLogging();
        });

        return services;
    }

    public static IServiceCollection RegisterMediatorAndHandlers(this IServiceCollection services)
    {
        // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AddModelHandler).Assembly));
        
        return services;
    }

    public static IServiceCollection RegisterSerilog(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(theme: AnsiConsoleTheme.Sixteen)
            .WriteTo.MongoDBBson(Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")
                                 ?? "mongodb://carsharing:password@localhost:27017/drivinglicense?authSource=admin",
                "logs",
                LogEventLevel.Verbose,
                50,
                TimeSpan.FromSeconds(10))
            .CreateLogger();
        services.AddSerilog();

        return services;
    }

    public static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        // services.AddTransient<IModelRepository, ModelRepository>();
        // services.AddTransient<IVehicleRepository, VehicleRepository>();

        return services;
    }

    public static IServiceCollection RegisterUnitOfWork(this IServiceCollection services)
    {
        // services.AddTransient<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection RegisterMappers(this IServiceCollection services)
    {
        // services.AddScoped<ColorMapper>();
        // services.AddScoped<StatusMapper>();
        
        return services;
    }

    public static IServiceCollection RegisterMassTransit(this IServiceCollection services)
    {
        // const string modelCreatedTopic = "model-created-topic";
        // const string modelCategoryUpdatedTopic = "model-category-updated-topic";
        // const string modelTariffUpdatedTopic = "model-tariff-updated-topic";
        // const string vehicleAddedTopic = "vehicle-added-topic";
        // const string vehicleDeletedTopic = "vehicle-deleted-topic";
        // const string vehicleOccupiedTopic = "vehicle-occupied-topic";
        // const string vehicleReadiedForReleasesTopic = "vehicle-readied-for-releases-topic";
        // const string vehicleReleasedTopic = "vehicle-released-topic";
        // const string vehicleServicedTopic = "vehicle-serviced-topic";
        //
        // services.Configure<KafkaTopicsConfiguration>(config =>
        // {
        //     config.ModelCreatedTopic = modelCreatedTopic;
        //     config.ModelCategoryUpdatedTopic = modelCategoryUpdatedTopic;
        //     config.ModelTariffUpdatedTopic = modelTariffUpdatedTopic;
        //     config.VehicleAddedTopic = vehicleAddedTopic;
        //     config.VehicleDeletedTopic = vehicleDeletedTopic;
        //     config.VehicleOccupiedTopic = vehicleOccupiedTopic;
        //     config.VehicleReadiedForReleaseTopic = vehicleReadiedForReleasesTopic;
        //     config.VehicleReleasedTopic = vehicleReleasedTopic;
        //     config.VehicleServicedTopic = vehicleServicedTopic;
        // });
        //
        // services.AddTransient<IMessageBus, KafkaProducer>();
        //
        // services.AddMassTransit(x =>
        // {
        //     x.UsingInMemory();
        //
        //     x.AddRider(rider =>
        //     {
        //         rider.AddProducer<string, ModelCreated>(modelCreatedTopic);
        //         rider.AddProducer<string, ModelCategoryUpdated>(modelCategoryUpdatedTopic);
        //         rider.AddProducer<string, ModelTariffUpdated>(modelTariffUpdatedTopic);
        //         
        //         rider.AddProducer<string, VehicleAdded>(vehicleAddedTopic);
        //         rider.AddProducer<string, VehicleDeleted>(vehicleDeletedTopic);
        //         rider.AddProducer<string, VehicleOccupied>(vehicleOccupiedTopic);
        //         rider.AddProducer<string, VehicleReadiedForRelease>(vehicleReadiedForReleasesTopic);
        //         rider.AddProducer<string, VehicleReleased>(vehicleReleasedTopic);
        //         rider.AddProducer<string, VehicleServiced>(vehicleServicedTopic);
        //         
        //         rider.UsingKafka((_, k) =>
        //             k.Host((Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS") ?? "localhost:9092").Split("__")));
        //     });
        // });

        return services;
    }

    public static IServiceCollection RegisterInboxAndOutboxBackgroundJobs(this IServiceCollection services)
    {
        // services.AddQuartz(configure =>
        // {
        //     var outboxJobKey = new JobKey(nameof(OutboxBackgroundJob));
        //     configure
        //         .AddJob<OutboxBackgroundJob>(j => j.WithIdentity(outboxJobKey))
        //         .AddTrigger(trigger => trigger.ForJob(outboxJobKey)
        //             .WithSimpleSchedule(scheduleBuilder => scheduleBuilder.WithIntervalInSeconds(3).RepeatForever()));
        //
        //     //     var actualityObserverJobKey = new JobKey(nameof(ActualityObserverBackgroundJob));
        //     //     configure
        //     //         .AddJob<ActualityObserverBackgroundJob>(j => j.WithIdentity(actualityObserverJobKey))
        //     //         .AddTrigger(trigger => trigger.ForJob(actualityObserverJobKey)
        //     //             .WithSimpleSchedule(scheduleBuilder => scheduleBuilder.WithIntervalInSeconds(3).RepeatForever()));
        // });
        //
        // services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        return services;
    }

    public static IServiceCollection RegisterTelemetry(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithMetrics(builder =>
            {
                builder.AddPrometheusExporter();

                builder.AddMeter("Microsoft.AspNetCore.Hosting",
                    "Microsoft.AspNetCore.Server.Kestrel");
                builder.AddView("http.server.request.duration",
                    new ExplicitBucketHistogramConfiguration
                    {
                        Boundaries =
                        [
                            0, 0.005, 0.01, 0.025, 0.05,
                            0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10
                        ]
                    });
            })
            .WithTracing(builder =>
            {
                builder
                    .AddGrpcCoreInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddNpgsql()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService("VehicleDocuments"))
                    .AddSource("VehicleDocuments")
                    .AddSource("MassTransit")
                    .AddJaegerExporter();
            });

        return services;
    }

    public static IServiceCollection RegisterHealthCheckV1(this IServiceCollection services)
    {
        var getConnectionString = () =>
        {
            var connectionBuilder = new NpgsqlConnectionStringBuilder
            {
                ApplicationName = "Vehicle_documents#" + Environment.MachineName,
                Host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost",
                Port = int.Parse(Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5450"),
                Database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "vehicledocuments_db",
                Username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres",
                Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "password",
                BrowsableConnectionString = false
            };

            return connectionBuilder.ConnectionString;
        };

        services.AddGrpcHealthChecks()
            .AddNpgSql(getConnectionString(), timeout: TimeSpan.FromSeconds(10))
            .AddKafka(cfg =>
                    cfg.BootstrapServers = (Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS")
                                            ?? "localhost:9092").Split("__")[0],
                timeout: TimeSpan.FromSeconds(10));

        return services;
    }
}