using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLDS.Models;
using WebAPI.Data;
using WebAPI.Identity;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly BurgerShopContext DbContext;
        public ItemsController(BurgerShopContext burgerShopContext) => this.DbContext = burgerShopContext;

        /// <summary>
        /// Método responsável por obter todos os items da base de dados
        /// </summary>
        /// <returns>lista de items</returns>
        [HttpGet]
        public ActionResult<IEnumerable<Item>> GetItems()
        {
            if (DbContext.Items == null)
            {
                return NotFound();
            }
            return Ok(DbContext.Items.ToList());
        }

        /// <summary>
        /// Método usado para obter um item específico por um determinado id
        /// </summary>
        /// <param name="id">id do item a ser filtrado</param>
        /// <returns>o item com o id correspondente ou NotFound caso não exista o item com esse id</returns>
        [HttpGet("{id}")]
        public ActionResult<Item> GetItem(int id)
        {
            if (DbContext.Items == null)
            {
                return NotFound();
            }
            var Item = DbContext.Items
                .SingleOrDefault(s => s.Id == id);
            if (Item == null)
            {
                return NotFound();
            }
            return Ok(Item);
        }

        /// <summary>
        /// Método responsável por adicionar um item à base de dados
        /// </summary>
        /// <param name="Item">item a ser adicionado</param>
        /// <returns>o item criado</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPost]
        public ActionResult<Item> Add(Item Item)
        {
            DbContext.Items.Add(Item);
            DbContext.SaveChanges();
            return CreatedAtAction(nameof(Add), new { id = Item.Id }, Item);
        }

        /// <summary>
        /// Método usado para editar um item por um determinado id
        /// </summary>
        /// <param name="id">id do item a ser alterado</param>
        /// <param name="item">os novos dados para o item</param>
        /// <returns>No content se for bem sucedido, BadRequest caso contrario</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPut("{id}")]
        public IActionResult Edit(int id, Item item)
        {
            if (!item.Id.Equals(id))
            {
                return BadRequest();
            }
            DbContext.Items.Entry(item).State = EntityState.Modified;
            DbContext.SaveChanges();
            return NoContent();
        }

        /// <summary>
        /// Método usado para eliminar um item da base de dados
        /// </summary>
        /// <param name="id">id do item a ser eliminado</param>
        /// <returns>No content se for bem sucedido, BadRequest caso contrario</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (DbContext.Items == null)
            {
                return NotFound();
            }
            var item = DbContext.Items.SingleOrDefault(s => s.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            DbContext.Items.Remove(item);
            DbContext.SaveChanges();
            return NoContent();
        }
    }
}