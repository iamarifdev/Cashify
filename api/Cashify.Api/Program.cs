using System.Reflection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for production environment
if (builder.Environment.IsProduction())
{
    builder.Host.UseSerilog(
        (context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration)
    );
}

// Configure core services
builder.Services
    .AddCashifyDatabase(builder.Configuration)
    .AddCashifyServices(Assembly.GetExecutingAssembly())
    .AddCashifyOpenApi()
    .AddCashifyAuthentication(builder.Configuration)
    .AddCashifyAuthorization();

var app = builder.Build();

// Configure middleware and endpoints
app
    .UseCashifyMiddleware()
    .UseCashifyEndpoints();

await app.RunAsync();