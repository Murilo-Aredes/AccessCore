using AuthService.Application.Common;

namespace AuthService.Application.Usuarios.AtualizarRedefinicaoSenhaObrigatoria;

public sealed record AtualizarRedefinicaoSenhaObrigatoriaCommand(
    Guid UsuarioId,
    bool RequerRedefinicaoSenha) : IRequisicao<AtualizarRedefinicaoSenhaObrigatoriaResponse>;
