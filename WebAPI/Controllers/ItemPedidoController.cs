using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLDS.Models;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemPedidoController : ControllerBase
    {
        private readonly BurgerShopContext context;
        public ItemPedidoController(BurgerShopContext dBProjectContext) => this.context = dBProjectContext;

        /// <summary>
        /// Obtém todos os itemPedidos presentes na base de dados
        /// </summary>
        /// <returns>Uma lista com os itemPedidos presentes na base de dados</returns>
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<ItemPedido>> GetItemPedidos()
        {
            if (context.ItemPedidos == null)
            {
                return NotFound();
            }
            return Ok(context.ItemPedidos.ToList());
        }

        /// <summary>
        /// Obtém o itemPedido de um determinado id
        /// </summary>
        /// <param name="id">id do itemPedido a obter</param>
        /// <returns>Ok caso o itemPedido exista, NotFound caso contrário</returns>
        [Authorize]
        [HttpGet("{id}")]
        public ActionResult<ItemPedido> GetItemPedido(int id)
        {
            if (context.ItemPedidos == null)
            {
                return NotFound();
            }
            var itemPedido = context.ItemPedidos.SingleOrDefault(i => i.Id == id);
            if (itemPedido == null)
            {
                return NotFound();
            }
            return Ok(itemPedido);
        }

        /// <summary>
        /// Adiciona um itemPedido à base de dados
        /// </summary>
        /// <param name="itemPedido">itemPedido a ser adicionado</param>
        /// <returns>O itemPedido criado</returns>
        [Authorize]
        [HttpPost]
        public ActionResult<ItemPedido> Add(ItemPedido itemPedido)
        {
            context.ItemPedidos.Add(itemPedido);
            context.SaveChanges();
            return CreatedAtAction(nameof(Add), new { id = itemPedido.Id }, itemPedido);
        }

        /// <summary>
        /// Edita um itemPedido na base de dados
        /// </summary>
        /// <param name="id">id do itemPedido a ser editado</param>
        /// <param name="itemPedido">Os novos dados para o itemPedido</param>
        /// <returns>No content caso o edit seja bem sucedido, BadRequest caso contrário</returns>
        [Authorize]
        [HttpPut("{id}")]
        public IActionResult Edit(int id, ItemPedido itemPedido)
        {
            if (!itemPedido.Id.Equals(id))
            {
                return BadRequest();
            }
            context.ItemPedidos.Entry(itemPedido).State = EntityState.Modified;
            context.SaveChanges();
            return NoContent();
        }

        /// <summary>
        /// Elimina um itemPedido da base de dados
        /// </summary>
        /// <param name="id">id do itemPedido a ser eliminado</param>
        /// <returns>No content caso o edit seja bem sucedido, BadRequest caso contrário</returns>
        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (context.ItemPedidos == null)
            {
                return NotFound();
            }
            var itemPedido = context.ItemPedidos.SingleOrDefault(i => i.Id == id);
            if (itemPedido == null)
            {
                return NotFound();
            }
            context.ItemPedidos.Remove(itemPedido);

            context.SaveChanges();
            return NoContent();
        }
    }
}