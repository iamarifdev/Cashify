using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using Cashify.Api.Database;
using Cashify.Api.Infrastructure;
using FluentValidation;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
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
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Cashify API";
        document.Info.Version = "v1";
        document.Info.Description = "Income/Expense multi-tenant API with Google Auth, businesses, cashbooks, transactions, lookups, activity logs, and reports.";
        return Task.CompletedTask;
    });
});
builder.Services.AddScoped<Cashify.Api.Infrastructure.JwtTokenService>();
builder.Services.AddScoped<Cashify.Api.Infrastructure.ActivityLogService>();
builder.Services.AddEndpointsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddHandlersFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddCors();

var jwtSection = builder.Configuration.GetSection("Jwt");
var signingKey = jwtSection.GetValue<string>("SigningKey") ?? "replace-me";
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = "MultiJwt";
        options.DefaultChallengeScheme = "MultiJwt";
    })
    .AddPolicyScheme("MultiJwt", "MultiJwt", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader["Bearer ".Length..].Trim();
                var handler = new JwtSecurityTokenHandler();
                try
                {
                    var jwt = handler.ReadJwtToken(token);
                    var issuer = jwt.Issuer ?? string.Empty;
                    if (issuer.Contains("accounts.google.com", StringComparison.OrdinalIgnoreCase))
                    {
                        return "GoogleIdToken";
                    }
                }
                catch
                {
                    // fall through
                }
            }

            return JwtBearerDefaults.AuthenticationScheme;
        };
    })
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
    })
    .AddScheme<AuthenticationSchemeOptions, GoogleIdTokenHandler>("GoogleIdToken", _ => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BusinessMember", policy => policy.RequireClaim("biz_member", "true"));
    options.AddPolicy("CashbookMember", policy => policy.RequireClaim("cb_member", "true"));
});

builder.Services.AddSingleton(new GoogleJsonWebSignature.ValidationSettings());

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseCors(cors =>
{
    cors
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
});

app.UseMiddleware<Cashify.Api.Middleware.ExceptionHandlingMiddleware>();
app.UseMiddleware<Cashify.Api.Middleware.AuthorizationMembershipMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

var apiGroup = app.MapGroup("/api");
apiGroup.MapEndpoints(app.Services);
app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet("/", () => Results.Redirect("/scalar"));

await app.RunAsync();
