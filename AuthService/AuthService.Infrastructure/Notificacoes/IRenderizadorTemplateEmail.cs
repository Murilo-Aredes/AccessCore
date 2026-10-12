namespace AuthService.Infrastructure.Notificacoes;

public interface IRenderizadorTemplateEmail
{
    Task<string> RenderizarAsync(string nomeTemplate, IReadOnlyDictionary<string, object?> variaveis, CancellationToken cancellationToken);
}
