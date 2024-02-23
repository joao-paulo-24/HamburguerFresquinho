using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayPal.Api;
using ProjetoLDS.Models;
using System.Net.Http;
using WebAPI.Data;
using WebAPI.Enums;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        /// <summary>
        /// Quanto é necessario gastar para ganhar um ponto, 100 = 1€
        /// </summary>
        const int PRECO_POR_PONTO = 100;

        private readonly BurgerShopContext context;

        public PedidosController(BurgerShopContext dBProjectContext) => this.context = dBProjectContext;

        /// <summary>
        /// Retorna todos os pedidos da base de dados
        /// </summary>
        /// <returns>Uma lista com todos os pedidos</returns>
        [HttpGet]
        public ActionResult<IEnumerable<Pedido>> GetPedidos()
        {
            if (context.Pedidos == null)
                return NotFound();
            List<ContaUtilizador> contas = context.ContaUtilizador.ToList();
            return Ok(context.Pedidos);
        }

        /// <summary>
        /// Criar um Pedido
        /// </summary>
        /// <param name="idUtilizador">Utilizador que fara o pedido</param>
        /// <param name="itemsPedido">Items pedidos com as suas alterações</param>
        /// <returns>Pedido criado</returns>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Pedido>> CriarPedido(Pedido pedidoCliente)
        {

            ContaUtilizador contaUtilizador = context.ContaUtilizador.Find(pedidoCliente.ContaUtilizador.Id);

            if (contaUtilizador == null)
                return BadRequest("Utilizador Nao Existe");

            List<ItemPedido> itemsConfirm = new List<ItemPedido>();

            ItemPedido itemPedido;

            foreach (var item in pedidoCliente.ItensPedido)
            {
                ItemCompra itemCompra = context.ItemCompras.Find(item.ItemCompra.Id);

                if (itemCompra == null)
                    return BadRequest("Item Compra Nao existe");

                if (item.EditIngredientes != null)
                {
                    List<EditIngredientes> editIngredientesConfirm = new List<EditIngredientes>();
                    foreach (var editIngrediente in item.EditIngredientes)
                    {
                        EditIngredientes editIngredienteConfirm = ConfirmEditIgrediente(editIngrediente);
                        if (editIngredienteConfirm == null)
                            return BadRequest(editIngrediente);

                        editIngredientesConfirm.Add(editIngredienteConfirm);
                    }

                    itemPedido = new ItemPedido(itemCompra, itemCompra.Preco, editIngredientesConfirm);
                }
                else
                    itemPedido = new ItemPedido(itemCompra, itemCompra.Preco);

                itemPedido.UsePoints = item.UsePoints;


                ////////// ADICIONAR OS INGREDIENTES E OS ITEMS, CASO SEJA ITEM OU SEJA MENU
                if (itemPedido.ItemCompra is WebAPI.Models.Item)
                    AddIngredientes((WebAPI.Models.Item)itemPedido.ItemCompra);


                itemPedido.ItemCompra.Use(itemPedido.EditIngredientes);

                itemsConfirm.Add(itemPedido);
            }

            try
            {
                UsePoints(itemsConfirm, contaUtilizador);
            }
            catch (Exception ExceptionPontos)
            {
                return BadRequest(ExceptionPontos);
            }


            Pedido pedido = new Pedido(contaUtilizador, itemsConfirm);

            contaUtilizador.AdicionarPontos(pedido.PrecoPedido() / PRECO_POR_PONTO);

            context.Pedidos.Add(pedido);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(CriarPedido), new { id = pedido.Id }, pedido);

        }



        private void AddIngredientes(WebAPI.Models.Item item)
        {
            List<Ingrediente> ingredientes = context.Ingredientes.ToList();
            List<IngredienteAtribuido> ingredientesAtribuidos = context.IngredienteAtribuidos.ToList();

            var ingredientesItem = ingredientesAtribuidos
            .Where(ing => ing.ItemId == item.Id)
            .ToList();

            List<Ingrediente> ingredientesFiltrados = ingredientes
            .Where(ing => ingredientesItem.Any(i => i.IdIngrediente == ing.Id))
            .ToList();

            item.Ingredientes = ingredientesFiltrados;
        }

        /// <summary>
        /// Retorna um pedido em específico
        /// </summary>
        /// <param name="id">id do item a retornar</param>
        /// <returns>Ok caso exista o pedido, NotFound caso contrário</returns>
        [HttpGet("{id}")]
        public ActionResult<Pedido> GetPedido(int id)
        {
            if (context.Pedidos == null)
                return NotFound();

            var pedido = context.Pedidos
                     .Include(p => p.ItensPedido)
                     .ThenInclude(ip => ip.ItemCompra)
                     .Include(e => e.ItensPedido)
                     .ThenInclude(ei => ei.EditIngredientes)
                     .ThenInclude(i => i.Ingrediente)
                     .FirstOrDefault(p => p.Id == id);
            if (pedido == null)
                return NotFound(id);

            return Ok(pedido);
        }

        /// <summary>
        /// Edita um pedido da base de dados
        /// </summary>
        /// <param name="id">id do ingpedidorediente a ser editado</param>
        /// <param name="ingredient">Os novos dados para o pedido</param>
        /// <returns>No content caso o edit seja bem sucedido, BadRequest caso contrário</returns>
        //[Authorize]
        [HttpPut("{id}")]
        public IActionResult Edit(int id, Pedido pedido)
        {
            if (!pedido.Id.Equals(id))
            {
                return BadRequest();
            }
            context.Pedidos.Entry(pedido).State = EntityState.Modified;
            context.SaveChanges();
            return NoContent();
        }


        /// <summary>
        /// Corrigir o editIngredientes, enviado pelo utilizador
        /// </summary>
        /// <param name="editIngredientes">Ingrediente enviado pelo utilizador</param>
        /// <returns>Ingrediente Corrigido</returns>
        [Authorize]
        private EditIngredientes? ConfirmEditIgrediente(EditIngredientes editIngredientes)
        {
            Ingrediente ingrediente = context.Ingredientes.Find(editIngredientes.Ingrediente.Id);

            if (ingrediente == null)
                return null;

            WebAPI.Models.Item item = context.Items.Find(editIngredientes.Item.Id);

            if (item == null)
                return null;

            if (!ingrediente.CanEdit())
                return null;

            if (ingrediente.TypeComida != item.TypeComida)
                return null;

            return new EditIngredientes(item, ingrediente, editIngredientes.Active);
        }

        /// <summary>
        /// Usar Pontos para comprar os ItemCompra
        /// </summary>
        [Authorize]
        private void UsePoints(List<ItemPedido> itemsPedido, ContaUtilizador contaUtilizador)
        {
            int pontosNecessariosTotal = 0;

            foreach (var item in itemsPedido)
            {
                if (!item.UsePoints)
                    continue; // Pular itens que o cliente nao quer usar pontos

                if (item.ItemCompra.PontosNecessarios == -1)
                    throw new Exception("Item não é compravel com pontos");

                pontosNecessariosTotal += item.ItemCompra.PontosNecessarios;
            }

            contaUtilizador.RemoverPontos(pontosNecessariosTotal);
        }

        /// <summary>
        /// Obtém todos os pedidos com um certo status
        /// </summary>
        /// <param name="statusPedido">Status do pedido a ser filtrado</param>
        /// <returns>NotFound caso nada seja encontrado, Ok success caso corra tudo bem</returns>
        [Authorize]
        [HttpGet("status{statusPedido}")]
        public ActionResult<Pedido> GetPedido(EnumStatusPedido statusPedido)
        {
            if (context.Pedidos == null)
                return NotFound();

            var pedido = context.Pedidos.Where(p => p.Status == statusPedido);

            if (pedido == null)
                return NotFound();

            return Ok(pedido);
        }

        /// <summary>
        /// Retorna um pedido de um determinado cliente
        /// </summary>
        /// <param name="idCliente">id do cliente a obter o pedido</param>
        /// <returns>NotFound caso não exista, Ok success caso contrário</returns>
        [HttpGet("Cliente/{idCliente}")]
        public ActionResult<Pedido> GetPedidoCliente(int idCliente)
        {
            if (context.Pedidos == null)
                return NotFound();

            var pedido = context.Pedidos.Where(p => p.ContaUtilizador.Id == idCliente);

            if (pedido == null)
                return NotFound();

            return Ok(pedido);
        }

    }
}
