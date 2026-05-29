using FluentValidation;
using Serilog;
using FollowUpCrm.Api.Authentication;
using FollowUpCrm.Api.Configuration;
using FollowUpCrm.Api.Middleware;
using FollowUpCrm.Api.Modules.Auth;
using FollowUpCrm.Api.Modules.Customers;
using FollowUpCrm.Api.Modules.Dashboard;
using FollowUpCrm.Api.Modules.FollowUps;
using FollowUpCrm.Api.Modules.Identity;
using FollowUpCrm.Api.Modules.Permissions;
using FollowUpCrm.Api.Modules.Workspaces;
using FollowUpCrm.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting FollowUp CRM API host");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    #region Services

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerConfiguration();
    builder.Services.AddCorsConfiguration(builder.Configuration);
    builder.Services.AddMediatRConfiguration();
    builder.Services.AddApiVersioningConfiguration();
    builder.Services.AddJwtAuthenticationConfiguration(builder.Configuration);
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddSingleton<GlobalExceptionHandlingMiddleware>();

    builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
    builder.Services.AddAuthModule();
    builder.Services.AddIdentityModule();
    builder.Services.AddWorkspacesModule();
    builder.Services.AddPermissionsModule();
    builder.Services.AddCustomersModule();
    builder.Services.AddFollowUpsModule();
    builder.Services.AddDashboardModule();

    #endregion

    var app = builder.Build();

    #region Pipeline

    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    app.UseCorsConfiguration();
    app.UseAuthentication();
    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
        app.UseSwaggerConfiguration();

    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/database", new HealthCheckOptions
    {
        Predicate = healthCheck => healthCheck.Tags.Contains("database")
    });

    await app.SeedDefaultAdminAsync();

    app.MapAuthModule();
    app.MapIdentityModule();
    app.MapWorkspacesModule();
    app.MapPermissionsModule();
    app.MapCustomersModule();
    app.MapFollowUpsModule();
    app.MapDashboardModule();

    app.MapControllers();

    #endregion

    app.Run();
}
catch (HostAbortedException)
{
    // Expected when EF Core tools build the host for design-time services.
}
catch (Exception ex)
{
    Log.Fatal(ex, "FollowUp CRM API host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
