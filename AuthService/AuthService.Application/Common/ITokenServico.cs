using AuthService.Domain.Aplicacoes;
using AuthService.Domain.Sessoes;
using AuthService.Domain.Usuarios;

namespace AuthService.Application.Common;

public interface ITokenServico
{
    TokenJwt GerarTokenAcesso(Usuario usuario, SessaoUsuario sessao, Aplicacao aplicacao);
    TokenJwt GerarTokenSessaoPendente(Usuario usuario, SessaoUsuario sessao, Aplicacao aplicacao);
    string GerarTokenSeguro();
    string GerarHashToken(string token);
}

public sealed record TokenJwt(string Valor, DateTimeOffset DataExpiracao);
