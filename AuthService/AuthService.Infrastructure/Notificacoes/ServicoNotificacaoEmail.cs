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
    IRenderizadorTemplateEmail renderizadorTemplateEmail,
    ILogger<ServicoNotificacaoEmail> logger) : IServicoNotificacao
{
    public async Task EnviarConfirmacaoEmailAsync(NotificacaoConfirmacaoEmail notificacao, CancellationToken cancellationToken)
    {
        var assunto = "Confirme seu e-mail";
        var corpo = await renderizadorTemplateEmail.RenderizarAsync(
            "confirmacao-email",
            new Dictionary<string, object?>
            {
                ["NomeUsuario"] = notificacao.NomeUsuario,
                ["TokenConfirmacao"] = notificacao.TokenConfirmacao
            },
            cancellationToken);

        await EnviarAsync(notificacao.Email, notificacao.NomeUsuario, assunto, corpo, cancellationToken, isBodyHtml: true);
    }

    public async Task EnviarRecuperacaoSenhaAsync(NotificacaoRecuperacaoSenha notificacao, CancellationToken cancellationToken)
    {
        var assunto = "Recuperação de senha";
        var corpo = await renderizadorTemplateEmail.RenderizarAsync(
            "recuperacao-senha",
            new Dictionary<string, object?>
            {
                ["NomeUsuario"] = notificacao.NomeUsuario,
                ["TokenRecuperacao"] = notificacao.TokenRecuperacao
            },
            cancellationToken);

        await EnviarAsync(notificacao.Email, notificacao.NomeUsuario, assunto, corpo, cancellationToken, isBodyHtml: true);
    }

    public async Task EnviarNovoLoginAsync(NotificacaoNovoLogin notificacao, CancellationToken cancellationToken)
    {
        var assunto = "Alerta de segurança: novo acesso detectado";
        var confirmarUrl = CriarUrlAcao(notificacao.SessaoUsuarioId, "confirmar-link", notificacao.TokenConfirmacao, confiarDispositivo: true);
        var rejeitarUrl = CriarUrlAcao(notificacao.SessaoUsuarioId, "rejeitar-link", notificacao.TokenConfirmacao, confiarDispositivo: null);
        var latitude = notificacao.Latitude;
        var longitude = notificacao.Longitude;
        var temMapa = false;
        var coordenadasTexto = "Não informadas";
        var googleMapsUrl = string.Empty;
        var openStreetMapUrl = string.Empty;
        var dataHoraTexto = notificacao.DataHora.ToString("dd/MM/yyyy HH:mm:ss 'UTC'");

        if (latitude.HasValue && longitude.HasValue)
        {
            var latitudeValor = latitude.Value;
            var longitudeValor = longitude.Value;
            temMapa = true;
            coordenadasTexto = $"{FormatarCoordenada(latitudeValor)}, {FormatarCoordenada(longitudeValor)}";
            googleMapsUrl = CriarMapaUrl(latitudeValor, longitudeValor);
            openStreetMapUrl = CriarOpenStreetMapUrl(latitudeValor, longitudeValor);
        }

        var corpo = await renderizadorTemplateEmail.RenderizarAsync(
            "novo-login",
            new Dictionary<string, object?>
            {
                ["NomeUsuario"] = notificacao.NomeUsuario,
                ["Dispositivo"] = ValorOuPadrao(notificacao.Dispositivo),
                ["Navegador"] = ValorOuPadrao(notificacao.Navegador),
                ["SistemaOperacional"] = ValorOuPadrao(notificacao.SistemaOperacional),
                ["EnderecoIpMascarado"] = ValorOuPadrao(notificacao.EnderecoIpMascarado),
                ["DataHora"] = dataHoraTexto,
                ["Localizacao"] = CriarLocalizacao(notificacao),
                ["Coordenadas"] = coordenadasTexto,
                ["TemMapa"] = temMapa,
                ["GoogleMapsUrl"] = googleMapsUrl,
                ["OpenStreetMapUrl"] = openStreetMapUrl,
                ["ConfirmarUrl"] = confirmarUrl,
                ["RejeitarUrl"] = rejeitarUrl
            },
            cancellationToken);

        await EnviarAsync(notificacao.Email, notificacao.NomeUsuario, assunto, corpo, cancellationToken, isBodyHtml: true);
    }

    private async Task EnviarAsync(string email, string nome, string assunto, string corpo, CancellationToken cancellationToken, bool isBodyHtml)
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

    private static string ValorOuPadrao(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? "Não informado" : valor;
    }
}
