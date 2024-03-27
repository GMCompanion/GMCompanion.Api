using GMCompanion.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace GMCompanion.Api.Infrastucture;

public static class DatabaseExtensions
{
    public static IServiceCollection AddPostgresTaskContext(this IServiceCollection services, IConfiguration configuration)
    {
        PostgresConfiguration postgresConfiguration = new PostgresConfiguration();
        configuration.Bind("PostgresConfiguration", postgresConfiguration);

        postgresConfiguration.Validate();
        services.AddDbContext<MarketContext>(options => { options.UseNpgsql(postgresConfiguration.BuildConnectionString()); });

        services.AddTransient<EFRepository<Character>>();
        services.AddTransient<EFRepository<Item>>();

        return services;
    }
}