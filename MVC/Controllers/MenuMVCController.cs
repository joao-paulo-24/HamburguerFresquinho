using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLDS.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using WebAPI.Data;
using WebAPI.Models;

namespace MVC.Controllers
{
    public class MenuMVCController : Controller
    {
        private readonly HttpClient httpClient;
        public MenuMVCController()

        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7235") // Defina sua URL da Web API aqui
            };
        }

        /// <summary>
        /// Função para mostrar pagina inicial com a listagem de menus
        /// </summary>
        /// <returns>A pagina web com os menus visiveis</returns>
        public async Task<IActionResult> Index()
        {
            if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
            {
                var cookie = System.Text.Json.JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);
                string userCookieExists = cookie.Role.ToString();
                ViewBag.UserCookieExists = userCookieExists;
            }

            List<Menu> menus = await httpClient.GetFromJsonAsync<List<Menu>>("/api/Menu");
            return View(menus);
        }

        /// <summary>
        /// Método que devolve a view de criação de um menu
        /// </summary>
        /// <returns>View de criação de um menu</returns>
        public async Task<IActionResult> CreateAsync()
        {
            List<Item> items = await httpClient.GetFromJsonAsync<List<Item>>("/api/Items");


            Menu menu = new Menu();
            menu.Items = items.ToList();
            ViewBag.Menu = menu;
            return View(menu);
        }

        //// <summary>
        /// Função que envia um item do utilizador para o WebApi
        /// </summary>
        /// <param name="itemViewModel">Item do Utilizador</param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> CreateAsync(Menu menu, int[] itemsSelecionados)
        {
			if (itemsSelecionados == null || itemsSelecionados.Length == 0)
			{
				return RedirectToAction("Index");
			}
			if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
            {
                var cookie = JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);

                // Obtenha o token usando o método adequado (não está claro no código fornecido)
                var tokenResponse = await httpClient.PostAsJsonAsync("api/ContaUtilizador/tokenLogged", cookie);

                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

                    // Configure o token nos cabeçalhos de autorização
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenContent);

                    List<Item> items = await httpClient.GetFromJsonAsync<List<Item>>("/api/Items");

                    if (menu.Image != null)
                    {
                        // Salvar o arquivo no sistema de arquivos do servidor
                        menu.ImagePath = SaveImage(menu.Image);
                    }
                    menu.Image = null;


                    // Serializar o objeto itemViewModel para JSON e enviá-lo para a API
                    HttpResponseMessage response = httpClient.PostAsJsonAsync("api/Menu", menu).Result;
                    Menu createdMenu = await response.Content.ReadFromJsonAsync<Menu>();

                    foreach (var item in itemsSelecionados)
                    {
                        ItemAtribuido itemAtribuido = new ItemAtribuido();
                        itemAtribuido.MenuName = createdMenu.Name;
                        itemAtribuido.IdItem = item;
                        itemAtribuido.MenuId = createdMenu.Id;
                        HttpResponseMessage response1 = httpClient.PostAsJsonAsync($"api/ItemAtribuido", itemAtribuido).Result;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        // Lógica para lidar com falhas na comunicação com a API
                        // Pode ser útil logar ou exibir mensagens de erro
                        var statusCode = response.StatusCode;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Código de status: {statusCode}");
                        var errorContent = response.Content.ReadAsStringAsync().Result;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Detalhes: {errorContent}");
                        return View(menu);
                    }
                }
            }

            return RedirectToAction("Index");
        }


        /// <summary>
        /// Função usada para guardar a imagem do Menu
        /// </summary>
        /// <param name="imageFile">Imagem a ser guardada</param>
        /// <returns>Path da imagem</returns>
        private string SaveImage(IFormFile imageFile)
        {
            string uploadsFolder = Path.Combine("wwwroot", "Images", "ItemImages");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(fileStream);
            }

            return uniqueFileName;
        }



        /// <summary>
        /// Função usada para detalhar os menus na pagina de detalhes na web
        /// </summary>
        /// <param name="id">nome do menu a ser detalhado</param>
        /// <returns>NotFound se não for encontrado o menu, View com o menu se este for encontrado</returns>
        [HttpGet]
        public async Task<IActionResult> Details(int Id)
        {
            {
                if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
                {
                    var cookie = JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);

                    // Obtenha o token usando o método adequado (não está claro no código fornecido)
                    var tokenResponse = await httpClient.PostAsJsonAsync("api/ContaUtilizador/tokenLogged", cookie);

                    if (tokenResponse.IsSuccessStatusCode)
                    {
                        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

                        // Configure o token nos cabeçalhos de autorização
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenContent);

                        List<Menu> menus = await httpClient.GetFromJsonAsync<List<Menu>>("/api/Menu");
                        List<Item> items = await httpClient.GetFromJsonAsync<List<Item>>("/api/Items");
                        List<ItemAtribuido> itemsAtribuidos = await httpClient.GetFromJsonAsync<List<ItemAtribuido>>("/api/ItemAtribuido");

                        if (Id == null)
                        {
                            return BadRequest();
                        }

                        if (Id.Equals(""))
                        {
                            return NotFound();
                        }
                        Menu menuEncontrado = null;

                        for (int i = 0; i < menus.Count; i++)
                        {
                            if (menus[i].Id == Id)
                            {
                                menuEncontrado = menus[i];
                                break; 
                            }
                        }

                        if (menuEncontrado == null)
                        {
                            return NotFound(); // Menu não encontrado
                        }

                        var itemsDoMenu = itemsAtribuidos.Where(ia => ia.MenuId == menuEncontrado.Id).ToList();

                        List<Item> itemsVB = new List<Item>();

                        foreach (var item in itemsDoMenu)
                        {
                            Item itemToInsert = items.Find(items => items.Id == item.IdItem);
                            itemsVB.Add(itemToInsert);
                        }

                        ViewBag.ItemAtribuido = itemsVB;

                        return View(menuEncontrado);


                    }
                }

                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Método que mostra a view do edit
        /// </summary>
        /// <param name="id">id do meu a editar</param>
        /// <returns>View do edit</returns>
        public async Task<IActionResult> EditAsync(int? id)
        {
            List<Menu> menus = await httpClient.GetFromJsonAsync<List<Menu>>("/api/Menu");
            List<Item> items = await httpClient.GetFromJsonAsync<List<Item>>("/api/Items");
            List<ItemAtribuido> itemsAtribuidos = await httpClient.GetFromJsonAsync<List<ItemAtribuido>>("/api/ItemAtribuido");
            if (id == null)
            {
                return BadRequest();
            }

            if (id.Equals(""))
            {
                return NotFound();
            }
            Menu menuEncontrado = null;

            for (int i = 0; i < menus.Count; i++)
            {
                if (menus[i].Id == id)
                {
                    menuEncontrado = menus[i];
                    break; // Se o menu for encontrado, podemos sair do loop.
                }
            }
            menuEncontrado.Items = items.ToList();
            ViewBag.Menu = menuEncontrado;
            ViewBag.ItemAtribuido = itemsAtribuidos.Where(ia => ia.MenuId == menuEncontrado.Id).ToList();
            if (menuEncontrado == null)
            {
                return NotFound();
            }
            return View(menuEncontrado);
        }


        /// <summary>
        /// Método que envia um menu e respetivos items à WebAPI para estes serem editados
        /// </summary>
        /// <param name="menu">Menu a ser editado</param>
        /// <param name="itemsSelecionados">Items do menu a ser editado</param>
        /// <returns>view dos menus</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Menu menu, int[] itemsSelecionados)
        {
			if (itemsSelecionados == null || itemsSelecionados.Length == 0)
			{
				return RedirectToAction("Index");
			}
			if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
            {
                var cookie = JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);

                // Obtenha o token usando o método adequado (não está claro no código fornecido)
                var tokenResponse = await httpClient.PostAsJsonAsync("api/ContaUtilizador/tokenLogged", cookie);

                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

                    // Configure o token nos cabeçalhos de autorização
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenContent);

                    List<Item> items = await httpClient.GetFromJsonAsync<List<Item>>("/api/Stock");
                    List<ItemAtribuido> itemsAtribuidos = await httpClient.GetFromJsonAsync<List<ItemAtribuido>>("/api/ItemAtribuido");

                    List<Item> todosItems = items.ToList();
                    var itemsToRemove = itemsAtribuidos
                    .Where(ing => ing.MenuId == menu.Id)
                    .ToList();

                    foreach (var itemAtribuido in itemsToRemove)
                    {
                        HttpResponseMessage response1 = await httpClient.DeleteAsync($"api/ItemAtribuido/{itemAtribuido.Id}");

                    }

                    string fileToDelete = Path.Combine("wwwroot", "Images", "ItemImages", menu.ImagePath);

                    if (System.IO.File.Exists(fileToDelete) && menu.Image != null)
                    {
                        System.IO.File.Delete(fileToDelete);
                        Console.WriteLine($"Arquivo {menu.ImagePath} excluído com sucesso!");
                    }

                    if (menu.Image != null)
                    {
                        menu.ImagePath = SaveImage(menu.Image);
                    }
                    menu.Image = null;

                    HttpResponseMessage response = httpClient.PutAsJsonAsync($"api/Menu/{menu.Id}", menu).Result;

                    foreach (var ing in itemsSelecionados)
                    {
                        ItemAtribuido itemAtribuido = new ItemAtribuido();
                        itemAtribuido.MenuName = menu.Name;
                        itemAtribuido.IdItem = ing;
                        itemAtribuido.MenuId = menu.Id;
                        itemsAtribuidos.Add(itemAtribuido);
                        HttpResponseMessage response2 = httpClient.PostAsJsonAsync($"api/ItemAtribuido", itemAtribuido).Result;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        // Lógica para lidar com falhas na comunicação com a API
                        // Pode ser útil logar ou exibir mensagens de erro
                        var statusCode = response.StatusCode;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Código de status: {statusCode}");
                        var errorContent = response.Content.ReadAsStringAsync().Result;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Detalhes: {errorContent}");
                        return View(menu);
                    }
                }
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// View que retorna a View de delete de um menu
        /// </summary>
        /// <param name="id">Id do menu a ser eliminado</param>
        /// <returns>View do menu</returns>
        public async Task<IActionResult> DeleteAsync(int id)
        {
            List<Menu> menus = await httpClient.GetFromJsonAsync<List<Menu>>("/api/Menu");
            Menu menuEncontrado = null;

            for (int i = 0; i < menus.Count; i++)
            {
                if (menus[i].Id == id)
                {
                    menuEncontrado = menus[i];
                    break; // Se o menu for encontrado, podemos sair do loop.
                }
            }
            if (menuEncontrado == null)
                return NotFound();
            ViewBag.Menu = menuEncontrado;

            return View(menuEncontrado);
        }

        /// <summary>
        /// Métodos que envia o id de um menu para a WebAPI de forma a que este seja eliminado.
        /// </summary>
        /// <param name="id">Id do menu a ser eliminado</param>
        /// <returns>View dos menus</returns>
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
            {
                var cookie = JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);

                // Obtenha o token usando o método adequado (não está claro no código fornecido)
                var tokenResponse = await httpClient.PostAsJsonAsync("api/ContaUtilizador/tokenLogged", cookie);

                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

                    // Configure o token nos cabeçalhos de autorização
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenContent);

                    List<Menu> menus = await httpClient.GetFromJsonAsync<List<Menu>>("/api/Menu");
                    List<ItemAtribuido> itemsAtribuidos = await httpClient.GetFromJsonAsync<List<ItemAtribuido>>("/api/ItemAtribuido");


                    Menu menuEncontrado = null;

                    for (int i = 0; i < menus.Count; i++)
                    {
                        if (menus[i].Id == id)
                        {
                            menuEncontrado = menus[i];
                            break; // Se o menu for encontrado, podemos sair do loop.
                        }
                    }
                    var menusToRemove = itemsAtribuidos
                   .Where(ing => ing.MenuId == menuEncontrado.Id)
                   .ToList();

                    string fileToDelete = Path.Combine("wwwroot", "Images", "ItemImages", menuEncontrado.ImagePath);

                    if (System.IO.File.Exists(fileToDelete))
                    {
                        System.IO.File.Delete(fileToDelete);
                        Console.WriteLine($"Arquivo {menuEncontrado.ImagePath} excluído com sucesso!");
                    }

                    foreach (var itemAtribuido in menusToRemove)
                    {
                        HttpResponseMessage response1 = await httpClient.DeleteAsync($"api/ItemAtribuido/{itemAtribuido.Id}");

                    }
                    HttpResponseMessage response = await httpClient.DeleteAsync($"/api/Menu/{menuEncontrado.Id}");

                    if (!response.IsSuccessStatusCode)
                    {
                        // Lógica para lidar com falhas na comunicação com a API
                        // Pode ser útil logar ou exibir mensagens de erro
                        var statusCode = response.StatusCode;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Código de status: {statusCode}");
                        var errorContent = response.Content.ReadAsStringAsync().Result;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Detalhes: {errorContent}");
                        return View(menuEncontrado);
                    }
                }
            }

            return RedirectToAction("Index");
        }

    }
}
