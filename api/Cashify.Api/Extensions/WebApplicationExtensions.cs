using Cashify.Api.Middleware;
using Scalar.AspNetCore;
using Serilog;

namespace Cashify.Api.Extensions;

public static class WebApplicationExtensions
{
    extension(WebApplication app)
    {
        public WebApplication UseCashifyMiddleware()
        {
            if (app.Environment.IsProduction())
            {
                app.UseSerilogRequestLogging();
            }
        
            app.UseCors(cors =>
            {
                cors
                    .WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseMiddleware<AuthorizationMembershipMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }

        public WebApplication UseCashifyEndpoints()
        {
            var apiGroup = app.MapGroup("/api");
            apiGroup.MapEndpoints(app.Services);
            app.MapOpenApi();
            app.MapScalarApiReference();
            app.MapGet("/", () => Results.Redirect("/scalar"));

            return app;
        }
    }
}