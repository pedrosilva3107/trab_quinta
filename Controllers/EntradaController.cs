using System.Xml.Serialization;
using FarmaciaSistema.Data;
using FarmaciaSistema.Helpers;
using FarmaciaSistema.Models;
using FarmaciaSistema.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSistema.Controllers;

// Implementa o fluxo "Recebedor/Estoquista" e "Farmacêutico" do BPMN de entrada de mercadoria:
// 1) RF01 - importa o XML da NF-e
// 2) RF02 - categoriza cada item (Controlado / Comum / Conveniência)
// 3) RF03 - exige assinatura/senha do farmacêutico para itens Controlados
// 4) Farmacêutico confere lote/validade e retém via da nota para o SNGPC
// 5) RF04 - registra lote e validade no estoque
[ApiController]
[Route("api/[controller]")]
public class EntradaController : ControllerBase
{
    private readonly AppDbContext _context;

    public EntradaController(AppDbContext context)
    {
        _context = context;
    }

    public class ImportarXmlRequest
    {
        public IFormFile ArquivoXml { get; set; } = null!;

        // RF03: obrigatórios somente se a nota contiver itens Controlados
        public string? FarmaceuticoNome { get; set; }
        public string? FarmaceuticoSenha { get; set; }

        // RNF03: dados de receituário/paciente referentes aos itens controlados retidos
        public string? NomePaciente { get; set; }
        public string? NumeroReceita { get; set; }
    }

    public class ImportarXmlResponse
    {
        public string NumeroNota { get; set; } = string.Empty;
        public List<string> ItensProcessados { get; set; } = new();
        public bool PossuiControlados { get; set; }
    }

    [HttpPost("importar-xml")]
    public async Task<ActionResult<ImportarXmlResponse>> ImportarXml([FromForm] ImportarXmlRequest request)
    {
        NfeDto nfe;
        try
        {
            var serializer = new XmlSerializer(typeof(NfeDto));
            await using var stream = request.ArquivoXml.OpenReadStream();
            nfe = (NfeDto?)serializer.Deserialize(stream)
                  ?? throw new InvalidOperationException("XML vazio");
        }
        catch (Exception ex)
        {
            return BadRequest($"XML inválido: {ex.Message}");
        }

        var possuiControlados = nfe.Itens.Any(i => i.TipoProduto == TipoProduto.Controlado);

        // RF03: exige a assinatura digital (senha) do farmacêutico para liberar
        // a entrada de medicamentos controlados
        Farmaceutico? farmaceutico = null;
        if (possuiControlados)
        {
            if (string.IsNullOrWhiteSpace(request.FarmaceuticoNome) || string.IsNullOrWhiteSpace(request.FarmaceuticoSenha))
                return BadRequest("Nota possui medicamentos controlados: informe usuário e senha do farmacêutico.");

            farmaceutico = await _context.Farmaceuticos
                .FirstOrDefaultAsync(f => f.Nome == request.FarmaceuticoNome);

            if (farmaceutico is null || farmaceutico.SenhaHash != CryptoHelper.HashSenha(request.FarmaceuticoSenha))
                return Unauthorized("Usuário ou senha do farmacêutico inválidos.");
        }

        var response = new ImportarXmlResponse
        {
            NumeroNota = nfe.Numero,
            PossuiControlados = possuiControlados
        };

        foreach (var item in nfe.Itens)
        {
            // RF02: categoriza o item (Controlado, Comum/MIP ou Conveniência)
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.CodigoBarras == item.CodigoBarras);

            if (produto is null)
            {
                produto = new Produto
                {
                    CodigoBarras = item.CodigoBarras,
                    Nome = item.Nome,
                    PrincipioAtivo = item.PrincipioAtivo,
                    TipoProduto = item.TipoProduto,
                    TipoReceita = item.TipoReceita
                };
                _context.Produtos.Add(produto);
                await _context.SaveChangesAsync();
            }

            // RF04: registra o lote e a data de validade. Se o lote já existir,
            // soma a quantidade recebida ao saldo existente.
            var lote = await _context.LotesEstoque.FirstOrDefaultAsync(l =>
                l.ProdutoId == produto.Id && l.NumeroLote == item.Lote);

            if (lote is null)
            {
                lote = new LoteEstoque
                {
                    ProdutoId = produto.Id,
                    NumeroLote = item.Lote,
                    DataValidade = item.Validade,
                    Quantidade = item.Quantidade
                };
                _context.LotesEstoque.Add(lote);
            }
            else
            {
                lote.Quantidade += item.Quantidade;
            }

            // Raia "Farmacêutico": conferir lote/validade e reter via da nota para o SNGPC
            if (item.TipoProduto == TipoProduto.Controlado && farmaceutico is not null)
            {
                var receitaPaciente = $"Paciente: {request.NomePaciente}; Receita: {request.NumeroReceita}";

                _context.RetencoesSngpc.Add(new RetencaoSngpc
                {
                    ProdutoId = produto.Id,
                    NumeroLote = item.Lote,
                    DataValidade = item.Validade,
                    Quantidade = item.Quantidade,
                    TipoMovimento = "Entrada",
                    FarmaceuticoId = farmaceutico.Id,
                    // RNF03: dado sensível armazenado criptografado (LGPD)
                    ReceitaPacienteCriptografado = CryptoHelper.Criptografar(receitaPaciente)
                });
            }

            response.ItensProcessados.Add(
                $"{produto.Nome} ({item.TipoProduto}) - lote {item.Lote}, qtd {item.Quantidade}");
        }

        await _context.SaveChangesAsync();
        return Ok(response);
    }
}
