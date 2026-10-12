using System.Text.Json;
using AuthService.Application.Autenticacao.Senha;
using AuthService.Application.Common;
using AuthService.Domain.Auditoria;
using AuthService.Domain.Seguranca;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Autenticacao.Email;

public sealed class ReenviarConfirmacaoEmailCommandHandler(IAuthDbContext dbContext, ITokenServico tokenServico, IProvedorDataHora provedorDataHora)
    : IManipuladorRequisicao<ReenviarConfirmacaoEmailCommand, MensagemGenericaResponse>
{
    public async Task<MensagemGenericaResponse> Handle(ReenviarConfirmacaoEmailCommand requisicao, CancellationToken cancellationToken)
    {
        var agora = provedorDataHora.UtcNow;
        var emailNormalizado = requisicao.Email.Trim().ToUpperInvariant();
        var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(x => x.EmailNormalizado == emailNormalizado, cancellationToken);

        if (usuario is not null && !usuario.EmailConfirmado)
        {
            var token = tokenServico.GerarTokenSeguro();
            dbContext.TokensConfirmacaoEmail.Add(new TokenConfirmacaoEmail(usuario.Id, tokenServico.GerarHashToken(token), agora, agora.AddHours(24)));
            dbContext.MensagensOutbox.Add(new MensagemOutbox(
                "ConfirmacaoEmailSolicitada",
                JsonSerializer.Serialize(new { usuario.Email, usuario.Nome, TokenConfirmacao = token }),
                agora));

            dbContext.EventosSeguranca.Add(new EventoSeguranca(
                usuario.Id,
                null,
                null,
                TipoEventoSeguranca.LoginFalhou,
                NivelEventoSeguranca.Informacao,
                null,
                null,
                "Reenvio de confirmação de e-mail solicitado.",
                null,
                agora));

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return new MensagemGenericaResponse("Se o usuário informado existir, as instruções serão enviadas.");
    }
}
