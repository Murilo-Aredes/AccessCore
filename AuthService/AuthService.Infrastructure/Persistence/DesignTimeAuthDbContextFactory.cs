using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthService.Infrastructure.Persistence;

public sealed class DesignTimeAuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=AuthService;Trusted_Connection=True;TrustServerCertificate=True",
                sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                })
            .Options;

        return new AuthDbContext(options);
    }
}
