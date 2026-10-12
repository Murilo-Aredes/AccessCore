using AuthService.Application.Common;
using AuthService.Infrastructure.Auditoria;
using AuthService.Infrastructure.Notificacoes;
using AuthService.Infrastructure.Outbox;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Seguranca;
using AuthService.Infrastructure.Tempo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtConfiguracao>(configuration.GetSection(JwtConfiguracao.Secao));
        services.Configure<TokenAtualizacaoConfiguracao>(configuration.GetSection(TokenAtualizacaoConfiguracao.Secao));
        services.Configure<SegurancaLoginConfiguracao>(configuration.GetSection(SegurancaLoginConfiguracao.Secao));
        services.Configure<SessaoConfiguracao>(configuration.GetSection(SessaoConfiguracao.Secao));
        services.Configure<OutboxConfiguracao>(configuration.GetSection(OutboxConfiguracao.Secao));
        services.Configure<SmtpConfiguracao>(configuration.GetSection(SmtpConfiguracao.Secao));
        services.Configure<LinksSistemaConfiguracao>(configuration.GetSection(LinksSistemaConfiguracao.Secao));

        services.AddScoped<AuditoriaSaveChangesInterceptor>();
        services.AddDbContext<AuthDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("AuthService")
                ?? "Server=(localdb)\\mssqllocaldb;Database=AuthService;Trusted_Connection=True;TrustServerCertificate=True";

            options.UseSqlServer(connectionString, sqlServerOptions =>
            {
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditoriaSaveChangesInterceptor>());
        });

        services.AddScoped<IAuthDbContext>(serviceProvider => serviceProvider.GetRequiredService<AuthDbContext>());
        services.AddSingleton<IProvedorDataHora, ProvedorDataHora>();
        services.AddSingleton<IPropriedadesSensiveisProvider, PropriedadesSensiveisProvider>();
        services.AddScoped<IHashSenhaUsuario, HashSenhaUsuario>();
        services.AddScoped<ITokenServico, TokenServico>();
        services.AddScoped<IServicoNotificacao, ServicoNotificacaoEmail>();
        var outboxConfiguracao = configuration.GetSection(OutboxConfiguracao.Secao).Get<OutboxConfiguracao>() ?? new OutboxConfiguracao();
        if (outboxConfiguracao.Habilitado)
        {
            services.AddHostedService<OutboxBackgroundService>();
        }

        return services;
    }
}
