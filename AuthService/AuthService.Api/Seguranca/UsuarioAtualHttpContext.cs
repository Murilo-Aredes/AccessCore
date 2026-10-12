using System.Security.Claims;
using AuthService.Application.Common;

namespace AuthService.Api.Seguranca;

public sealed class UsuarioAtualHttpContext(IHttpContextAccessor httpContextAccessor, IContextoCorrelacao contextoCorrelacao) : IUsuarioAtual
{
    public Guid? UsuarioId => ObterGuid(ClaimTypes.NameIdentifier) ?? ObterGuid("sub");
    public Guid? SessaoUsuarioId => ObterGuid("sid");
    public Guid? AplicacaoId => ObterGuid("application_id");
    public string? EnderecoIp => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    public string? IdentificadorDispositivo => httpContextAccessor.HttpContext?.Request.Headers["X-Device-ID"].FirstOrDefault();
    public string IdentificadorCorrelacao => contextoCorrelacao.IdentificadorCorrelacao;

    private Guid? ObterGuid(string claimType)
    {
        var valor = httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);
        return Guid.TryParse(valor, out var guid) ? guid : null;
    }
}
