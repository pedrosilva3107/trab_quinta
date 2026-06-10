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

    // RF02: Controlado, Comum (MIP) ou Conveniência
    public TipoProduto TipoProduto { get; set; }

    public List<LoteEstoque> Lotes { get; set; } = new();
}
