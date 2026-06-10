namespace FarmaciaSistema.Models.Dto;

// RF07: payload enviado pelo PDV ao confirmar uma venda.
public class VendaDto
{
    public List<ItemVendaDto> Itens { get; set; } = new();

    // RF03: necessário apenas se algum item vendido for um medicamento controlado
    public string? FarmaceuticoNome { get; set; }
    public string? FarmaceuticoSenha { get; set; }

    // RNF03: dados do paciente/receita, exigidos quando há item controlado
    public string? NomePaciente { get; set; }
    public string? NumeroReceita { get; set; }
}

public class ItemVendaDto
{
    public int LoteEstoqueId { get; set; }
    public int Quantidade { get; set; }
}
