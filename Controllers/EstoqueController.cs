using FarmaciaSistema.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSistema.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstoqueController : ControllerBase
{
    private readonly AppDbContext _context;

    public EstoqueController(AppDbContext context)
    {
        _context = context;
    }

    public class LoteEstoqueResponse
    {
        public int LoteEstoqueId { get; set; }
        public string Produto { get; set; } = string.Empty;
        public string TipoProduto { get; set; } = string.Empty;
        public string NumeroLote { get; set; } = string.Empty;
        public DateTime DataValidade { get; set; }
        public int Quantidade { get; set; }
    }

    // Saldo atual em estoque, por lote
    [HttpGet]
    public async Task<ActionResult<List<LoteEstoqueResponse>>> GetSaldo()
    {
        var lotes = await _context.LotesEstoque
            .Include(l => l.Produto)
            .Select(l => new LoteEstoqueResponse
            {
                LoteEstoqueId = l.Id,
                Produto = l.Produto!.Nome,
                TipoProduto = l.Produto.TipoProduto.ToString(),
                NumeroLote = l.NumeroLote,
                DataValidade = l.DataValidade,
                Quantidade = l.Quantidade
            })
            .ToListAsync();

        return lotes;
    }

    // RF05: notifica (lista) medicamentos com vencimento nos próximos `dias` (padrão 30)
    [HttpGet("vencimentos")]
    public async Task<ActionResult<List<LoteEstoqueResponse>>> GetProximosVencimentos([FromQuery] int dias = 30)
    {
        var limite = DateTime.Today.AddDays(dias);

        var lotes = await _context.LotesEstoque
            .Include(l => l.Produto)
            .Where(l => l.DataValidade <= limite && l.Quantidade > 0)
            .OrderBy(l => l.DataValidade)
            .Select(l => new LoteEstoqueResponse
            {
                LoteEstoqueId = l.Id,
                Produto = l.Produto!.Nome,
                TipoProduto = l.Produto.TipoProduto.ToString(),
                NumeroLote = l.NumeroLote,
                DataValidade = l.DataValidade,
                Quantidade = l.Quantidade
            })
            .ToListAsync();

        return lotes;
    }
}
