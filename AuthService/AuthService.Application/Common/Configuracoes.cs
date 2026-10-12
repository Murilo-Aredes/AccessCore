namespace AuthService.Application.Common;

public sealed class JwtConfiguracao
{
    public const string Secao = "Jwt";
    public string Emissao { get; init; } = "AuthService";
    public string Audiencia { get; init; } = "AuthService.Clientes";
    public string ChaveSecreta { get; init; } = "alterar-em-producao-chave-com-pelo-menos-32-caracteres";
    public int MinutosExpiracaoTokenAcesso { get; init; } = 15;
    public int MinutosExpiracaoSessaoPendente { get; init; } = 5;
}

public sealed class TokenAtualizacaoConfiguracao
{
    public const string Secao = "TokenAtualizacao";
    public int DiasExpiracao { get; init; } = 7;
}

public sealed class SegurancaLoginConfiguracao
{
    public const string Secao = "SegurancaLogin";
    public int QuantidadeMaximaTentativas { get; init; } = 5;
    public int MinutosBloqueio { get; init; } = 15;
}

public sealed class SessaoConfiguracao
{
    public const string Secao = "Sessao";
    public int DiasExpiracao { get; init; } = 7;
}

public sealed class OutboxConfiguracao
{
    public const string Secao = "Outbox";
    public bool Habilitado { get; init; } = true;
    public int IntervaloSegundos { get; init; } = 30;
}

public sealed class SmtpConfiguracao
{
    public const string Secao = "Smtp";
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public bool EnableSsl { get; init; } = true;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = "AuthService";
}

public sealed class LinksSistemaConfiguracao
{
    public const string Secao = "LinksSistema";
    public string UrlBaseApi { get; init; } = "https://localhost:7295";
}
