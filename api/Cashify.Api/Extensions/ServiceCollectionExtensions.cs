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

namespace Cashify.Api.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCashifyDatabase(IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("Default")));
        
            return services;
        }

        public IServiceCollection AddCashifyServices(Assembly assembly)
        {
            services.AddMemoryCache();
            services.AddValidatorsFromAssemblyContaining<Program>();
            services.AddHandlersFromAssembly(assembly);
            services.AddEndpointsFromAssembly(assembly);
            services.AddCors();
            services.AddHttpContextAccessor();
            services.AddScoped<JwtTokenService>();
            services.AddScoped<ActivityLogService>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddSingleton(new GoogleJsonWebSignature.ValidationSettings());
        
            return services;
        }

        public IServiceCollection AddCashifyAuthentication(IConfiguration configuration)
        {
            const string multiJwtSchemeName = "MultiJwt";
            const string googleIdTokenScheme = "GoogleIdToken";
        
            var jwtSection = configuration.GetSection("Jwt");
            var signingKey = jwtSection.GetValue<string>("SigningKey") ?? "replace-me";
        
            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = multiJwtSchemeName;
                    options.DefaultChallengeScheme = multiJwtSchemeName;
                })
                .AddPolicyScheme(multiJwtSchemeName, "MultiJwt", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                        if (string.IsNullOrWhiteSpace(authHeader) ||
                            !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            return JwtBearerDefaults.AuthenticationScheme;
                    
                        var token = authHeader["Bearer ".Length..].Trim();
                        var handler = new JwtSecurityTokenHandler();
                        try
                        {
                            var jwt = handler.ReadJwtToken(token);
                            var issuer = jwt.Issuer ?? string.Empty;
                            if (issuer.Contains("accounts.google.com", StringComparison.OrdinalIgnoreCase))
                            {
                                return googleIdTokenScheme;
                            }
                        }
                        catch
                        {
                            // fall through
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
                .AddScheme<AuthenticationSchemeOptions, GoogleIdTokenHandler>(googleIdTokenScheme, _ => { });

            return services;
        }

        public IServiceCollection AddCashifyAuthorization()
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("BusinessMember", policy => policy.RequireClaim("biz_member", "true"));
                options.AddPolicy("CashbookMember", policy => policy.RequireClaim("cb_member", "true"));
            });
        
            return services;
        }

        public IServiceCollection AddCashifyOpenApi()
        {
            services.AddEndpointsApiExplorer();
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info.Title = "Cashify API";
                    document.Info.Version = "v1";
                    document.Info.Description =
                        "Income/Expense multi-tenant API with Google Auth, businesses, cashbooks, transactions, lookups, activity logs, and reports.";
                    return Task.CompletedTask;
                });
            });
        
            return services;
        }

        public IServiceCollection AddHandlersFromAssembly(Assembly assembly)
        {
            var handlerTypes = assembly
                .DefinedTypes
                .Where(type => type is { IsClass: true, IsAbstract: false } && type.Name.EndsWith("Handler", StringComparison.Ordinal));

            foreach (var type in handlerTypes)
            {
                services.AddTransient(type);
            }

            return services;
        }
    }
}
