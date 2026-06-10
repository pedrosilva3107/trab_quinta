namespace FarmaciaSistema.Models;

// RF07: registro do PDV que abate o estoque automaticamente após a confirmação da venda.
public class Venda
{
    public int Id { get; set; }

    public DateTime DataVenda { get; set; } = DateTime.Now;

    public List<ItemVenda> Itens { get; set; } = new();
}

public class ItemVenda
{
    public int Id { get; set; }

    public int VendaId { get; set; }
    public Venda? Venda { get; set; }

    public int LoteEstoqueId { get; set; }
    public LoteEstoque? LoteEstoque { get; set; }

    public int Quantidade { get; set; }
}
