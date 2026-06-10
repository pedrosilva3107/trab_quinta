using FarmaciaSistema.Data;
using FarmaciaSistema.Helpers;
using FarmaciaSistema.Models;
using FarmaciaSistema.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSistema.Controllers;

// PDV: confirma a venda e abate o estoque (RF07).
[ApiController]
[Route("api/[controller]")]
public class VendasController : ControllerBase
{
    private readonly AppDbContext _context;

    public VendasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<Venda>> Confirmar(VendaDto dto)
    {
        if (dto.Itens.Count == 0)
            return BadRequest("A venda precisa ter ao menos um item.");

        var lotes = new List<LoteEstoque>();
        var possuiControlados = false;

        foreach (var itemDto in dto.Itens)
        {
            var lote = await _context.LotesEstoque
                .Include(l => l.Produto)
                .FirstOrDefaultAsync(l => l.Id == itemDto.LoteEstoqueId);

            if (lote is null)
                return NotFound($"Lote de estoque {itemDto.LoteEstoqueId} não encontrado.");

            if (lote.Quantidade < itemDto.Quantidade)
                return BadRequest($"Saldo insuficiente para {lote.Produto!.Nome} (lote {lote.NumeroLote}).");

            if (lote.Produto!.TipoProduto == TipoProduto.Controlado)
                possuiControlados = true;

            lotes.Add(lote);
        }

        // RF03: saída de medicamentos controlados exige assinatura/senha do farmacêutico
        Farmaceutico? farmaceutico = null;
        if (possuiControlados)
        {
            if (string.IsNullOrWhiteSpace(dto.FarmaceuticoNome) || string.IsNullOrWhiteSpace(dto.FarmaceuticoSenha))
                return BadRequest("Venda possui medicamentos controlados: informe usuário e senha do farmacêutico.");

            farmaceutico = await _context.Farmaceuticos
                .FirstOrDefaultAsync(f => f.Nome == dto.FarmaceuticoNome);

            if (farmaceutico is null || farmaceutico.SenhaHash != CryptoHelper.HashSenha(dto.FarmaceuticoSenha))
                return Unauthorized("Usuário ou senha do farmacêutico inválidos.");
        }

        var venda = new Venda();
        _context.Vendas.Add(venda);

        for (var i = 0; i < dto.Itens.Count; i++)
        {
            var itemDto = dto.Itens[i];
            var lote = lotes[i];

            // RF07: abate automático do saldo de estoque
            lote.Quantidade -= itemDto.Quantidade;

            venda.Itens.Add(new ItemVenda
            {
                LoteEstoqueId = lote.Id,
                Quantidade = itemDto.Quantidade
            });

            if (lote.Produto!.TipoProduto == TipoProduto.Controlado && farmaceutico is not null)
            {
                var receitaPaciente = $"Paciente: {dto.NomePaciente}; Receita: {dto.NumeroReceita}";

                _context.RetencoesSngpc.Add(new RetencaoSngpc
                {
                    ProdutoId = lote.ProdutoId,
                    NumeroLote = lote.NumeroLote,
                    DataValidade = lote.DataValidade,
                    Quantidade = itemDto.Quantidade,
                    TipoMovimento = "Saida",
                    FarmaceuticoId = farmaceutico.Id,
                    // RNF03: dado sensível armazenado criptografado (LGPD)
                    ReceitaPacienteCriptografado = CryptoHelper.Criptografar(receitaPaciente)
                });
            }
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Confirmar), new { id = venda.Id }, venda);
    }
}
