namespace AuthService.Application.Autorizacao.ObterAcessosUsuario;

public sealed record ObterAcessosUsuarioResponse(
    UsuarioAcessoResponse Usuario,
    AplicacaoAcessoResponse Aplicacao,
    IReadOnlyCollection<string> Perfis,
    IReadOnlyCollection<string> Permissoes,
    IReadOnlyCollection<EscopoAcessoResponse> Escopos);

public sealed record UsuarioAcessoResponse(Guid Id, string Nome);

public sealed record AplicacaoAcessoResponse(Guid Id, string Codigo, string Nome);

public sealed record EscopoAcessoResponse(string Tipo, string Identificador);
