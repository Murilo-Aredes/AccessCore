using System.Text.Json;
using AuthService.Application.Common;
using AuthService.Domain.Auditoria;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using AuditoriaEntidade = AuthService.Domain.Auditoria.Auditoria;

namespace AuthService.Infrastructure.Auditoria;

public sealed class AuditoriaSaveChangesInterceptor(
    IUsuarioAtual usuarioAtual,
    IContextoCorrelacao contextoCorrelacao,
    IProvedorDataHora provedorDataHora,
    IPropriedadesSensiveisProvider propriedadesSensiveisProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AdicionarAuditorias(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AdicionarAuditorias(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AdicionarAuditorias(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var auditorias = dbContext.ChangeTracker.Entries()
            .Where(DeveAuditar)
            .Select(CriarAuditoria)
            .Where(x => x is not null)
            .Cast<AuditoriaEntidade>()
            .ToList();

        if (auditorias.Count > 0)
        {
            dbContext.Set<AuditoriaEntidade>().AddRange(auditorias);
        }
    }

    private static bool DeveAuditar(EntityEntry entry)
    {
        return entry.Entity is not AuditoriaEntidade
            && entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
            && entry.Metadata.FindPrimaryKey() is not null;
    }

    private AuditoriaEntidade? CriarAuditoria(EntityEntry entry)
    {
        var acao = entry.State switch
        {
            EntityState.Added => TipoAcaoAuditoria.Inclusao,
            EntityState.Modified => TipoAcaoAuditoria.Alteracao,
            EntityState.Deleted => TipoAcaoAuditoria.Exclusao,
            _ => throw new InvalidOperationException("Estado não auditável.")
        };

        var camposAlterados = entry.State == EntityState.Modified
            ? CriarCamposAlterados(entry)
            : [];

        if (entry.State == EntityState.Modified && camposAlterados.Count == 0)
        {
            return null;
        }

        var identificadorEntidade = ObterIdentificadorEntidade(entry);
        var dadosAntes = entry.State is EntityState.Modified or EntityState.Deleted ? CriarSnapshot(entry, usarOriginal: true) : null;
        var dadosDepois = entry.State is EntityState.Added or EntityState.Modified ? CriarSnapshot(entry, usarOriginal: false) : null;

        return new AuditoriaEntidade(
            usuarioAtual.UsuarioId,
            usuarioAtual.SessaoUsuarioId,
            usuarioAtual.AplicacaoId,
            entry.Metadata.ClrType.Name,
            identificadorEntidade,
            acao,
            dadosAntes is null ? null : JsonSerializer.Serialize(dadosAntes),
            dadosDepois is null ? null : JsonSerializer.Serialize(dadosDepois),
            camposAlterados.Count == 0 ? null : JsonSerializer.Serialize(camposAlterados),
            usuarioAtual.EnderecoIp,
            usuarioAtual.IdentificadorDispositivo,
            contextoCorrelacao.IdentificadorCorrelacao,
            provedorDataHora.UtcNow);
    }

    private Dictionary<string, object?> CriarSnapshot(EntityEntry entry, bool usarOriginal)
    {
        var snapshot = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            var nome = property.Metadata.Name;
            if (property.Metadata.IsShadowProperty() || propriedadesSensiveisProvider.EhSensivel(nome))
            {
                continue;
            }

            snapshot[nome] = usarOriginal ? property.OriginalValue : property.CurrentValue;
        }

        return snapshot;
    }

    private List<object> CriarCamposAlterados(EntityEntry entry)
    {
        return entry.Properties
            .Where(x => x.IsModified && !x.Metadata.IsShadowProperty() && !propriedadesSensiveisProvider.EhSensivel(x.Metadata.Name))
            .Select(x => new
            {
                campo = x.Metadata.Name,
                valorAnterior = x.OriginalValue,
                valorNovo = x.CurrentValue
            })
            .Cast<object>()
            .ToList();
    }

    private static string ObterIdentificadorEntidade(EntityEntry entry)
    {
        var chave = entry.Metadata.FindPrimaryKey();
        if (chave is null)
        {
            return string.Empty;
        }

        var valores = chave.Properties
            .Select(property => entry.Property(property.Name).CurrentValue?.ToString() ?? string.Empty);

        return string.Join("|", valores);
    }
}
