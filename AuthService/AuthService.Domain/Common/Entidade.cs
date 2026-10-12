namespace AuthService.Domain.Common;

public abstract class Entidade
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
