namespace AuthService.Domain.Auditoria;

public enum TipoEventoSeguranca
{
    LoginRealizado = 1,
    LoginFalhou = 2,
    NovoDispositivoDetectado = 3,
    SessaoConfirmada = 4,
    SessaoRejeitada = 5,
    SessaoRevogada = 6,
    ReutilizacaoTokenDetectada = 7,
    SenhaAlterada = 8,
    RecuperacaoSenhaSolicitada = 9,
    UsuarioBloqueado = 10,
    UsuarioDesbloqueado = 11,
    PermissaoConcedida = 12,
    PermissaoNegada = 13,
    PermissaoRemovida = 14
}
