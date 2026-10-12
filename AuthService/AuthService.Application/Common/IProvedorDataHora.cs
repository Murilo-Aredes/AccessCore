namespace AuthService.Application.Common;

public interface IProvedorDataHora
{
    DateTimeOffset UtcNow { get; }
}
