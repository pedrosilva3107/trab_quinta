namespace FarmaciaSistema.Models;

// RF04: rastreio de lote/validade. Núcleo do controle de estoque (saldo em tempo real).
public class LoteEstoque
{
    public int Id { get; set; }

    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public string NumeroLote { get; set; } = string.Empty;

    public DateTime DataValidade { get; set; }

    public int Quantidade { get; set; }
}
