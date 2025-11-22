using System.Reflection;
using System.Text;
using Cashify.Api.Database;
using Cashify.Api.Extensions;
using FluentValidation;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddMemoryCache();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddScoped<Cashify.Api.Infrastructure.JwtTokenService>();
builder.Services.AddEndpointsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddHandlersFromAssembly(Assembly.GetExecutingAssembly());

var jwtSection = builder.Configuration.GetSection("Jwt");
var signingKey = jwtSection.GetValue<string>("SigningKey") ?? "replace-me";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtSection.GetValue<string>("Issuer"),
            ValidAudience = jwtSection.GetValue<string>("Audience"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BusinessMember", policy => policy.RequireAssertion(_ => true));
    options.AddPolicy("CashbookMember", policy => policy.RequireAssertion(_ => true));
});

builder.Services.AddSingleton(new GoogleJsonWebSignature.ValidationSettings());

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseMiddleware<Cashify.Api.Middleware.ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet("/", () => Results.Redirect("/scalar"));

await app.RunAsync();
