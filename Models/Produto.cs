using System.ComponentModel.DataAnnotations;

namespace FarmaciaSistema.Models;

// Catálogo de produtos (nome, código de barras, princípio ativo, tipo de receita exigida)
public class Produto
{
    public int Id { get; set; }

    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public string CodigoBarras { get; set; } = string.Empty;

    public string? PrincipioAtivo { get; set; }

    // RF02: Controlado, Comum (MIP - Medicamento Isento de Prescrição) ou Conveniência
    public TipoProduto TipoProduto { get; set; }

    // Tipo de receita exigida para a venda (ex: "Nenhuma", "Receita Branca", "Receita Controle Especial")
    // Itens MIP (Comum) normalmente não exigem receita ("Nenhuma").
    public string TipoReceita { get; set; } = "Nenhuma";

    public List<LoteEstoque> Lotes { get; set; } = new();
}
