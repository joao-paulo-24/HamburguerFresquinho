using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Common;
using NuGet.DependencyResolver;
using PayPal.Api;
using ProjetoLDS.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using WebAPI.Data;
using WebAPI.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MVC.Controllers
{
    public class StockMVCController : Controller
    {
        private readonly HttpClient httpClient;
        public StockMVCController()
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7235")
            };
        }

        /// <summary>
        /// Função para mostrar os ingredientes na web
        /// </summary>
        /// <returns>view com a lista dos items</returns>
        public async Task<IActionResult> IndexAsync()
        {

            List<Ingrediente> ingredientes = await httpClient.GetFromJsonAsync<List<Ingrediente>>("/api/Stock");
            if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
            {
                var cookie = JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);
                string userCookieExists = cookie.Role.ToString();
                ViewBag.UserCookieExists = userCookieExists;
            }

            return View(ingredientes);
        }

        /// <summary>
        /// Método que devolve a view de create de um ingrediente
        /// </summary>
        /// <returns>View de create de um ingrediente</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Função usada para criar um ingrediente na base de dados
        /// </summary>
        /// <param name="ingrediente">ingrediente a ser enviado no pedido para a base de dados</param>
        /// <returns>retorna a view da lista de ingredientes</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(Ingrediente ingrediente)
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

                    // Salvar o arquivo no sistema de arquivos do servidor
                    if (ingrediente.Image != null)
                    {
                        ingrediente.ImagePath = SaveImage(ingrediente.Image);
                    }

                    ingrediente.Image = null;

                    // Continue com a chamada para o endpoint protegido
                    HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/Stock", ingrediente);

                    if (!response.IsSuccessStatusCode)
                    {
                        // Lógica para lidar com falhas na comunicação com a API
                        // Pode ser útil logar ou exibir mensagens de erro
                        var statusCode = response.StatusCode;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Código de status: {statusCode}");
                        var errorContent = response.Content.ReadAsStringAsync().Result;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Detalhes: {errorContent}");
                        return View(ingrediente);
                    }
                }
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Função usada para guardar a imagem do Ingrediente
        /// </summary>
        /// <param name="imageFile">Imagem a ser guardada</param>
        /// <returns>Path da imagem</returns>
        private string SaveImage(IFormFile imageFile)
        {
            // Caminho da pasta onde as imagens serão armazenadas (por exemplo, na raiz do seu projeto)
            string uploadsFolder = Path.Combine("wwwroot", "Images", "IngredientImages");

            // Garanta que a pasta exista, se não existir, crie-a
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Gere um nome único para o arquivo
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;

            // Combine o caminho da pasta com o nome do arquivo para obter o caminho completo
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Salvar o arquivo no sistema de arquivos
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(fileStream);
            }

            // Retorne o caminho relativo (ou completo) do arquivo
            return uniqueFileName;
        }


        /// <summary>
        /// Método que devolve a view de edição de ingredientes
        /// </summary>
        /// <param name="id">Id do ingrediente a ser editado</param>
        /// <returns>View de edição do ingrediente</returns>
        public async Task<IActionResult> EditAsync(int id)
        {
            List<Ingrediente> ingredientes = await httpClient.GetFromJsonAsync<List<Ingrediente>>("/api/Stock");

            Ingrediente ingrediente = null;
            if (id == null)
            {
                return BadRequest();
            }

            if (id == 0)
            {
                return NotFound();
            }

            for (int i = 0; i < ingredientes.Count; i++)
            {
                if (ingredientes[i].Id == id)
                {
                    ingrediente = ingredientes[i];
                    break;
                }
            }
            if (ingrediente == null)
            {
                return NotFound();
            }
            ViewBag.Ingrediente = ingrediente;

            return View(ingrediente);
        }


        /// <summary>
        /// Função usada para editar um ingrediente na base de dados
        /// </summary>
        /// <param name="ingrediente">ingrediente a ser editado</param>
        /// <returns>retorna a view da lista de ingredientes</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Ingrediente ingrediente)
        {
            if (!ModelState.IsValid)
                return View(ingrediente);

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



                    string fileToDelete = Path.Combine("wwwroot", "Images", "IngredientImages", ingrediente.ImagePath);

                    if (System.IO.File.Exists(fileToDelete) && ingrediente.Image != null)
                    {
                        System.IO.File.Delete(fileToDelete);
                        Console.WriteLine($"Arquivo {ingrediente.ImagePath} excluído com sucesso!");
                    }

                    if (ingrediente.Image != null)
                    {
                        // Salvar o arquivo no sistema de arquivos do servidor
                        ingrediente.ImagePath = SaveImage(ingrediente.Image);
                    }
                    ingrediente.Image = null;

                    HttpResponseMessage response = httpClient.PutAsJsonAsync($"api/Stock/{ingrediente.Id}", ingrediente).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        // Lógica para lidar com falhas na comunicação com a API
                        // Pode ser útil logar ou exibir mensagens de erro
                        var statusCode = response.StatusCode;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Código de status: {statusCode}");
                        var errorContent = response.Content.ReadAsStringAsync().Result;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Detalhes: {errorContent}");
                        return View(ingrediente);
                    }
                }
            }

            return RedirectToAction("Index");
        }


        /// <summary>
        /// Método que retorna a view de delete de um ingrediente
        /// </summary>
        /// <param name="id">Id do ingrediente a ser eliminado</param>
        /// <returns>View do ingrediente a eliminar</returns>
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (id == null)
                return BadRequest();
            if (id == 0)
                return NotFound();
            List<Ingrediente> ingredientes = await httpClient.GetFromJsonAsync<List<Ingrediente>>("/api/Stock");
            Ingrediente ingredienteEncontrado = null;
            for (int i = 0; i < ingredientes.Count; i++)
            {
                if (ingredientes[i].Id == id)
                {
                    ingredienteEncontrado = ingredientes[i];
                    break;
                }
            }
            if (ingredienteEncontrado == null)
                return NotFound();
            ViewBag.Ingrediente = ingredienteEncontrado;

            return View(ingredienteEncontrado);
        }

        /// <summary>
        /// Função usada para eliminar um ingrediente da base de dados
        /// </summary>
        /// <param name="name">Nome do ingrediente a ser eliminado</param>
        /// <returns>retorna a view da lista de ingredientes</returns>
        [HttpPost, ActionName("DeletePost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteIngredienteAsync(int id)
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

                    List<Ingrediente> ingredientes = await httpClient.GetFromJsonAsync<List<Ingrediente>>("/api/Stock");
                    List<IngredienteAtribuido> ingredienteAtribuidos = await httpClient.GetFromJsonAsync<List<IngredienteAtribuido>>("api/IngredienteAtribuido");
                    Ingrediente ingredienteEncontrado = null;

                    for (int i = 0; i < ingredientes.Count; i++)
                    {
                        if (ingredientes[i].Id == id)
                        {
                            ingredienteEncontrado = ingredientes[i];
                            break;
                        }
                    }

                    bool ingredienteAtribuidoEncontrado = ingredienteAtribuidos.Any(item => item.IdIngrediente == ingredienteEncontrado.Id);

                    if (!ingredienteAtribuidoEncontrado)
                    {

                        string fileToDelete = Path.Combine("wwwroot", "Images", "IngredientImages", ingredienteEncontrado.ImagePath);

                        if (System.IO.File.Exists(fileToDelete))
                        {
                            System.IO.File.Delete(fileToDelete);
                            Console.WriteLine($"Arquivo {ingredienteEncontrado.ImagePath} excluído com sucesso!");
                        }

                        HttpResponseMessage response = await httpClient.DeleteAsync($"/api/Stock/{ingredienteEncontrado.Id}");

                        if (!response.IsSuccessStatusCode)
                        {
                            // Lógica para lidar com falhas na comunicação com a API
                            // Pode ser útil logar ou exibir mensagens de erro
                            var statusCode = response.StatusCode;
                            ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Código de status: {statusCode}");
                            var errorContent = response.Content.ReadAsStringAsync().Result;
                            ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Detalhes: {errorContent}");
                            return View(ingredienteEncontrado);
                        }
                    }
                    if (ingredienteAtribuidoEncontrado)
                    {
                        TempData["ErrorMessage"] = "Não é possível excluir este ingrediente porque está atribuído a um item.";
                        return View("Delete", ingredienteEncontrado);
                    }
                }
            }

            return RedirectToAction("Index");
        }

    }
}