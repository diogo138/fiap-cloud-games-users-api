using FCG.Users.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FCG.Users.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static IApplicationBuilder MigrateDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsuariosDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<UsuariosDbContext>>();

        try
        {
            logger.LogInformation("Aplicando migrations do banco de dados...");
            context.Database.Migrate();
            logger.LogInformation("Migrations aplicadas com sucesso.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao aplicar migrations do banco de dados.");
            throw;
        }

        return app;
    }
}
