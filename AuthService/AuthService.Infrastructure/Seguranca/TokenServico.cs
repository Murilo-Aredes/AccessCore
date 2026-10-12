using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Application.Common;
using AuthService.Domain.Aplicacoes;
using AuthService.Domain.Sessoes;
using AuthService.Domain.Usuarios;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Seguranca;

public sealed class TokenServico(IOptions<JwtConfiguracao> jwtConfiguracao, IProvedorDataHora provedorDataHora) : ITokenServico
{
    public TokenJwt GerarTokenAcesso(Usuario usuario, SessaoUsuario sessao, Aplicacao aplicacao)
    {
        var dataExpiracao = provedorDataHora.UtcNow.AddMinutes(jwtConfiguracao.Value.MinutosExpiracaoTokenAcesso);
        return GerarJwt(usuario, sessao, aplicacao, dataExpiracao, null);
    }

    public TokenJwt GerarTokenSessaoPendente(Usuario usuario, SessaoUsuario sessao, Aplicacao aplicacao)
    {
        var dataExpiracao = provedorDataHora.UtcNow.AddMinutes(jwtConfiguracao.Value.MinutosExpiracaoSessaoPendente);
        return GerarJwt(usuario, sessao, aplicacao, dataExpiracao, "sessao_pendente");
    }

    public string GerarTokenSeguro()
    {
        Span<byte> bytes = stackalloc byte[64];
        RandomNumberGenerator.Fill(bytes);
        return WebEncoders.Base64UrlEncode(bytes);
    }

    public string GerarHashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    private TokenJwt GerarJwt(Usuario usuario, SessaoUsuario sessao, Aplicacao aplicacao, DateTimeOffset dataExpiracao, string? tipoToken)
    {
        var configuracao = jwtConfiguracao.Value;
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuracao.ChaveSecreta));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new("sid", sessao.Id.ToString()),
            new("application_id", aplicacao.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(tipoToken))
        {
            claims.Add(new Claim("token_type", tipoToken));
        }

        var token = new JwtSecurityToken(
            issuer: configuracao.Emissao,
            audience: configuracao.Audiencia,
            claims: claims,
            notBefore: provedorDataHora.UtcNow.UtcDateTime,
            expires: dataExpiracao.UtcDateTime,
            signingCredentials: credenciais);

        return new TokenJwt(new JwtSecurityTokenHandler().WriteToken(token), dataExpiracao);
    }
}
