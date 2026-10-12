using AuthService.Application.Common;

namespace AuthService.Api.Seguranca;

public sealed class ContextoCorrelacao : IContextoCorrelacao
{
    public string IdentificadorCorrelacao { get; set; } = Guid.NewGuid().ToString();
}
