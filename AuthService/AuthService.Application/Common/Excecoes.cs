namespace AuthService.Application.Common;

public abstract class AuthServiceException(string mensagem) : Exception(mensagem);

public sealed class NaoAutenticadoException(string mensagem) : AuthServiceException(mensagem);

public sealed class AcessoNegadoException(string mensagem) : AuthServiceException(mensagem);

public sealed class NaoEncontradoException(string mensagem) : AuthServiceException(mensagem);

public sealed class ConflitoException(string mensagem) : AuthServiceException(mensagem);
