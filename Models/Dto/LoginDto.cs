namespace FarmaciaSistema.Models.Dto;

// RF03: login do farmacêutico (usuário/senha = "assinatura digital" simplificada)
public class LoginDto
{
    public string Nome { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class TokenResponse
{
    public string Token { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
}
