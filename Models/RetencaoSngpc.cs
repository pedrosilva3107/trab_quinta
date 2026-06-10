namespace FarmaciaSistema.Models;

// Etapa "Conferir lote, validade e reter via da nota para o SNGPC" (raia Farmacêutico)
// e base para o arquivo de transmissão do RF06.
public class RetencaoSngpc
{
    public int Id { get; set; }

    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public string NumeroLote { get; set; } = string.Empty;

    public DateTime DataValidade { get; set; }

    public int Quantidade { get; set; }

    public string TipoMovimento { get; set; } = string.Empty; // "Entrada" ou "Saida"

    public DateTime DataMovimento { get; set; } = DateTime.Now;

    public int FarmaceuticoId { get; set; }
    public Farmaceutico? Farmaceutico { get; set; }

    // RNF03: dado sensível (paciente/receita) armazenado de forma criptografada
    public string ReceitaPacienteCriptografado { get; set; } = string.Empty;

    public bool TransmitidoSngpc { get; set; }
}
