using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Identity;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly BurgerShopContext context;
        public MenuController(BurgerShopContext dBProjectContext) => this.context = dBProjectContext;

        /// <summary>
		/// Adiciona um menu à base de dados
		/// </summary>
		/// <param name="menu">Menu a ser adicionado</param>
		/// <returns>O menu  criado</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPost]
        public ActionResult<Menu> Add(Menu menu)
        {
            context.Menus.Add(menu);
            context.SaveChanges();
            return CreatedAtAction(nameof(Add), new { id = menu.Id }, menu);
        }

        /// <summary>
		/// Retorna todos os menús da base de dados
		/// </summary>
		/// <returns>retorna uma lista dos menus</returns>
        [HttpGet]
        public ActionResult<IEnumerable<Menu>> GetMenus()
        {
            var menusWithItems = context.Menus.ToList();

            if (menusWithItems == null)
            {
                return NotFound();
            }

            return Ok(menusWithItems);
        }

        /// <summary>
		/// Obtém um menu específico
		/// </summary>
		/// <param name="id">id do menu a ser obtido</param>
		/// <returns>NotFound caso o menu nao exista, Ok success caso contrário</returns>
        [HttpGet("{id}")]
        public ActionResult<Menu> GetMenu(int id)
        {
            if (context.Menus == null)
            {
                return NotFound();
            }
            var Menu = context.Menus.Include(m => m.Items).SingleOrDefault(s => s.Id == id);
            if (Menu == null)
            {
                return NotFound();
            }
            return Ok(Menu);
        }

        /// <summary>
		/// Edita um menú na base de dados
		/// </summary>
		/// <param name="id">id do menú a ser editado</param>
		/// <param name="menu">Novos dados do menú a ser editado</param>
		/// <returns>BadRequest caso algo não corra bem, NoContent caso contrário</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        public IActionResult Edit(int id, Menu menu)
        {
            if (!menu.Id.Equals(id))
            {
                return BadRequest();
            }
            context.Menus.Entry(menu).State = EntityState.Modified;
            context.SaveChanges();
            return NoContent();
        }

        /// <summary>
		/// Elimina um menú da base de dados
		/// </summary>
		/// <param name="id">id do item a ser eliminado</param>
		/// <returns>NotFound caso algo não corra bem, NoContent caso contrário</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (context.Menus == null)
            {
                return NotFound();
            }
            var menu = context.Menus.SingleOrDefault(s => s.Id == id);
            if (menu == null)
            {
                return NotFound();
            }
            context.Menus.Remove(menu);
            context.SaveChanges();
            return NoContent();
        }
    }
}