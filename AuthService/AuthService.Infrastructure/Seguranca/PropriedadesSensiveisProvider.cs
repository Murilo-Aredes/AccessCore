using AuthService.Application.Common;

namespace AuthService.Infrastructure.Seguranca;

public sealed class PropriedadesSensiveisProvider : IPropriedadesSensiveisProvider
{
    private static readonly HashSet<string> PropriedadesSensiveis = new(StringComparer.OrdinalIgnoreCase)
    {
        "Senha",
        "SenhaHash",
        "TokenAcesso",
        "TokenAtualizacao",
        "TokenHash",
        "ChavesSecretas",
        "ApiKey",
        "ConnectionString",
        "ChaveSecreta"
    };

    public bool EhSensivel(string nomePropriedade) => PropriedadesSensiveis.Contains(nomePropriedade);
}
