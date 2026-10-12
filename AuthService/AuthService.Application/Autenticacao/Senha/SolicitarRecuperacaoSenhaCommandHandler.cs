using System.Text.Json;
using AuthService.Application.Common;
using AuthService.Domain.Auditoria;
using AuthService.Domain.Seguranca;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Autenticacao.Senha;

public sealed class SolicitarRecuperacaoSenhaCommandHandler(IAuthDbContext dbContext, ITokenServico tokenServico, IProvedorDataHora provedorDataHora)
    : IManipuladorRequisicao<SolicitarRecuperacaoSenhaCommand, MensagemGenericaResponse>
{
    public async Task<MensagemGenericaResponse> Handle(SolicitarRecuperacaoSenhaCommand requisicao, CancellationToken cancellationToken)
    {
        var agora = provedorDataHora.UtcNow;
        var emailNormalizado = requisicao.Email.Trim().ToUpperInvariant();
        var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(x => x.EmailNormalizado == emailNormalizado, cancellationToken);

        if (usuario is not null)
        {
            var token = tokenServico.GerarTokenSeguro();
            dbContext.TokensRecuperacaoSenha.Add(new TokenRecuperacaoSenha(
                usuario.Id,
                tokenServico.GerarHashToken(token),
                agora,
                agora.AddMinutes(30)));

            dbContext.EventosSeguranca.Add(new EventoSeguranca(
                usuario.Id,
                null,
                null,
                TipoEventoSeguranca.RecuperacaoSenhaSolicitada,
                NivelEventoSeguranca.Informacao,
                null,
                null,
                "Recuperação de senha solicitada.",
                null,
                agora));

            dbContext.MensagensOutbox.Add(new MensagemOutbox(
                "RecuperacaoSenhaSolicitada",
                JsonSerializer.Serialize(new { usuario.Email, usuario.Nome, TokenRecuperacao = token }),
                agora));

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return new MensagemGenericaResponse("Se o usuário informado existir, as instruções serão enviadas.");
    }
}
