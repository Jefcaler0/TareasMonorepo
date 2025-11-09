using DotNetEnv;
using Serilog;
using System.Text.Json.Serialization;
using Tareas.API.Endpoints;
using Tareas.Application;
using Tareas.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    LoadEnvFileIfPresent(builder.Environment.ContentRootPath);

    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    });

    builder.Logging.ClearProviders();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    var allowedOrigins = ResolveAllowedOrigins(builder.Configuration);
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontendPolicy", policy =>
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.UseCors("FrontendPolicy");

    app.MapTaskEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static void LoadEnvFileIfPresent(string contentRootPath)
{
    var envFile = Path.Combine(contentRootPath, ".env");
    if (File.Exists(envFile))
    {
        Env.TraversePath().Load();
    }
}

static string[] ResolveAllowedOrigins(IConfiguration configuration)
{
    var originsValue = configuration["FRONTEND_URL"] ?? configuration["Frontend:AllowedOrigins"] ?? "http://localhost:5173";
    var origins = originsValue
        .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    return origins.Length > 0 ? origins : new[] { "http://localhost:5173" };
}
