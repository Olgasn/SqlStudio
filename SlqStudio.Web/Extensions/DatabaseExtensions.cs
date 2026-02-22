using Application.Common.SQL;
using Microsoft.EntityFrameworkCore;
using SlqStudio.Persistence;

namespace SlqStudio.Extensions;

public static class DatabaseExtensions
{
    public static void AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MySQL")
            ?? throw new InvalidOperationException("Connection string 'MySQL' is not configured.");
        var labsConnectionString = configuration.GetConnectionString("LabsConnection")
            ?? throw new InvalidOperationException("Connection string 'LabsConnection' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddSingleton<SqlManager>(sp => new SqlManager(labsConnectionString));
    }
}
