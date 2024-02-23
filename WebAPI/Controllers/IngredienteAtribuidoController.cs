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
    public class IngredienteAtribuidoController : ControllerBase
    {
        private readonly BurgerShopContext DbContext;
        public IngredienteAtribuidoController(BurgerShopContext burgerShopContext) => this.DbContext = burgerShopContext;

        /// <summary>
		/// Obtém todos os ingredientes atribuídos da base de dados
		/// </summary>
		/// <returns>Uma lista de todos os ingredientes atribuídos presentes na base de dados</returns>
        [HttpGet]
        public ActionResult<IEnumerable<IngredienteAtribuido>> GetIngredients()
        {
            if (DbContext.IngredienteAtribuidos == null)
            {
                return NotFound();
            }
            return Ok(DbContext.IngredienteAtribuidos.ToList());
        }

        /// <summary>
		/// Atribui o ingrediente ao item de forma a que estes estam logicamente ligados
		/// </summary>
		/// <param name="ingrediente">ingrediente a ser atribuído</param>
		/// <returns>Ingrediente atribuído</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPost]
        public ActionResult<IngredienteAtribuido> Add(IngredienteAtribuido ingrediente)
        {
            DbContext.IngredienteAtribuidos.Add(ingrediente);
            DbContext.SaveChanges();
            return CreatedAtAction(nameof(Add), new { id = ingrediente.Id }, ingrediente);
        }

        /// <summary>
		/// Edita um ingrediente atribuído a um certo item
		/// </summary>
		/// <param name="id">id do ingrediente a ser editado</param>
		/// <param name="ingredient">novos dados do ingrediente a ser editado</param>
		/// <returns>BadRequest caso algo não corra bem, NoContent caso contrário</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPut("{id}")]
        public IActionResult Edit(int id, IngredienteAtribuido ingredient)
        {
            if (!ingredient.Id.Equals(id))
            {
                return BadRequest();
            }
            DbContext.IngredienteAtribuidos.Entry(ingredient).State = EntityState.Modified;
            DbContext.SaveChanges();
            return NoContent();
        }

        /// <summary>
		/// Elimina um ingrediente atribuído a um item
		/// </summary>
		/// <param name="id">id do ingrediente a ser eliminado</param>
		/// <returns>BadRequest caso algo não corra bem, NoContent caso contrário</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (DbContext.IngredienteAtribuidos == null)
            {
                return NotFound();
            }
            var ingredient = DbContext.IngredienteAtribuidos.SingleOrDefault(s => s.Id == id);
            if (ingredient == null)
            {
                return NotFound();
            }
            DbContext.IngredienteAtribuidos.Remove(ingredient);
            DbContext.SaveChanges();
            return NoContent();
        }
    }
}
