using AuthService.Application.Common;

namespace AuthService.Infrastructure.Tempo;

public sealed class ProvedorDataHora : IProvedorDataHora
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
