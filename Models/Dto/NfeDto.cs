using System.Xml.Serialization;

namespace FarmaciaSistema.Models.Dto;

// RF01: estrutura simplificada de XML de Nota Fiscal Eletrônica para fins didáticos.
// Em um cenário real seria o layout oficial da NF-e (SEFAZ), aqui está reduzida
// aos campos relevantes para o controle de estoque/SNGPC.
[XmlRoot("NFe")]
public class NfeDto
{
    [XmlElement("Numero")]
    public string Numero { get; set; } = string.Empty;

    [XmlArray("Itens")]
    [XmlArrayItem("Item")]
    public List<NfeItemDto> Itens { get; set; } = new();
}

public class NfeItemDto
{
    [XmlElement("CodigoBarras")]
    public string CodigoBarras { get; set; } = string.Empty;

    [XmlElement("Nome")]
    public string Nome { get; set; } = string.Empty;

    [XmlElement("PrincipioAtivo")]
    public string? PrincipioAtivo { get; set; }

    // RF02: "Controlado", "Comum" ou "Conveniencia"
    [XmlElement("TipoProduto")]
    public TipoProduto TipoProduto { get; set; }

    // Tipo de receita exigida para a venda do produto (ex: "Nenhuma", "Receita Branca",
    // "Receita Controle Especial"). Itens MIP normalmente vêm como "Nenhuma".
    [XmlElement("TipoReceita")]
    public string TipoReceita { get; set; } = "Nenhuma";

    [XmlElement("Lote")]
    public string Lote { get; set; } = string.Empty;

    [XmlElement("Validade")]
    public DateTime Validade { get; set; }

    [XmlElement("Quantidade")]
    public int Quantidade { get; set; }
}
