namespace FollowUpCrm.Api.Configuration;

public static class CorsConfiguration
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(o => o.AddPolicy("AllowFrontend", p =>
            p.WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!)
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials()));

        return services;
    }

    public static WebApplication UseCorsConfiguration(this WebApplication app)
    {
        app.UseCors("AllowFrontend");
        return app;
    }
}