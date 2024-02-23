using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Identity;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemAtribuidoController : ControllerBase
    {
        private readonly BurgerShopContext DbContext;
        public ItemAtribuidoController(BurgerShopContext burgerShopContext) => this.DbContext = burgerShopContext;

        /// <summary>
		/// Devolve todos os items atribuídos existentes na base de dados
		/// </summary>
		/// <returns>Uma lista de todos os items atribuídos existentes</returns>
        [HttpGet]
        public ActionResult<IEnumerable<ItemAtribuido>> GetItems()
        {
            if (DbContext.ItemAtribuidos == null)
            {
                return NotFound();
            }
            return Ok(DbContext.ItemAtribuidos.ToList());
        }

        /// <summary>
		/// Atribui o item ao menu de forma a que estes estam logicamente ligados
		/// </summary>
		/// <param name="item">item a ser atribuído</param>
		/// <returns>o item atribuído</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPost]
        public ActionResult<ItemAtribuido> Add(ItemAtribuido item)
        {
            DbContext.ItemAtribuidos.Add(item);
            DbContext.SaveChanges();
            return CreatedAtAction(nameof(Add), new { id = item.Id }, item);
        }

        /// <summary>
		/// Edita um item atribuído a um certo menu
		/// </summary>
		/// <param name="id">id do item atribuído a ser editado</param>
		/// <param name="item">dados do novo item atribuído</param>
		/// <returns>BadRequest caso algo não corra bem, NoContent caso contrário</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPut("{id}")]
        public IActionResult Edit(int id, ItemAtribuido item)
        {
            if (!item.Id.Equals(id))
            {
                return BadRequest();
            }
            DbContext.ItemAtribuidos.Entry(item).State = EntityState.Modified;
            DbContext.SaveChanges();
            return NoContent();
        }

        /// <summary>
		/// Elimina um item atribuído
		/// </summary>
		/// <param name="id">id do item atribuído a ser eliminado</param>
		/// <returns>NotFound caso algo não corra bem, NoContent caso contrário</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (DbContext.ItemAtribuidos == null)
            {
                return NotFound();
            }
            var item = DbContext.ItemAtribuidos.SingleOrDefault(s => s.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            DbContext.ItemAtribuidos.Remove(item);
            DbContext.SaveChanges();
            return NoContent();
        }
    }
}