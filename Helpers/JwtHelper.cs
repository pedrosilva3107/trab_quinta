using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FarmaciaSistema.Models;
using Microsoft.IdentityModel.Tokens;

namespace FarmaciaSistema.Helpers;

// RF03: gera o token JWT que representa a "assinatura digital" do farmacêutico
// autenticado, usado para autorizar a entrada/saída de medicamentos controlados
// e o acesso aos dados sensíveis do SNGPC (RNF03).
public class JwtHelper
{
    private readonly IConfiguration _config;

    public JwtHelper(IConfiguration config)
    {
        _config = config;
    }

    public (string Token, DateTime ExpiraEm) GerarToken(Farmaceutico farmaceutico)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiraEm = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresMinutes"] ?? "60"));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, farmaceutico.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, farmaceutico.Nome),
            new Claim(ClaimTypes.Role, "Farmaceutico")
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiraEm,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
