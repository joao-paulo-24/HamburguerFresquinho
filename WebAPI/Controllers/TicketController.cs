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
    public class TicketController : ControllerBase
    {
        private readonly BurgerShopContext context;
        public TicketController(BurgerShopContext dBProjectContext) => this.context = dBProjectContext;

        /// <summary>
        /// Obtém todos os tickets presentes na base de dados
        /// </summary>
        /// <returns>Uma lista com os tickets presentes na base de dados</returns>
        [HttpGet]
        public ActionResult<IEnumerable<Ticket>> GetTickets()
        {
            if (context.Ticket == null)
            {
                return NotFound();
            }
            List<ContaUtilizador> contas = context.ContaUtilizador.ToList();
            return Ok(context.Ticket.ToList());
        }

        /// <summary>
        /// Obtém o ticket de um determinado id
        /// </summary>
        /// <param name="id">id do ticket a obter</param>
        /// <returns>Ok caso o ticket exista, NotFound caso contrário</returns>
        [HttpGet("{id}")]
        public ActionResult<Ticket> GetTicket(int id)
        {
            if (context.Ticket == null)
            {
                return NotFound();
            }
            var ticket = context.Ticket.SingleOrDefault(t => t.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }
            return Ok(ticket);
        }

        /// <summary>
        /// Adiciona um ticket à base de dados
        /// </summary>
        /// <param name="ticket">ticket a ser adicionado</param>
        /// <returns>O ticket criado</returns>
        [Authorize]
        [HttpPost]
        public ActionResult<Ticket> Add(Ticket ticket)
        {
            ContaUtilizador conta = context.ContaUtilizador.FirstOrDefault(c => c.Email == ticket.ContaUtilizador.Email);
            ticket.ContaUtilizador = conta;
            context.Ticket.Add(ticket);
            context.SaveChanges();
            return CreatedAtAction(nameof(Add), new { id = ticket.Id }, ticket);
        }

        /// <summary>
        /// Edita um ticket na base de dados
        /// </summary>
        /// <param name="id">id do ticket a ser editado</param>
        /// <param name="ticket">Os novos dados para o ticket</param>
        /// <returns>No content caso o edit seja bem sucedido, BadRequest caso contrário</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpPut("{id}")]
        public IActionResult Edit(int id, Ticket ticket)
        {
            if (!ticket.Id.Equals(id))
            {
                return BadRequest();
            }
            context.Ticket.Entry(ticket).State = EntityState.Modified;
            context.SaveChanges();
            return NoContent();
        }

        /// <summary>
        /// Elimina um ticket da base de dados
        /// </summary>
        /// <param name="id">id do ticket a ser eliminado</param>
        /// <returns>No content caso o edit seja bem sucedido, BadRequest caso contrário</returns>
        [Authorize(Policy = IdentityData.AdminUserPolicyName)]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (context.Ticket == null)
            {
                return NotFound();
            }
            var ticket = context.Ticket.SingleOrDefault(t => t.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }
            context.Ticket.Remove(ticket);
            context.SaveChanges();
            return NoContent();
        }

    }
}