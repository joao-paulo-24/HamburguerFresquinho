using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProjetoLDS.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.Models;

namespace MVC.Controllers
{
    public class ItemsMVCController : Controller
    {
        private readonly HttpClient httpClient;
        public ItemsMVCController()
        {
            // Configurar o HttpClient para fazer chamadas à sua Web API
            httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7235") // Defina sua URL da Web API aqui
            };
        }
        /// <summary>
        /// Função para mostrar os items na web
        /// </summary>
        /// <returns>view com a lista dos items</returns>
        public async Task<IActionResult> Index()
        {
            if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
            {
                var cookie = System.Text.Json.JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);
                string userCookieExists = cookie.Role.ToString();
                ViewBag.UserCookieExists = userCookieExists;
            }


            List<Item> items = await httpClient.GetFromJsonAsync<List<Item>>("/api/Items");

            return View(items);
        }

        /// <summary>
        /// Método que retorna a view do create
        /// </summary>
        /// <returns>View do create</returns>
        public async Task<IActionResult> CreateAsync()
        {
            List<Ingrediente> ingredientes = await httpClient.GetFromJsonAsync<List<Ingrediente>>("/api/Stock");

            Item item = new Item();
            item.Ingredientes = ingredientes.ToList();
            ViewBag.Item = item;
            return View();
        }

        /// <summary>
        /// Função que envia um item do utilizador para o WebApi
        /// </summary>
        /// <param name="itemViewModel">Item do Utilizador</param>
        /// <returns>Lista de items, caso o novo seja adicionado, ou o formulário caso algum problema tenha surgido</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> CreateAsync(Item item, int[] ingredientesSelecionados)
        {

            if (ingredientesSelecionados == null || ingredientesSelecionados.Length == 0)
            {
                return RedirectToAction("Index");
            }
            if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
            {
                var cookie = System.Text.Json.JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);

                // Obtenha o token usando o método adequado (não está claro no código fornecido)
                var tokenResponse = await httpClient.PostAsJsonAsync("api/ContaUtilizador/tokenLogged", cookie);
                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenContent);
                    // Configure o token nos cabeçalhos de autorização
                    if (item.Image != null)
                    {
                        // Salvar o arquivo no sistema de arquivos do servidor
                        item.ImagePath = SaveImage(item.Image);
                    }
                    item.Image = null;

                    // Serializar o objeto itemViewModel para JSON e enviá-lo para a API
                    HttpResponseMessage response = httpClient.PostAsJsonAsync("api/items", item).Result;
                    Item createdItem = await response.Content.ReadFromJsonAsync<Item>();


                    foreach (var ing in ingredientesSelecionados)
                    {
                        HttpResponseMessage response2 = httpClient.GetAsync($"api/Stock/{ing}").Result;

                        if (response.IsSuccessStatusCode)
                        {
                            // Deserializar o Ingrediente
                            string content = await response2.Content.ReadAsStringAsync();
                            Ingrediente ingrediente = JsonConvert.DeserializeObject<Ingrediente>(content);

                            // Comparar o TypeComida com null
                            if (ingrediente.TypeComida == item.TypeComida)
                            {
                                IngredienteAtribuido ingredienteAtribuido = new IngredienteAtribuido();
                                ingredienteAtribuido.ItemName = createdItem.Name;
                                ingredienteAtribuido.IdIngrediente = ing;
                                ingredienteAtribuido.ItemId = createdItem.Id;

                                HttpResponseMessage response1 = httpClient.PostAsJsonAsync("api/IngredienteAtribuido", ingredienteAtribuido).Result;

                                // Aqui você pode adicionar mais lógica, se necessário
                            }
                        }
                        else
                        {
                            var statusCode = response2.StatusCode;
                            ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Código de status: {statusCode}");
                            var errorContent = response2.Content.ReadAsStringAsync().Result;
                            ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Detalhes: {errorContent}");
                            return View(item);
                        }
                    }

                    // Verificar se a solicitação foi bem-sucedida
                    if (response.IsSuccessStatusCode)
                    {
                        // Redirecionar para a página desejada após o processamento
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // Lógica para lidar com falhas na comunicação com a API
                        // Pode ser útil logar ou exibir mensagens de erro
                        var statusCode = response.StatusCode;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Código de status: {statusCode}");
                        var errorContent = response.Content.ReadAsStringAsync().Result;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Detalhes: {errorContent}");
                        return View(item);
                    }
                }
            }
            return RedirectToAction("Index");

        }

        /// <summary>
        /// Função usada para guardar a imagem do Item
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
        /// Método que retorna a view do edit
        /// </summary>
        /// <param name="id">Id do item a editar</param>
        /// <returns>view do edit</returns>
        public async Task<IActionResult> EditAsync(int? id)
        {
            List<Item> items = await httpClient.GetFromJsonAsync<List<Item>>("/api/Items");
            List<Ingrediente> ingredientes = await httpClient.GetFromJsonAsync<List<Ingrediente>>("/api/Stock");
            List<IngredienteAtribuido> ingredientesAtribuidos = await httpClient.GetFromJsonAsync<List<IngredienteAtribuido>>("/api/IngredienteAtribuido");
            if (id == null)
            {
                return BadRequest();
            }

			if (id.Equals(""))
            {
                return NotFound();
            }
            Item itemEncontrado = null;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Id == id)
                {
                    itemEncontrado = items[i];
                    break; // Se o item for encontrado, podemos sair do loop.
                }
            }

            itemEncontrado.Ingredientes = ingredientes.ToList();
            ViewBag.Item = itemEncontrado;
            ViewBag.IngredientesAtribuido = ingredientesAtribuidos.Where(ia => ia.ItemId == itemEncontrado.Id).ToList();

            if (itemEncontrado == null)
            {
                return NotFound();
            }
            return View(itemEncontrado);
        }

        /// <summary>
        /// Método que envia um item e seus ingredientes para a WebAPI para estes serem editados
        /// </summary>
        /// <param name="item">item a editar</param>
        /// <param name="ingredientesSelecionados">ingredientes selecionados do mesmo ingrediente</param>
        /// <returns>View do menu de items</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Item item, int[] ingredientesSelecionados)
        {
			if (ingredientesSelecionados == null || ingredientesSelecionados.Length == 0)
			{
				return RedirectToAction("Index");
			}
			
            if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
            {
                var cookie = System.Text.Json.JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);

                // Obtenha o token usando o método adequado (não está claro no código fornecido)
                var tokenResponse = await httpClient.PostAsJsonAsync("api/ContaUtilizador/tokenLogged", cookie);

                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenContent);

                    List<Ingrediente> ingredientes = await httpClient.GetFromJsonAsync<List<Ingrediente>>("/api/Stock");
                    List<IngredienteAtribuido> ingredientesAtribuidos = await httpClient.GetFromJsonAsync<List<IngredienteAtribuido>>("/api/IngredienteAtribuido");

                    List<Ingrediente> todosIngredientes = ingredientes.ToList();
                    var itemsToRemove = ingredientesAtribuidos
                    .Where(ing => ing.ItemId == item.Id)
                    .ToList();

                    foreach (var ingredienteAtribuido in itemsToRemove)
                    {
                        HttpResponseMessage response1 = await httpClient.DeleteAsync($"api/IngredienteAtribuido/{ingredienteAtribuido.Id}");

                    }

                    string fileToDelete = Path.Combine("wwwroot", "Images", "ItemImages", item.ImagePath);

                    if (System.IO.File.Exists(fileToDelete) && item.Image != null)
                    {
                        System.IO.File.Delete(fileToDelete);
                        Console.WriteLine($"Arquivo {item.ImagePath} excluído com sucesso!");
                    }

                    if (item.Image != null)
                    {
                        // Salvar o arquivo no sistema de arquivos do servidor
                        item.ImagePath = SaveImage(item.Image);
                    }
                    item.Image = null;

                    // Serializar o objeto itemViewModel para JSON e enviá-lo para a API
                    HttpResponseMessage response = httpClient.PutAsJsonAsync($"api/items/{item.Id}", item).Result;

                    foreach (var ing in ingredientesSelecionados)
                    {
                        IngredienteAtribuido ingredienteAtribuido = new IngredienteAtribuido();
                        ingredienteAtribuido.ItemName = item.Name;
                        ingredienteAtribuido.IdIngrediente = ing;
                        ingredienteAtribuido.ItemId = item.Id;
                        ingredientesAtribuidos.Add(ingredienteAtribuido);
                        HttpResponseMessage response2 = httpClient.PostAsJsonAsync($"api/IngredienteAtribuido", ingredienteAtribuido).Result;
                    }

                    // Verificar se a solicitação foi bem-sucedida
                    if (response.IsSuccessStatusCode)
                    {
                        // Redirecionar para a página desejada após o processamento
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // Lógica para lidar com falhas na comunicação com a API
                        // Pode ser útil logar ou exibir mensagens de erro
                        var statusCode = response.StatusCode;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Código de status: {statusCode}");
                        var errorContent = response.Content.ReadAsStringAsync().Result;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Detalhes: {errorContent}");
                        return View(item);
                    }
                }
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Método que retorna a view do delete
        /// </summary>
        /// <param name="id">Id do pedido a eliminar</param>
        /// <returns>View do delete</returns>
        public async Task<IActionResult> DeleteAsync(int id)
        {
            List<Item> items = await httpClient.GetFromJsonAsync<List<Item>>("/api/Items");
            Item itemEncontrado = null;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Id == id)
                {
                    itemEncontrado = items[i];
                    break; // Se o item for encontrado, podemos sair do loop.
                }
            }
            if (itemEncontrado == null)
                return NotFound();
            ViewBag.Item = itemEncontrado;

            return View(itemEncontrado);
        }

        /// <summary>
        /// Método que envia o Id do item para a WebAPI para que esse seja eliminado
        /// </summary>
        /// <param name="id">Id do item a ser eliminado</param>
        /// <returns>View do menu de items</returns>
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
            {
                var cookie = System.Text.Json.JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);

                // Obtenha o token usando o método adequado (não está claro no código fornecido)
                var tokenResponse = await httpClient.PostAsJsonAsync("api/ContaUtilizador/tokenLogged", cookie);

                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenContent);

                    List<Item> items = await httpClient.GetFromJsonAsync<List<Item>>("/api/Items");
                    List<ItemAtribuido> itemsAtribuidos = await httpClient.GetFromJsonAsync<List<ItemAtribuido>>("api/ItemAtribuido");
                    List<IngredienteAtribuido> ingredientesAtribuidos = await httpClient.GetFromJsonAsync<List<IngredienteAtribuido>>("/api/IngredienteAtribuido");

                    try
                    {
                        Item itemEncontrado = null;

                        for (int i = 0; i < items.Count; i++)
                        {
                            if (items[i].Id == id)
                            {
                                itemEncontrado = items[i];
                                break; // Se o item for encontrado, podemos sair do loop.
                            }
                        }

                        bool itemAtribuidoEncontrado = itemsAtribuidos.Any(item => item.IdItem == itemEncontrado.Id);

                        if (!itemAtribuidoEncontrado)
                        {
                            var itemsToRemove = ingredientesAtribuidos
                           .Where(ing => ing.ItemId == itemEncontrado.Id)
                           .ToList();

                            string fileToDelete = Path.Combine("wwwroot", "Images", "ItemImages", itemEncontrado.ImagePath);

                            if (System.IO.File.Exists(fileToDelete))
                            {
                                System.IO.File.Delete(fileToDelete);
                                Console.WriteLine($"Arquivo {itemEncontrado.ImagePath} excluído com sucesso!");
                            }

                            foreach (var ingredienteAtribuido in itemsToRemove)
                            {
                                HttpResponseMessage response1 = await httpClient.DeleteAsync($"api/IngredienteAtribuido/{ingredienteAtribuido.Id}");

                            }
                            HttpResponseMessage response = await httpClient.DeleteAsync($"/api/Items/{itemEncontrado.Id}");
                            if (response.IsSuccessStatusCode)
                            {
                                // O item foi excluído com sucesso.
                                Console.WriteLine("Item excluído com sucesso!");
                            }
                            else
                            {
                                // Se a requisição não for bem-sucedida, você pode lidar com o erro aqui.
                                Console.WriteLine($"Ocorreu um erro: {response.StatusCode}");
                            }
                            // Exclusão bem-sucedida, redireciona para a página principal
                            return RedirectToAction("Index");
                        }

                        if (itemAtribuidoEncontrado)
                        {
                            TempData["ErrorMessage"] = "Não é possível excluir este ingrediente porque está atribuído a um menu.";
                            return View("Delete", itemEncontrado);
                        }

                    }
                    catch (Exception ex)
                    {
                        // Trata outras exceções que possam ocorrer durante o processo de exclusão
                        ModelState.AddModelError(string.Empty, "Erro durante a exclusão do item: " + ex.Message);
                        return RedirectToAction("Delete", new { id }); // Ou outra ação adequada para lidar com erros
                    }
                }
            }
            return RedirectToAction("Index");
        }
    }
}