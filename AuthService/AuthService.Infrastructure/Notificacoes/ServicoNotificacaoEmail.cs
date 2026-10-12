using System.Globalization;
using System.Net;
using System.Net.Mail;
using AuthService.Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.Infrastructure.Notificacoes;

public sealed class ServicoNotificacaoEmail(
    IOptions<SmtpConfiguracao> smtpConfiguracao,
    IOptions<LinksSistemaConfiguracao> linksSistemaConfiguracao,
    ILogger<ServicoNotificacaoEmail> logger) : IServicoNotificacao
{
    public Task EnviarConfirmacaoEmailAsync(NotificacaoConfirmacaoEmail notificacao, CancellationToken cancellationToken)
    {
        var assunto = "Confirme seu e-mail";
        var corpo = $"""
            Olá, {notificacao.NomeUsuario}.

            Use o token abaixo para confirmar seu e-mail:

            {notificacao.TokenConfirmacao}

            Se você não solicitou esse cadastro, ignore esta mensagem.
            """;

        return EnviarAsync(notificacao.Email, notificacao.NomeUsuario, assunto, corpo, cancellationToken);
    }

    public Task EnviarRecuperacaoSenhaAsync(NotificacaoRecuperacaoSenha notificacao, CancellationToken cancellationToken)
    {
        var assunto = "Recuperação de senha";
        var corpo = $"""
            Olá, {notificacao.NomeUsuario}.

            Use o token abaixo para redefinir sua senha:

            {notificacao.TokenRecuperacao}

            Se você não solicitou a recuperação de senha, ignore esta mensagem.
            """;

        return EnviarAsync(notificacao.Email, notificacao.NomeUsuario, assunto, corpo, cancellationToken);
    }

    public Task EnviarNovoLoginAsync(NotificacaoNovoLogin notificacao, CancellationToken cancellationToken)
    {
        var assunto = "Alerta de segurança: novo acesso detectado";
        var confirmarUrl = CriarUrlAcao(notificacao.SessaoUsuarioId, "confirmar-link", notificacao.TokenConfirmacao, confiarDispositivo: true);
        var rejeitarUrl = CriarUrlAcao(notificacao.SessaoUsuarioId, "rejeitar-link", notificacao.TokenConfirmacao, confiarDispositivo: null);
        string? mapaUrl = null;
        string? openStreetMapUrl = null;
        string coordenadasTexto = "Não informadas";
        if (notificacao.Latitude is decimal latitude && notificacao.Longitude is decimal longitude)
        {
            mapaUrl = CriarMapaUrl(latitude, longitude);
            openStreetMapUrl = CriarOpenStreetMapUrl(latitude, longitude);
            coordenadasTexto = $"{FormatarCoordenada(latitude)}, {FormatarCoordenada(longitude)}";
        }


        var localizacao = CriarLocalizacao(notificacao);
        var dataHoraTexto = notificacao.DataHora.ToString("dd/MM/yyyy HH:mm:ss 'UTC'");
        var mapaHtml = mapaUrl is null
            ? """
                <p style="color:#64748b;font-size:13px;line-height:1.5;margin:8px 0 0;">O mapa não está disponível porque o login não enviou latitude e longitude.</p>
                """
            : $$"""
                <table role="presentation" cellpadding="0" cellspacing="0" style="margin-top:12px;">
                    <tr>
                        <td style="padding:0 8px 8px 0;">
                            <a href="{{WebUtility.HtmlEncode(mapaUrl)}}" style="background:#0f172a;border-radius:6px;color:#ffffff;display:inline-block;font-size:13px;font-weight:700;padding:10px 14px;text-decoration:none;">Abrir no Google Maps</a>
                        </td>
                        <td style="padding:0 0 8px 0;">
                            <a href="{{WebUtility.HtmlEncode(openStreetMapUrl)}}" style="background:#334155;border-radius:6px;color:#ffffff;display:inline-block;font-size:13px;font-weight:700;padding:10px 14px;text-decoration:none;">Abrir no OpenStreetMap</a>
                        </td>
                    </tr>
                </table>
                """;

        var corpo = $$"""
            <!doctype html>
            <html lang="pt-BR">
            <body style="background:#eef2f7;color:#172033;font-family:Arial,Helvetica,sans-serif;margin:0;padding:0;">
                <div style="display:none;font-size:1px;line-height:1px;max-height:0;max-width:0;opacity:0;overflow:hidden;">Detectamos uma tentativa de acesso em um novo dispositivo.</div>
                <div style="margin:0 auto;max-width:680px;padding:32px 16px;">
                    <div style="background:#ffffff;border:1px solid #dce3ed;border-radius:8px;overflow:hidden;">
                        <div style="background:#0f172a;color:#ffffff;padding:24px 28px;">
                            <p style="color:#93c5fd;font-size:12px;font-weight:700;letter-spacing:.08em;margin:0 0 8px;text-transform:uppercase;">Alerta de segurança</p>
                            <h1 style="font-size:24px;line-height:1.25;margin:0;">Novo acesso em análise</h1>
                        </div>

                        <div style="padding:28px;">
                            <p style="font-size:16px;line-height:1.55;margin:0 0 18px;">Olá, {{WebUtility.HtmlEncode(notificacao.NomeUsuario)}}.</p>
                            <p style="font-size:16px;line-height:1.55;margin:0 0 22px;">Identificamos uma tentativa de acesso à sua conta em um dispositivo que ainda não foi reconhecido. Revise os detalhes abaixo e escolha uma ação.</p>

                            <table role="presentation" cellpadding="0" cellspacing="0" style="background:#f8fafc;border:1px solid #e2e8f0;border-radius:8px;margin:0 0 22px;width:100%;">
                                <tr>
                                    <td style="padding:18px 20px;">
                                        <p style="color:#64748b;font-size:12px;font-weight:700;letter-spacing:.06em;margin:0 0 12px;text-transform:uppercase;">Detalhes do acesso</p>
                                        <table role="presentation" cellpadding="0" cellspacing="0" style="width:100%;">
                                            <tr>
                                                <td style="color:#64748b;font-size:13px;padding:7px 16px 7px 0;width:150px;">Dispositivo</td>
                                                <td style="color:#172033;font-size:14px;font-weight:700;padding:7px 0;">{{HtmlValor(notificacao.Dispositivo)}}</td>
                                            </tr>
                                            <tr>
                                                <td style="color:#64748b;font-size:13px;padding:7px 16px 7px 0;">Navegador</td>
                                                <td style="color:#172033;font-size:14px;font-weight:700;padding:7px 0;">{{HtmlValor(notificacao.Navegador)}}</td>
                                            </tr>
                                            <tr>
                                                <td style="color:#64748b;font-size:13px;padding:7px 16px 7px 0;">Sistema</td>
                                                <td style="color:#172033;font-size:14px;font-weight:700;padding:7px 0;">{{HtmlValor(notificacao.SistemaOperacional)}}</td>
                                            </tr>
                                            <tr>
                                                <td style="color:#64748b;font-size:13px;padding:7px 16px 7px 0;">IP</td>
                                                <td style="color:#172033;font-size:14px;font-weight:700;padding:7px 0;">{{HtmlValor(notificacao.EnderecoIpMascarado)}}</td>
                                            </tr>
                                            <tr>
                                                <td style="color:#64748b;font-size:13px;padding:7px 16px 7px 0;">Data e hora</td>
                                                <td style="color:#172033;font-size:14px;font-weight:700;padding:7px 0;">{{WebUtility.HtmlEncode(dataHoraTexto)}}</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>

                            <table role="presentation" cellpadding="0" cellspacing="0" style="background:#ffffff;border:1px solid #e2e8f0;border-radius:8px;margin:0 0 24px;width:100%;">
                                <tr>
                                    <td style="padding:18px 20px;">
                                        <p style="color:#64748b;font-size:12px;font-weight:700;letter-spacing:.06em;margin:0 0 12px;text-transform:uppercase;">Localização aproximada</p>
                                        <p style="color:#172033;font-size:16px;font-weight:700;margin:0 0 6px;">{{WebUtility.HtmlEncode(localizacao)}}</p>
                                        <p style="color:#64748b;font-size:13px;line-height:1.5;margin:0;">Coordenadas: {{WebUtility.HtmlEncode(coordenadasTexto)}}</p>
                                        {{mapaHtml}}
                                    </td>
                                </tr>
                            </table>

                            <table role="presentation" cellpadding="0" cellspacing="0" style="background:#fff7ed;border:1px solid #fed7aa;border-radius:8px;margin:0 0 24px;width:100%;">
                                <tr>
                                    <td style="padding:18px 20px;">
                                        <p style="color:#9a3412;font-size:15px;font-weight:700;margin:0 0 6px;">Foi você?</p>
                                        <p style="color:#7c2d12;font-size:14px;line-height:1.5;margin:0;">Confirme o acesso se reconhecer este dispositivo. Caso contrário, rejeite imediatamente para bloquear a sessão.</p>
                                    </td>
                                </tr>
                            </table>

                            <table role="presentation" cellpadding="0" cellspacing="0" style="margin:0 0 20px;">
                                <tr>
                                    <td style="padding:0 10px 12px 0;">
                                        <a href="{{WebUtility.HtmlEncode(confirmarUrl)}}" style="background:#15803d;border-radius:6px;color:#ffffff;display:inline-block;font-size:15px;font-weight:700;padding:13px 18px;text-decoration:none;">Sim, fui eu</a>
                                    </td>
                                    <td style="padding:0 0 12px 0;">
                                        <a href="{{WebUtility.HtmlEncode(rejeitarUrl)}}" style="background:#b91c1c;border-radius:6px;color:#ffffff;display:inline-block;font-size:15px;font-weight:700;padding:13px 18px;text-decoration:none;">Não fui eu</a>
                                    </td>
                                </tr>
                            </table>

                            <p style="border-top:1px solid #e2e8f0;color:#64748b;font-size:13px;line-height:1.5;margin:8px 0 0;padding-top:18px;">Se você não iniciou esse acesso, recomendamos alterar sua senha após rejeitar a sessão.</p>
                        </div>
                    </div>
                </div>
            </body>
            </html>
            """;

        return EnviarAsync(notificacao.Email, notificacao.NomeUsuario, assunto, corpo, cancellationToken, isBodyHtml: true);
    }

    private async Task EnviarAsync(string email, string nome, string assunto, string corpo, CancellationToken cancellationToken, bool isBodyHtml = false)
    {
        var configuracao = smtpConfiguracao.Value;
        ValidarConfiguracao(configuracao);

        using var mensagem = new MailMessage
        {
            From = new MailAddress(configuracao.FromEmail, configuracao.FromName),
            Subject = assunto,
            Body = corpo,
            IsBodyHtml = isBodyHtml
        };

        mensagem.To.Add(new MailAddress(email, nome));

        using var smtpClient = new SmtpClient(configuracao.Host, configuracao.Port)
        {
            EnableSsl = configuracao.EnableSsl,
            Credentials = new NetworkCredential(configuracao.Username, configuracao.Password)
        };

        await smtpClient.SendMailAsync(mensagem, cancellationToken);

        logger.LogInformation("E-mail {Assunto} enviado para {Email}.", assunto, email);
    }

    private static void ValidarConfiguracao(SmtpConfiguracao configuracao)
    {
        if (string.IsNullOrWhiteSpace(configuracao.Host)
            || string.IsNullOrWhiteSpace(configuracao.Username)
            || string.IsNullOrWhiteSpace(configuracao.Password)
            || string.IsNullOrWhiteSpace(configuracao.FromEmail))
        {
            throw new InvalidOperationException("Configuração SMTP incompleta.");
        }
    }

    private string CriarUrlAcao(Guid sessaoUsuarioId, string acao, string token, bool? confiarDispositivo)
    {
        var urlBase = linksSistemaConfiguracao.Value.UrlBaseApi.TrimEnd('/');
        var url = $"{urlBase}/api/sessoes/{sessaoUsuarioId:D}/{acao}?token={Uri.EscapeDataString(token)}";
        return confiarDispositivo.HasValue ? $"{url}&confiarDispositivo={confiarDispositivo.Value.ToString().ToLowerInvariant()}" : url;
    }

    private static string CriarMapaUrl(decimal latitude, decimal longitude)
    {
        return $"https://www.google.com/maps?q={FormatarCoordenada(latitude)},{FormatarCoordenada(longitude)}";
    }

    private static string CriarOpenStreetMapUrl(decimal latitude, decimal longitude)
    {
        var latitudeTexto = FormatarCoordenada(latitude);
        var longitudeTexto = FormatarCoordenada(longitude);
        return $"https://www.openstreetmap.org/?mlat={latitudeTexto}&mlon={longitudeTexto}#map=12/{latitudeTexto}/{longitudeTexto}";
    }

    private static string CriarLocalizacao(NotificacaoNovoLogin notificacao)
    {
        var partes = new[] { notificacao.Cidade, notificacao.Estado, notificacao.Pais }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .ToArray();

        return partes.Length > 0 ? string.Join(", ", partes) : "Não informada";
    }

    private static string FormatarCoordenada(decimal valor)
    {
        return valor.ToString(CultureInfo.InvariantCulture);
    }

    private static string HtmlValor(string? valor)
    {
        return WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(valor) ? "Não informado" : valor);
    }
}
