using FarmaciaSistema.Data;
using FarmaciaSistema.Helpers;
using FarmaciaSistema.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSistema.Controllers;

// RF03: autenticação do farmacêutico (assinatura digital simplificada).
// O token JWT retornado é exigido para acessar as rotas sensíveis do SNGPC (RNF03).
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtHelper _jwtHelper;

    public AuthController(AppDbContext context, JwtHelper jwtHelper)
    {
        _context = context;
        _jwtHelper = jwtHelper;
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(LoginDto dto)
    {
        var farmaceutico = await _context.Farmaceuticos
            .FirstOrDefaultAsync(f => f.Nome == dto.Nome);

        if (farmaceutico is null || farmaceutico.SenhaHash != CryptoHelper.HashSenha(dto.Senha))
            return Unauthorized("Usuário ou senha inválidos.");

        var (token, expiraEm) = _jwtHelper.GerarToken(farmaceutico);

        return new TokenResponse
        {
            Token = token,
            Nome = farmaceutico.Nome,
            ExpiraEm = expiraEm
        };
    }
}
