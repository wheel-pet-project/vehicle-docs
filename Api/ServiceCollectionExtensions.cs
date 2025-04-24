using System.Reflection;
using Amazon.Runtime;
using Amazon.S3;
using Api.Adapters.Grpc.EnumMappers;
using Api.Adapters.Kafka;
using Application.DomainEventHandlers;
using Application.Ports.ImageValidators;
using Application.Ports.Kafka;
using Application.Ports.Postgres;
using Application.Ports.S3;
using Application.UseCases.Commands.AddOsago;
using Application.UseCases.Commands.AddPtsToVehicleDocuments;
using Application.UseCases.Commands.AddStsToVehicleDocuments;
using Application.UseCases.Commands.AddVehicleDocuments;
using Application.UseCases.Queries.GetOsagoByVehicleDocumentsId;
using Application.UseCases.Queries.GetPtsByVehicleDocumentsId;
using Application.UseCases.Queries.GetStsByVehicleDocumentsId;
using Application.UseCases.Queries.GetVehicleDocumentsByVehicleId;
using Confluent.Kafka;
using Domain.OsagoAggregate.DomainEvents;
using Domain.Services;
using Domain.VehicleDocumentsAggregate.DomainEvents;
using FluentResults;
using From.VehicleDocumentsKafkaEvents;
using From.VehicleFleetKafkaEvents.Vehicle;
using Infrastructure.Adapters.ImageValidators;
using Infrastructure.Adapters.Kafka;
using Infrastructure.Adapters.Postgres;
using Infrastructure.Adapters.Postgres.Inbox;
using Infrastructure.Adapters.Postgres.OsagoActualityObserver;
using Infrastructure.Adapters.Postgres.Outbox;
using Infrastructure.Adapters.Postgres.Repositories;
using Infrastructure.Adapters.S3;
using Infrastructure.Options;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Quartz;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Api;

public static class ServiceCollectionExtensions
{
    private static readonly Configuration Configuration;

    static ServiceCollectionExtensions()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        Configuration = environment switch
        {
            "Development" => new Configuration
            {
                ApplicationName = "Vehicle_documents#" + Environment.MachineName,
                PostgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost",
                PostgresPort = int.Parse(Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5450"),
                PostgresDatabase = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "vehicledocuments_db",
                PostgresUsername = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres",
                PostgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "password",
                AwsAccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? "aws_access_key_id",
                AwsSecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
                                     ?? "aws_secret_access_key",
                AwsServiceUrl = Environment.GetEnvironmentVariable("AWS_S3_SERVICE_URL") ?? "aws_service_url",
                AwsStsBuckets = (Environment.GetEnvironmentVariable("AWS_STS_BUCKETS") ?? "default_bucket").Split("__"),
                AwsPtsBuckets = (Environment.GetEnvironmentVariable("AWS_PTS_BUCKETS") ?? "default_bucket").Split("__"),
                AwsOsagoBuckets = (Environment.GetEnvironmentVariable("AWS_OSAGO_BUCKETS") ??
                                   "default_bucket").Split("__"),
                BootstrapServers = (Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS") ??
                                    "localhost:9092").Split("__"),
                OsagoExpiredTopic = Environment.GetEnvironmentVariable("OSAGO_EXPIRED_TOPIC") ?? "osago_expired_topic",
                DocumentAddingCompletedTopic = Environment.GetEnvironmentVariable("DOCUMENTS_ADDING_COMPLETED_TOPIC") ??
                                               "documents_adding_completed_topic",
                VehicleAddedTopic = Environment.GetEnvironmentVariable("VEHICLE_ADDED_TOPIC") ?? "vehicle-added-topic",
                MongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") ??
                                        "mongodb://carsharing:password@localhost:27017/drivinglicense?authSource=admin"
            },
            "Production" => new Configuration
            {
                ApplicationName = "Vehicle_documents#" + Environment.MachineName,
                PostgresHost = GetEnvironmentOrThrow("POSTGRES_HOST"),
                PostgresPort = int.Parse(GetEnvironmentOrThrow("POSTGRES_PORT")),
                PostgresDatabase = GetEnvironmentOrThrow("POSTGRES_DB"),
                PostgresUsername = GetEnvironmentOrThrow("POSTGRES_USER"),
                PostgresPassword = GetEnvironmentOrThrow("POSTGRES_PASSWORD"),
                AwsAccessKeyId = GetEnvironmentOrThrow("AWS_ACCESS_KEY_ID"),
                AwsSecretAccessKey = GetEnvironmentOrThrow("AWS_SECRET_ACCESS_KEY"),
                AwsServiceUrl = GetEnvironmentOrThrow("AWS_S3_SERVICE_URL"),
                AwsStsBuckets = GetEnvironmentOrThrow("AWS_STS_BUCKETS").Split("__"),
                AwsPtsBuckets = GetEnvironmentOrThrow("AWS_PTS_BUCKETS").Split("__"),
                AwsOsagoBuckets = GetEnvironmentOrThrow("AWS_OSAGO_BUCKETS").Split("__"),
                BootstrapServers = GetEnvironmentOrThrow("BOOTSTRAP_SERVERS").Split("__"),
                OsagoExpiredTopic = GetEnvironmentOrThrow("OSAGO_EXPIRED_TOPIC"),
                DocumentAddingCompletedTopic = GetEnvironmentOrThrow("DOCUMENTS_ADDING_COMPLETED_TOPIC"),
                VehicleAddedTopic = GetEnvironmentOrThrow("VEHICLE_ADDED_TOPIC"),
                MongoConnectionString = GetEnvironmentOrThrow("MONGO_CONNECTION_STRING")
            },
            _ => throw new ArgumentException("Unknown environment")
        };

        return;

        string GetEnvironmentOrThrow(string environmentName)
        {
            return Environment.GetEnvironmentVariable(environmentName) ??
                   throw new ArgumentNullException(environmentName, "not exist in environment variables");
        }
    }

    public static IServiceCollection RegisterPostgresContextAndDataSource(this IServiceCollection services)
    {
        services.AddScoped<NpgsqlDataSource>(_ =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder
            {
                ConnectionStringBuilder =
                {
                    ApplicationName = Configuration.ApplicationName,
                    Host = Configuration.PostgresHost,
                    Port = Configuration.PostgresPort,
                    Database = Configuration.PostgresDatabase,
                    Username = Configuration.PostgresUsername,
                    Password = Configuration.PostgresPassword,
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

    public static IServiceCollection RegisterDomainServices(this IServiceCollection services)
    {
        services.AddTransient<ICreateVehicleDocumentsService, CreateVehicleDocumentsService>();
        
        return services;
    }

    public static IServiceCollection RegisterMediatorAndHandlers(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Commands
        services.AddTransient<IRequestHandler<AddOsagoCommand, Result>, AddOsagoHandler>();
        services.AddTransient<IRequestHandler<AddPtsToVehicleDocumentsCommand, Result>,
            AddPtsToVehicleDocumentsHandler>();
        services.AddTransient<IRequestHandler<AddStsToVehicleDocumentsCommand, Result>,
            AddStsToVehicleDocumentsHandler>();
        services.AddTransient<IRequestHandler<AddVehicleDocumentsCommand, Result>, AddVehicleDocumentsHandler>();


        // Queries
        var serviceProvider = services.BuildServiceProvider();
        services
            .AddTransient<
                IRequestHandler<GetVehicleDocumentsByVehicleIdQuery,
                    Result<GetVehicleDocumentsByVehicleIdQueryResponse>>, GetVehicleDocumentsByVehicleIdQueryHandler>();
        services
            .AddTransient<IRequestHandler<GetStsByVehicleDocumentsIdQuery,
                Result<GetStsByVehicleDocumentsIdQueryResponse>>>(_ =>
                new GetStsByVehicleDocumentsIdQueryHandler(serviceProvider.GetRequiredService<NpgsqlDataSource>(),
                    Configuration.AwsServiceUrl));
        services.AddTransient<IRequestHandler<GetPtsByVehicleDocumentsIdQuery,
            Result<GetPtsByVehicleDocumentsIdQueryResponse>>>(_ =>
            new GetPtsByVehicleDocumentsIdQueryHandler(serviceProvider.GetRequiredService<NpgsqlDataSource>(),
                Configuration.AwsServiceUrl));
        services.AddTransient<IRequestHandler<GetOsagoByVehicleDocumentsIdQuery,
            Result<GetOsagoByVehicleDocumentsIdQueryResponse>>>(_ =>
            new GetOsagoByVehicleDocumentsIdQueryHandler(serviceProvider.GetRequiredService<NpgsqlDataSource>(),
                Configuration.AwsServiceUrl));

        // Domain event handlers
        services.AddTransient<INotificationHandler<OsagoAddedDomainEvent>, OsagoAddedHandler>();
        services.AddTransient<INotificationHandler<OsagoExpiredDomainEvent>, OsagoExpiredHandler>();
        services
            .AddTransient<INotificationHandler<DocumentAddingCompletedDomainEvent>, DocumentAddingCompletedHandler>();

        return services;
    }

    public static IServiceCollection RegisterSerilog(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(theme: AnsiConsoleTheme.Sixteen)
            .WriteTo.MongoDBBson(Configuration.MongoConnectionString,
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
        services.AddTransient<IOsagoRepository, OsagoRepository>();
        services.AddTransient<IVehicleDocumentsRepository, VehicleDocumentsRepository>();

        return services;
    }

    public static IServiceCollection RegisterUnitOfWork(this IServiceCollection services)
    {
        services.AddTransient<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection RegisterInbox(this IServiceCollection services)
    {
        services.AddTransient<IInbox, Inbox>();

        return services;
    }

    public static IServiceCollection RegisterEnumMappers(this IServiceCollection services)
    {
        services.AddScoped<ColorMapper>();
        services.AddScoped<ExpiryStatusMapper>();

        return services;
    }

    public static IServiceCollection RegisterTimeProvider(this IServiceCollection services)
    {
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        
        return services;
    }

    public static IServiceCollection RegisterMassTransit(this IServiceCollection services)
    {
        services.Configure<KafkaTopicsConfiguration>(config =>
        {
            config.OsagoExpiredTopic = Configuration.OsagoExpiredTopic;
            config.DocumentAddingCompletedTopic = Configuration.DocumentAddingCompletedTopic;
        });

        services.AddTransient<IMessageBus, KafkaProducer>();

        services.AddMassTransit(x =>
        {
            x.UsingInMemory();

            x.AddRider(rider =>
            {
                rider.AddConsumer<VehicleAddedConsumer>();

                rider.AddProducer<string, OsagoExpired>(Configuration.OsagoExpiredTopic);
                rider.AddProducer<string, DocumentAddingCompleted>(Configuration.DocumentAddingCompletedTopic);

                rider.UsingKafka((context, k) =>
                {
                    k.TopicEndpoint<VehicleAdded>(Configuration.VehicleAddedTopic,
                        "vehicledocuments-consumer-group",
                        e =>
                        {
                            e.EnableAutoOffsetStore = false;
                            e.EnablePartitionEof = true;
                            e.AutoOffsetReset = AutoOffsetReset.Earliest;
                            e.CreateIfMissing();
                            e.UseKillSwitch(cfg =>
                                cfg.SetActivationThreshold(1)
                                    .SetRestartTimeout(TimeSpan.FromMinutes(1))
                                    .SetTripThreshold(0.05)
                                    .SetTrackingPeriod(TimeSpan.FromMinutes(1)));
                            e.UseMessageRetry(retry => retry.Interval(200, TimeSpan.FromSeconds(1)));
                            e.ConfigureConsumer<VehicleAddedConsumer>(context);
                        });

                    k.Host(Configuration.BootstrapServers);
                });
            });
        });

        return services;
    }

    public static IServiceCollection RegisterInboxAndOutboxBackgroundJobs(this IServiceCollection services)
    {
        services.AddQuartz(configure =>
        {
            var outboxJobKey = new JobKey(nameof(OutboxBackgroundJob));
            configure
                .AddJob<OutboxBackgroundJob>(j => j.WithIdentity(outboxJobKey))
                .AddTrigger(trigger => trigger.ForJob(outboxJobKey)
                    .WithSimpleSchedule(scheduleBuilder => scheduleBuilder.WithIntervalInSeconds(3).RepeatForever()));

            var inboxJobKey = new JobKey(nameof(InboxBackgroundJob));
            configure
                .AddJob<InboxBackgroundJob>(j => j.WithIdentity(inboxJobKey))
                .AddTrigger(trigger => trigger.ForJob(inboxJobKey)
                    .WithSimpleSchedule(scheduleBuilder => scheduleBuilder.WithIntervalInSeconds(3).RepeatForever()));

            var osagoActualityObserverJobKey = new JobKey(nameof(OsagoActualityObserverBackgroundJob));
            configure
                .AddJob<OsagoActualityObserverBackgroundJob>(j => j.WithIdentity(osagoActualityObserverJobKey))
                .AddTrigger(trigger => trigger.ForJob(osagoActualityObserverJobKey)
                    .WithSimpleSchedule(scheduleBuilder => scheduleBuilder.WithIntervalInMinutes(20).RepeatForever()));
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

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
                ApplicationName = Configuration.ApplicationName,
                Host = Configuration.PostgresHost,
                Port = Configuration.PostgresPort,
                Database = Configuration.PostgresDatabase,
                Username = Configuration.PostgresUsername,
                Password = Configuration.PostgresPassword,
                BrowsableConnectionString = false
            };

            return connectionBuilder.ConnectionString;
        };

        services.AddGrpcHealthChecks()
            .AddNpgSql(getConnectionString(), timeout: TimeSpan.FromSeconds(10))
            .AddKafka(cfg =>
                    cfg.BootstrapServers = Configuration.BootstrapServers[0],
                timeout: TimeSpan.FromSeconds(10));

        return services;
    }

    public static IServiceCollection RegisterS3Storage(this IServiceCollection services)
    {
        services.AddTransient<IAmazonS3>(_ =>
            new AmazonS3Client(
                Configuration.AwsAccessKeyId,
                Configuration.AwsSecretAccessKey,
                new AmazonS3Config
                {
                    ForcePathStyle = true,
                    ServiceURL = Configuration.AwsServiceUrl,
                    AuthenticationRegion = "ru-central1",
                    RetryMode = RequestRetryMode.Standard
                }));
        services.AddTransient<IS3Storage, S3Storage>();

        services.Configure<S3Options>(options =>
        {
            options.StsBuckets = Configuration.AwsStsBuckets;
            options.PtsBuckets = Configuration.AwsPtsBuckets;
            options.OsagoBuckets = Configuration.AwsOsagoBuckets;
        });

        return services;
    }

    public static IServiceCollection RegisterImageValidators(this IServiceCollection services)
    {
        services.AddTransient<IImageFormatValidator, ImageFormatValidator>();
        services.AddTransient<IImageSizeValidator, ImageSizeValidator>();

        return services;
    }
}

internal class Configuration
{
    public required string ApplicationName { get; init; }


    // Postgres
    public required string PostgresHost { get; init; }
    public required int PostgresPort { get; init; }
    public required string PostgresDatabase { get; init; }
    public required string PostgresUsername { get; init; }
    public required string PostgresPassword { get; init; }


    // YandexS3
    public required string AwsAccessKeyId { get; init; }
    public required string AwsSecretAccessKey { get; init; }
    public required string AwsServiceUrl { get; init; }
    public required string[] AwsStsBuckets { get; init; }
    public required string[] AwsPtsBuckets { get; init; }
    public required string[] AwsOsagoBuckets { get; init; }


    // Kafka
    public required string[] BootstrapServers { get; init; }
    public required string OsagoExpiredTopic { get; init; }
    public required string DocumentAddingCompletedTopic { get; init; }
    public required string VehicleAddedTopic { get; init; }


    // Mongo
    public required string MongoConnectionString { get; init; }
}