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
    public class ItemCompraController : ControllerBase
    {
        private readonly BurgerShopContext context;
        public ItemCompraController(BurgerShopContext dBProjectContext) => this.context = dBProjectContext;

        /// <summary>
        /// Obtém todos os itemPedidos presentes na base de dados
        /// </summary>
        /// <returns>Uma lista com os itemPedidos presentes na base de dados</returns>
        [HttpGet]
        public ActionResult<IEnumerable<ItemPedido>> GetItemCompra()
        {
            if (context.ItemCompras == null)
            {
                return NotFound();
            }
            return Ok(context.ItemCompras.ToList());
        }

        /// <summary>
        /// Retorna um item compra específico
        /// </summary>
        /// <param name="id">id do itemcompra a retornar</param>
        /// <returns>NotFound caso o item não exista, Ok success caso tudo corra bem</returns>
        [HttpGet("{id}")]
        public ActionResult<ItemCompra> GetItem(int id)
        {
            if (context.ItemCompras == null)
            {
                return NotFound();
            }

            var item = context.ItemCompras.SingleOrDefault(s => s.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            if (item is Menu)
            {
                return Ok(context.Menus.Include(m => m.Items).SingleOrDefault(s => s.Id == id));
            }

            return Ok(item);
        }
    }
}