using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLDS.Models;
using System.Net.Http;
using System.Text.Json;
using WebAPI.Data;
using WebAPI.Enums;
using WebAPI.Models;

namespace MVC.Controllers
{
	public class PedidoMVCController : Controller
	{
		private readonly HttpClient httpClient;
		public PedidoMVCController()

		{
			httpClient = new HttpClient
			{
				BaseAddress = new Uri("https://localhost:7235")
			};
		}

		/// <summary>
		/// Função para mostrar pagina inicial com a listagem de pedidos
		/// </summary>
		/// <returns>A pagina web com os pedidos visiveis</returns>
		public async Task<IActionResult> Index()
		{

            if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
            {
                var cookie = JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);
                string userCookieExists = cookie.Role.ToString();
                ViewBag.UserCookieExists = userCookieExists;
                ViewBag.User = cookie;
				
				if (cookie.Role == EnumUserRole.Cliente)
				{
					List<Pedido> PedidosCliente = await httpClient.GetFromJsonAsync<List<Pedido>>($"/api/Pedidos/Cliente/{cookie.Id}");

					return View(PedidosCliente);
				}
			}

            List<Pedido> Pedidos = await httpClient.GetFromJsonAsync<List<Pedido>>("/api/Pedidos");
            return View(Pedidos);
		}

        /// <summary>
        /// Função usada para detalhar os pedidos na pagina de detalhes na web
        /// </summary>
        /// <param name="id">Id do pedido a ser detalhado</param>
        /// <returns>NotFound se não for encontrado o pedido, View com o pedido se este for encontrado</returns>
        public async Task<IActionResult> DetailsAsync(int id)
        {
            List<Pedido> pedidos = await httpClient.GetFromJsonAsync<List<Pedido>>("/api/Pedidos");

            if (id == null)
            {
                return BadRequest();
            }

            if (id.Equals(""))
            {
                return NotFound();
            }
            Pedido pedidoEncontrado = null;

            foreach (var pedido in pedidos)
            {
                if (pedido.Id == id)
                {
                    pedidoEncontrado = pedido;
                    break; // Se o menu for encontrado, podemos sair do loop.
                }
            }
            Pedido tempPedido = await httpClient.GetFromJsonAsync<Pedido>($"/api/Pedidos/{pedidoEncontrado.Id}");

            tempPedido.ContaUtilizador = pedidoEncontrado.ContaUtilizador;
            ViewBag.Pedido = tempPedido;
            if (pedidoEncontrado == null)
            {
                return NotFound();
            }
            return View(pedidoEncontrado);

        }

        /// <summary>
        /// Método que permite alterar o estado do pedido
        /// </summary>
        /// <param name="pedidoId">Id do pedido</param>
        /// <param name="NovoEstado">Novo estado para o pedido</param>
        /// <returns>retorna a view dos pedidos, ou notFound aso não encontre um pedido com esse Id</returns>
        [HttpPost]
		public async Task<IActionResult> AlterarEstado(int pedidoId, string NovoEstado)
		{
			List<Pedido> pedidos = await httpClient.GetFromJsonAsync<List<Pedido>>("/api/Pedidos");
			List<ContaUtilizador> contas = await httpClient.GetFromJsonAsync<List<ContaUtilizador>>("/api/ContaUtilizador");

			Pedido pedidoEncontrado = pedidos.FirstOrDefault(p => p.Id == pedidoId);
			ContaUtilizador contaEncontrada = contas.FirstOrDefault(t => t.Email == pedidoEncontrado.ContaUtilizador.Email);
			ContaUtilizador contaUtilizadorEncontrada = contas.FirstOrDefault(c => c.Id == contaEncontrada.Id);




			if (pedidoEncontrado != null)
			{
				if (NovoEstado.Equals("Pendente"))
				{
					pedidoEncontrado.Status = (EnumStatusPedido)0;
				}
				else if (NovoEstado.Equals("Preparar"))
				{
					pedidoEncontrado.Status = (EnumStatusPedido)1;
				}
				else if (NovoEstado.Equals("Pronto"))
				{
					pedidoEncontrado.Status = (EnumStatusPedido)2;
				}
				else if (NovoEstado.Equals("Entregue"))
				{
					pedidoEncontrado.Status = (EnumStatusPedido)3;
				}
				else if (NovoEstado.Equals("Cancelado"))
				{
					pedidoEncontrado.Status = (EnumStatusPedido)4;
				}
				HttpResponseMessage response = httpClient.PutAsJsonAsync($"api/Pedidos/{pedidoEncontrado.Id}", pedidoEncontrado).Result;
				if (!response.IsSuccessStatusCode)
				{
					// Lógica para lidar com falhas na comunicação com a API
					// Pode ser útil logar ou exibir mensagens de erro
					var statusCode = response.StatusCode;
					ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Código de status: {statusCode}");
					var errorContent = response.Content.ReadAsStringAsync().Result;
					ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Detalhes: {errorContent}");
					return BadRequest(pedidoEncontrado);
				}
				return RedirectToAction("Index");
			}
			else
			{
				return NotFound();
			}

		}

        /// <summary>
        /// Função Responsavel por apresentar apresentar a View de criação de pedido ao utilizador
        /// </summary>
        /// <returns>View de create</returns>
        public IActionResult Create()
		{
			return View();
		}

		/// <summary>
		/// Função que envia um pedido do utilizador para o WebApi
		/// </summary>
		/// <param name="pedidoViewModel">Pedido do Utilizador</param>
		/// <returns>View dos pedidos</returns>
		[ValidateAntiForgeryToken]
		[HttpPost]
		public IActionResult Create(Pedido pedidoViewModel)
		{
			if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
			{
				var cookie = JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);
				pedidoViewModel.ContaUtilizador.Email = cookie.Email;

				// Serializar o objeto pedidoViewModel para JSON e enviá-lo para a API
				HttpResponseMessage response = httpClient.PostAsJsonAsync("api/pedidos", pedidoViewModel).Result;

				// Verificar se a solicitação foi bem-sucedida
				if (response.IsSuccessStatusCode)
				{
					// Redirecionar para a página desejada após o processamento
					return RedirectToAction("Index");
				}
				else
				{
					// Lógica para lidar com falhas na comunicação com a API
					var statusCode = response.StatusCode;
					ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Código de status: {statusCode}");
					var errorContent = response.Content.ReadAsStringAsync().Result;
					ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Detalhes: {errorContent}");
					return View(pedidoViewModel);
				}
			}
			return RedirectToAction("Index");
		}

        /// <summary>
        /// Método para alterar o estado de pagamento do pedido
        /// </summary>
        /// <param name="pedidoId">Id do pedido</param>
        /// <param name="NovoEstadoPagamento">Novo estado de pagamento</param>
        /// <returns>View dos pedidos</returns>
        [HttpPost]
		public async Task<IActionResult> AlterarEstadoPagamento(int pedidoId, string NovoEstadoPagamento)
		{
			List<Pedido> pedidos = await httpClient.GetFromJsonAsync<List<Pedido>>("/api/Pedidos");
			List<ContaUtilizador> contas = await httpClient.GetFromJsonAsync<List<ContaUtilizador>>("/api/ContaUtilizador");

			Pedido pedidoEncontrado = pedidos.FirstOrDefault(p => p.Id == pedidoId);
			ContaUtilizador contaEncontrada = contas.FirstOrDefault(t => t.Email == pedidoEncontrado.ContaUtilizador.Email);
			ContaUtilizador contaUtilizadorEncontrada = contas.FirstOrDefault(c => c.Id == contaEncontrada.Id);


			if (pedidoEncontrado != null)
			{
				if (NovoEstadoPagamento.Equals("False"))
				{
					pedidoEncontrado.Pago = false;
				}
				else if (NovoEstadoPagamento.Equals("True"))
				{
					pedidoEncontrado.Pago = true;
				}
				HttpResponseMessage response = httpClient.PutAsJsonAsync($"api/Pedidos/{pedidoEncontrado.Id}", pedidoEncontrado).Result;
				if (!response.IsSuccessStatusCode)
				{
					// Lógica para lidar com falhas na comunicação com a API
					var statusCode = response.StatusCode;
					ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Código de status: {statusCode}");
					var errorContent = response.Content.ReadAsStringAsync().Result;
					ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Detalhes: {errorContent}");
					return BadRequest(pedidoEncontrado);
				}
				return RedirectToAction("Index");
			}
			else
			{
				return NotFound();
			}

		}


	}
}

