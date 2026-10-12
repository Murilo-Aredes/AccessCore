using System.Text.Json;
using AuthService.Application.Common;
using AuthService.Domain.Auditoria;
using AuthService.Domain.Seguranca;
using AuthService.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Usuarios.CadastrarUsuario;

public sealed class CadastrarUsuarioCommandHandler(
    IAuthDbContext dbContext,
    IHashSenhaUsuario hashSenhaUsuario,
    ITokenServico tokenServico,
    IProvedorDataHora provedorDataHora)
    : IManipuladorRequisicao<CadastrarUsuarioCommand, CadastrarUsuarioResponse>
{
    public async Task<CadastrarUsuarioResponse> Handle(CadastrarUsuarioCommand requisicao, CancellationToken cancellationToken)
    {
        var agora = provedorDataHora.UtcNow;
        var email = requisicao.Email.Trim();
        var emailNormalizado = email.ToUpperInvariant();

        var emailJaCadastrado = await dbContext.Usuarios
            .AnyAsync(x => x.EmailNormalizado == emailNormalizado, cancellationToken);

        if (emailJaCadastrado)
        {
            throw new ConflitoException("Já existe um usuário cadastrado com este e-mail.");
        }

        var usuario = new Usuario(requisicao.Nome.Trim(), email, emailNormalizado, "senha-pendente", agora);
        usuario.AlterarSenha(hashSenhaUsuario.GerarHash(usuario, requisicao.Senha), agora);
        if (requisicao.RequerRedefinicaoSenha)
        {
            usuario.MarcarRedefinicaoSenhaObrigatoria(agora);
        }

        dbContext.Usuarios.Add(usuario);

        if (requisicao.EnviarConfirmacaoEmail)
        {
            var tokenConfirmacao = tokenServico.GerarTokenSeguro();
            dbContext.TokensConfirmacaoEmail.Add(new TokenConfirmacaoEmail(
                usuario.Id,
                tokenServico.GerarHashToken(tokenConfirmacao),
                agora,
                agora.AddHours(24)));

            dbContext.MensagensOutbox.Add(new MensagemOutbox(
                "ConfirmacaoEmailSolicitada",
                JsonSerializer.Serialize(new
                {
                    usuario.Id,
                    usuario.Email,
                    usuario.Nome,
                    TokenConfirmacao = tokenConfirmacao
                }),
                agora));
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CadastrarUsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.EmailConfirmado, usuario.Ativo, usuario.RequerRedefinicaoSenha);
    }
}
