namespace FarmaciaSistema.Models;

// RF03: usado para validar a "assinatura digital" (senha) que libera entrada/saída
// de medicamentos controlados.
public class Farmaceutico
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string SenhaHash { get; set; } = string.Empty;
}
