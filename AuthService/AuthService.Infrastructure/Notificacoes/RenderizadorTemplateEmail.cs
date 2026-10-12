using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using AuthService.Application.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AuthService.Infrastructure.Notificacoes;

public sealed partial class RenderizadorTemplateEmail(
    IHostEnvironment hostEnvironment,
    IOptions<EmailTemplatesConfiguracao> configuracao) : IRenderizadorTemplateEmail
{
    public async Task<string> RenderizarAsync(string nomeTemplate, IReadOnlyDictionary<string, object?> variaveis, CancellationToken cancellationToken)
    {
        var caminhoTemplate = ObterCaminhoTemplate(nomeTemplate);
        if (!File.Exists(caminhoTemplate))
        {
            throw new FileNotFoundException($"Template de e-mail não encontrado: {caminhoTemplate}", caminhoTemplate);
        }

        var conteudo = await File.ReadAllTextAsync(caminhoTemplate, cancellationToken);
        conteudo = RenderizarSecoes(conteudo, variaveis);
        return RenderizarVariaveis(conteudo, variaveis);
    }

    private string ObterCaminhoTemplate(string nomeTemplate)
    {
        if (nomeTemplate.Any(Path.GetInvalidFileNameChars().Contains))
        {
            throw new InvalidOperationException($"Nome de template inválido: {nomeTemplate}");
        }

        var diretorio = configuracao.Value.Diretorio;
        var caminhoDiretorio = Path.IsPathRooted(diretorio)
            ? diretorio
            : Path.Combine(hostEnvironment.ContentRootPath, diretorio);

        return Path.Combine(caminhoDiretorio, $"{nomeTemplate}.html");
    }

    private static string RenderizarSecoes(string conteudo, IReadOnlyDictionary<string, object?> variaveis)
    {
        conteudo = SecaoVerdadeiraRegex().Replace(conteudo, match =>
        {
            var nome = match.Groups["nome"].Value;
            return ValorBooleano(variaveis, nome) ? match.Groups["conteudo"].Value : string.Empty;
        });

        return SecaoFalsaRegex().Replace(conteudo, match =>
        {
            var nome = match.Groups["nome"].Value;
            return ValorBooleano(variaveis, nome) ? string.Empty : match.Groups["conteudo"].Value;
        });
    }

    private static string RenderizarVariaveis(string conteudo, IReadOnlyDictionary<string, object?> variaveis)
    {
        return VariavelRegex().Replace(conteudo, match =>
        {
            var nome = match.Groups["nome"].Value;
            if (!variaveis.TryGetValue(nome, out var valor) || valor is null)
            {
                return string.Empty;
            }

            return WebUtility.HtmlEncode(Convert.ToString(valor, CultureInfo.InvariantCulture) ?? string.Empty);
        });
    }

    private static bool ValorBooleano(IReadOnlyDictionary<string, object?> variaveis, string nome)
    {
        if (!variaveis.TryGetValue(nome, out var valor) || valor is null)
        {
            return false;
        }

        return valor switch
        {
            bool booleano => booleano,
            string texto => !string.IsNullOrWhiteSpace(texto),
            _ => true
        };
    }

    [GeneratedRegex(@"{{#(?<nome>[A-Za-z0-9_]+)}}(?<conteudo>.*?){{/\k<nome>}}", RegexOptions.Singleline)]
    private static partial Regex SecaoVerdadeiraRegex();

    [GeneratedRegex(@"{{\^(?<nome>[A-Za-z0-9_]+)}}(?<conteudo>.*?){{/\k<nome>}}", RegexOptions.Singleline)]
    private static partial Regex SecaoFalsaRegex();

    [GeneratedRegex(@"{{(?<nome>[A-Za-z0-9_]+)}}")]
    private static partial Regex VariavelRegex();
}
