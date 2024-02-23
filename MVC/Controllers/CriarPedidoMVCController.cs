using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using Newtonsoft.Json;
using PayPal.Api;
using ProjetoLDS.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using WebAPI.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MVC.Controllers
{
	public class CriarPedidoMVCController : Controller
	{
		private readonly HttpClient httpClient;

		List<ItemCompra> itemsCarrinho;

		private readonly IHttpContextAccessor httpContextAccessor;

		public CriarPedidoMVCController(IHttpContextAccessor httpContextAccessor)
		{
			httpClient = new HttpClient
			{
				BaseAddress = new Uri("https://localhost:7235")
			};

			this.httpContextAccessor = httpContextAccessor;

			InitializeItemsCarrinhoAsync().Wait();
		}

        /// <summary>
        /// Método que inicializa o carrinho de compras.
        /// </summary>
        /// <returns>item adicionado ao carrinho</returns>
        private async Task InitializeItemsCarrinhoAsync()
		{
			itemsCarrinho = new List<ItemCompra>();

			int? numItems = httpContextAccessor.HttpContext.Session.GetInt32("numItems");
			if (numItems.HasValue)
			{
				for (int i = 0; i < numItems; i++)
				{
					int? itemId = httpContextAccessor.HttpContext.Session.GetInt32($"itemCarrinho{i}");
					if (itemId.HasValue)
					{
						if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
						{
							var cookie = JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);
							ViewBag.User = cookie;
						}

						ItemCompra item = await httpClient.GetFromJsonAsync<ItemCompra>($"/api/ItemCompra/{itemId}");
						itemsCarrinho.Add(item);
					}
				}
			}
		}

        /// <summary>
        /// Método que lista todos os items/menus de forma a que estes possam ser adicionados ao carrinho.
        /// </summary>
        /// <returns>view de uma lista de items/menus a escolher</returns>
        public async Task<IActionResult> Index()
		{
			if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
			{
				var cookie = JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);
				ViewBag.User = cookie;
				// Obtenha o token usando o método adequado (não está claro no código fornecido)
				var tokenResponse = await httpClient.PostAsJsonAsync("api/ContaUtilizador/tokenLogged", cookie);

				if (tokenResponse.IsSuccessStatusCode)
				{

					var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

					// Configure o token nos cabeçalhos de autorização
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenContent);

					// List<ItemCompra> itemsCompra = await httpClient.GetFromJsonAsync<List<ItemCompra>>("/api/ItemCompra");

					// CriarPedidoViewModel pedidoViewModel = new CriarPedidoViewModel(itemsCarrinho, itemsCompra);


					List<WebAPI.Models.Item> items = await httpClient.GetFromJsonAsync<List<WebAPI.Models.Item>>("/api/Items");
					List<WebAPI.Models.Menu> menu = await httpClient.GetFromJsonAsync<List<WebAPI.Models.Menu>>("/api/Menu");

					List<ItemCompra> itemCompra = new List<ItemCompra>(items);

					foreach (var item in items)
					{
						item.Ingredientes = await GetIngredientes(item.Id);
					}

					itemCompra.AddRange(menu);

					CriarPedidoViewModel pedidoViewModel = new CriarPedidoViewModel(itemsCarrinho, itemCompra);

					return View(pedidoViewModel);
				}
			}
			return RedirectToAction("Index", "Home");
		}


        /// <summary>
        /// Método que retorna os ingredientes de um item.
        /// </summary>
        /// <param name="itemId">Id do item cujos ingredientes irão ser retornados</param>
        /// <returns>retorna o json coms os ingredientes retornados do método "GetIngredientes()"</returns>
        [HttpGet]
		public async Task<IActionResult> GetIngredientesItem(int itemId)
		{
			if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
			{
				var cookie = JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);
				ViewBag.User = cookie;

				// Obtenha o token usando o método adequado (não está claro no código fornecido)
				var tokenResponse = await httpClient.PostAsJsonAsync("api/ContaUtilizador/tokenLogged", cookie);

				if (tokenResponse.IsSuccessStatusCode)
				{

					var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

					// Configure o token nos cabeçalhos de autorização
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenContent);

					return Json(await GetIngredientes(itemId));
				}
			}
			return Json(null);
		}

        /// <summary>
        /// Método que retorna os ingredientes de um item
        /// </summary>
        /// <param name="itemId">Id do item cujos ingredientes irão ser retornados</param>
        /// <returns>inrgedientes do item</returns>
        private async Task<List<Ingrediente>> GetIngredientes(int itemId)
		{
			List<Ingrediente> ingredientes = await httpClient.GetFromJsonAsync<List<Ingrediente>>("/api/Stock");
			List<IngredienteAtribuido> ingredientesAtribuidos = await httpClient.GetFromJsonAsync<List<IngredienteAtribuido>>("/api/IngredienteAtribuido");

			List<Ingrediente> todosIngredientes = ingredientes.ToList();

			var ingredientesItem = ingredientesAtribuidos
			.Where(ing => ing.ItemId == itemId)
			.ToList();

			List<Ingrediente> ingredientesFiltrados = todosIngredientes
			.Where(ing => ingredientesItem.Any(i => i.IdIngrediente == ing.Id))
			.ToList();

			return ingredientesFiltrados;
		}

        /// <summary>
        /// Função usada confirmar o pedido
        /// </summary>
        /// <param name="data">todo o conteúdo do pedido</param>
        /// <returns>retorna a view da criação do pedido, ou leva para a página de confirmação do pedido</returns>
        [ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<IActionResult> ConfirmarAsync(IFormCollection data)
		{
			if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
			{
				var cookie = JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);
				ViewBag.User = cookie;

				// Obtenha o token usando o método adequado (não está claro no código fornecido)
				var tokenResponse = await httpClient.PostAsJsonAsync("api/ContaUtilizador/tokenLogged", cookie);

				if (tokenResponse.IsSuccessStatusCode)
				{
					var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

					// Configure o token nos cabeçalhos de autorização
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenContent);

					Pedido pedido = JsonConvert.DeserializeObject<Pedido>(data["Pedido"]);
					if (pedido == null)
					{
						return RedirectToAction("Index");
					}
					pedido.ContaUtilizador.Email = cookie.Email;

					HttpResponseMessage response = httpClient.PostAsJsonAsync("api/Pedidos", pedido).Result;

					return View();
				}
			}
			return RedirectToAction("Index");
		}
	}
}
