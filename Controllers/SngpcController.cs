using System.Text;
using FarmaciaSistema.Data;
using FarmaciaSistema.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSistema.Controllers;

// RF06: gera o arquivo de transmissão de movimentação de psicotrópicos para o SNGPC/Anvisa.
[ApiController]
[Route("api/[controller]")]
public class SngpcController : ControllerBase
{
    private readonly AppDbContext _context;

    public SngpcController(AppDbContext context)
    {
        _context = context;
    }

    public class RetencaoResponse
    {
        public int Id { get; set; }
        public string Produto { get; set; } = string.Empty;
        public string NumeroLote { get; set; } = string.Empty;
        public DateTime DataValidade { get; set; }
        public int Quantidade { get; set; }
        public string TipoMovimento { get; set; } = string.Empty;
        public DateTime DataMovimento { get; set; }
        public string Farmaceutico { get; set; } = string.Empty;
        public string ReceitaPaciente { get; set; } = string.Empty;
        public bool TransmitidoSngpc { get; set; }
    }

    // Lista as retenções de medicamentos controlados ainda não transmitidas
    [HttpGet("retencoes")]
    public async Task<ActionResult<List<RetencaoResponse>>> GetRetencoes([FromQuery] bool somentePendentes = true)
    {
        var query = _context.RetencoesSngpc
            .Include(r => r.Produto)
            .Include(r => r.Farmaceutico)
            .AsQueryable();

        if (somentePendentes)
            query = query.Where(r => !r.TransmitidoSngpc);

        var retencoes = await query
            .OrderBy(r => r.DataMovimento)
            .Select(r => new RetencaoResponse
            {
                Id = r.Id,
                Produto = r.Produto!.Nome,
                NumeroLote = r.NumeroLote,
                DataValidade = r.DataValidade,
                Quantidade = r.Quantidade,
                TipoMovimento = r.TipoMovimento,
                DataMovimento = r.DataMovimento,
                Farmaceutico = r.Farmaceutico!.Nome,
                ReceitaPaciente = CryptoHelper.Descriptografar(r.ReceitaPacienteCriptografado),
                TransmitidoSngpc = r.TransmitidoSngpc
            })
            .ToListAsync();

        return retencoes;
    }

    // RF06: gera o arquivo (texto, layout simplificado) com as movimentações pendentes
    // e marca como transmitido.
    [HttpPost("transmissao")]
    public async Task<IActionResult> GerarArquivoTransmissao()
    {
        var pendentes = await _context.RetencoesSngpc
            .Include(r => r.Produto)
            .Include(r => r.Farmaceutico)
            .Where(r => !r.TransmitidoSngpc)
            .OrderBy(r => r.DataMovimento)
            .ToListAsync();

        if (pendentes.Count == 0)
            return NoContent();

        var sb = new StringBuilder();
        sb.AppendLine("ARQUIVO SNGPC - MOVIMENTACAO DE PSICOTROPICOS");
        sb.AppendLine($"Gerado em: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("Produto;Lote;Validade;Quantidade;Movimento;Data;Farmaceutico");

        foreach (var r in pendentes)
        {
            sb.AppendLine(string.Join(';',
                r.Produto!.Nome,
                r.NumeroLote,
                r.DataValidade.ToString("yyyy-MM-dd"),
                r.Quantidade,
                r.TipoMovimento,
                r.DataMovimento.ToString("yyyy-MM-dd HH:mm:ss"),
                r.Farmaceutico!.Nome));

            r.TransmitidoSngpc = true;
        }

        await _context.SaveChangesAsync();

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var nomeArquivo = $"sngpc_{DateTime.Now:yyyyMMddHHmmss}.txt";
        return File(bytes, "text/plain", nomeArquivo);
    }
}
