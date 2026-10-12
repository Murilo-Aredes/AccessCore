using AuthService.Domain.Common;

namespace AuthService.Domain.Auditoria;

public sealed class MensagemOutbox : Entidade
{
    private MensagemOutbox()
    {
    }

    public MensagemOutbox(string tipo, string conteudoJson, DateTimeOffset dataCriacao)
    {
        Tipo = tipo;
        ConteudoJson = conteudoJson;
        DataCriacao = dataCriacao;
    }

    public string Tipo { get; private set; } = string.Empty;
    public string ConteudoJson { get; private set; } = string.Empty;
    public DateTimeOffset DataCriacao { get; private set; }
    public DateTimeOffset? DataProcessamento { get; private set; }
    public string? ErroProcessamento { get; private set; }
    public int QuantidadeTentativas { get; private set; }

    public void MarcarComoProcessada(DateTimeOffset dataProcessamento)
    {
        DataProcessamento = dataProcessamento;
        ErroProcessamento = null;
    }

    public void RegistrarFalha(string erroProcessamento)
    {
        QuantidadeTentativas++;
        ErroProcessamento = erroProcessamento;
    }

    public void MarcarComoNaoProcessavel(DateTimeOffset dataProcessamento, string erroProcessamento)
    {
        DataProcessamento = dataProcessamento;
        QuantidadeTentativas++;
        ErroProcessamento = erroProcessamento;
    }
}
