using AuthService.Application.Common;

namespace AuthService.Application.Usuarios.CadastrarUsuario;

public sealed record CadastrarUsuarioCommand(
    string Nome,
    string Email,
    string Senha,
    bool EnviarConfirmacaoEmail = true,
    bool RequerRedefinicaoSenha = false) : IRequisicao<CadastrarUsuarioResponse>;
