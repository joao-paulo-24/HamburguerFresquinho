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
    public class StockController : ControllerBase
    {
        private readonly BurgerShopContext DbContext;
        public StockController(BurgerShopContext burgerShopContext) => this.DbContext = burgerShopContext;

        /// <summary>
        /// Obtém todos os ingredientes da base de dados
        /// </summary>
        /// <returns>Uma lista com os ingredientes da base de dados</returns>
        [HttpGet]
        public ActionResult<IEnumerable<Ingrediente>> GetIngredients()
        {
            if (DbContext.Ingredientes == null)
            {
                return NotFound();
            }
            return Ok(DbContext.Ingredientes.ToList());
        }

        /// <summary>
        /// Obtém um ingrediente específico de um determinado id
        /// </summary>
        /// <param name="id">id do ingrediente a obter</param>
        /// <returns>Ok caso o ingrediente exista, NotFound caso contrário</returns>
        [Authorize]
        [HttpGet("{id}")]
        public ActionResult<Ingrediente> GetIngredient(int id)
        {
            if (DbContext.Ingredientes == null)
            {
                return NotFound();
            }
            var ingrediente = DbContext.Ingredientes.SingleOrDefault(s => s.Id == id);
            if (ingrediente == null)
            {
                return NotFound();
            }
            return Ok(ingrediente);
        }

        /// <summary>
        /// Adiciona um ingrediente à base de dados
        /// </summary>
        /// <param name="ingrediente">Dados do ingrediente a ser adicionado</param>
        /// <returns>O ingrediente criado</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPost]
        public ActionResult<Ingrediente> Add(Ingrediente ingrediente)
        {
            DbContext.Ingredientes.Add(ingrediente);
            DbContext.SaveChanges();
            return CreatedAtAction(nameof(Add), new { id = ingrediente.Id }, ingrediente);
        }

        /// <summary>
        /// Edita um ingrediente da base de dados
        /// </summary>
        /// <param name="id">id do ingrediente a ser editado</param>
        /// <param name="ingredient">Os novos dados para o ingrediente</param>
        /// <returns>No content caso o edit seja bem sucedido, BadRequest caso contrário</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPut("{id}")]
        public IActionResult Edit(int id, Ingrediente ingredient)
        {
            if (!ingredient.Id.Equals(id))
            {
                return BadRequest();
            }
            DbContext.Ingredientes.Entry(ingredient).State = EntityState.Modified;
            DbContext.SaveChanges();
            return NoContent();
        }

        /// <summary>
        /// Elimina um ingrediente da base de dados
        /// </summary>
        /// <param name="id">id do ingrediente a ser eliminado</param>
        /// <returns>No content caso o edit seja bem sucedido, BadRequest caso contrário</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (DbContext.Ingredientes == null)
            {
                return NotFound();
            }
            var ingredient = DbContext.Ingredientes.SingleOrDefault(s => s.Id == id);
            if (ingredient == null)
            {
                return NotFound();
            }
            DbContext.Ingredientes.Remove(ingredient);
            DbContext.SaveChanges();
            return NoContent();
        }
    }
}