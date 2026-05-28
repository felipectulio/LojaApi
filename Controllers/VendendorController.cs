using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LojaApi.Data;
using LojaApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace LojaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendedorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VendedorController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/vendedor
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vendedor>>> Get()
        {
            return await _context.Vendedores.ToListAsync();
        }

        // POST: api/vendedor
        [HttpPost]
        public async Task<ActionResult> Post(Vendedor vendedor)
        {
            _context.Vendedores.Add(vendedor);
            await _context.SaveChangesAsync();
            return Ok(vendedor);
        }

        // PUT: api/vendedor/{codigo}
        [HttpPut("{codigo}")]
        public async Task<ActionResult> Put(int codigo, Vendedor vendedor)
        {
            var vendedorBanco = await _context.Vendedores.FindAsync(codigo);
            if (vendedorBanco == null)
            {
                return NotFound();
            }

            vendedorBanco.Nome = vendedor.Nome;
            vendedorBanco.Email = vendedor.Email;
            vendedorBanco.Telefone = vendedor.Telefone;
            vendedorBanco.Salario = vendedor.Salario;

            await _context.SaveChangesAsync();
            return Ok(vendedorBanco);
        }

        // DELETE: api/vendedor/{codigo}
        [HttpDelete("{codigo}")]
        public async Task<ActionResult> Delete(int codigo)
        {
            var vendedor = await _context.Vendedores.FindAsync(codigo);
            if (vendedor == null)
            {
                return NotFound();
            }

            _context.Vendedores.Remove(vendedor);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // QUESTÃO 8 - GET: api/vendedor/salario/{valor}
        [HttpGet("salario/{valor}")]
        public async Task<ActionResult<IEnumerable<Vendedor>>> GetBySalario(decimal valor)
        {
            // Filtra os vendedores com salário estritamente MAIOR que o valor informado
            var vendedores = await _context.Vendedores
                .Where(v => v.Salario > valor)
                .ToListAsync();

            return Ok(vendedores);
        }
    }
}