using FarmaciaSistema.Data;
using FarmaciaSistema.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmaciaSistema.Controllers;

// Catálogo de produtos (RF02: classificação Controlado / Comum / Conveniência)
[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProdutosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Produto>>> GetAll([FromQuery] TipoProduto? tipo)
    {
        var query = _context.Produtos.AsQueryable();

        if (tipo is not null)
            query = query.Where(p => p.TipoProduto == tipo);

        return await query.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Produto>> GetById(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);
        if (produto is null) return NotFound();
        return produto;
    }

    [HttpPost]
    public async Task<ActionResult<Produto>> Create(Produto produto)
    {
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = produto.Id }, produto);
    }
}
