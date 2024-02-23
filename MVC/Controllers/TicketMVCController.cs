using Microsoft.AspNetCore.Mvc;
using ProjetoLDS.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using WebAPI.Enums;
using WebAPI.Models;

namespace MVC.Controllers
{

    public class TicketMVCController : Controller
    {
        private readonly HttpClient httpClient;
        public TicketMVCController()
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7235")
            };
        }

        /// <summary>
        /// Método que mosta a lista de tickets
        /// </summary>
        /// <returns>View com a lista de tickets</returns>
        public async Task<IActionResult> Index()
        {
            if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
            {
                var cookie = System.Text.Json.JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);
                string userCookieExists = cookie.Role.ToString();
                ViewBag.UserCookieExists = userCookieExists;
                ViewBag.User = cookie;
            }

            List<Ticket> tickets = await httpClient.GetFromJsonAsync<List<Ticket>>("/api/Ticket");
            return View(tickets);
        }

        /// <summary>
        /// Método que retorna a view de criação de um ticket
        /// </summary>
        /// <returns>View de criação de um ticket</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Função usada para criar um ticket na base de dados
        /// </summary>
        /// <param name="ticket">ticket a ser enviado no pedido para a base de dados</param>
        /// <returns>retorna a view da lista de tickets</returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(Ticket ticket)
        {

            if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
            {
                var cookie = JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);
                ContaUtilizador newUser = new ContaUtilizador();
                newUser.Email = cookie.Email;
                newUser.Username = cookie.Username;
                newUser.Password = cookie.Password;
                newUser.Role = cookie.Role;
                newUser.Pontos = cookie.Pontos;

                // Obtenha o token usando o método adequado (não está claro no código fornecido)
                var tokenResponse = await httpClient.PostAsJsonAsync("api/ContaUtilizador/tokenLogged", cookie);

                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenContent = await tokenResponse.Content.ReadAsStringAsync();

                    // Configure o token nos cabeçalhos de autorização
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenContent);

                    ticket.ContaUtilizador = newUser;

                    HttpResponseMessage response = httpClient.PostAsJsonAsync("api/Ticket", ticket).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        // Lógica para lidar com falhas na comunicação com a API
                        var statusCode = response.StatusCode;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Código de status: {statusCode}");
                        var errorContent = response.Content.ReadAsStringAsync().Result;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Detalhes: {errorContent}");
                        return BadRequest(ticket);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return NotFound();
                }
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Método que retorna a view de edição do ticket
        /// </summary>
        /// <param name="id">Id do ticket</param>
        /// <returns>View de edição dot ticket</returns>
        public async Task<IActionResult> EditAsync(int id)
        {
            List<Ticket> tickets = await httpClient.GetFromJsonAsync<List<Ticket>>("/api/Ticket");

            Ticket ticket = null;
            if (id == null)
            {
                return BadRequest();
            }

            if (id == 0)
            {
                return NotFound();
            }

            for (int i = 0; i < tickets.Count; i++)
            {
                if (tickets[i].Id == id)
                {
                    ticket = tickets[i];
                    break;
                }
            }
            if (ticket == null)
            {
                return NotFound();
            }
            ViewBag.Ticket = ticket;

            return View(ticket);
        }

        /// <summary>
        /// Função usada para editar um ticket na base de dados
        /// </summary>
        /// <param name="ticket">ticket a ser editado</param>
        /// <returns>retorna a view da lista de tickets</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Ticket ticket)
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

                    if (!ModelState.IsValid)
                        return View(ticket);
                    HttpResponseMessage response = httpClient.PutAsJsonAsync($"api/Ticket/{ticket.Id}", ticket).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var statusCode = response.StatusCode;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Código de status: {statusCode}");
                        var errorContent = response.Content.ReadAsStringAsync().Result;
                        ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Detalhes: {errorContent}");
                        return View(ticket);
                    }
                }
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Método que retorna a view de delete de um ticket
        /// </summary>
        /// <param name="id">Id do ticket a eliminar</param>
        /// <returns>View de delete</returns>
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (id == null)
                return BadRequest();
            if (id == 0)
                return NotFound();
            List<Ticket> tickets = await httpClient.GetFromJsonAsync<List<Ticket>>("/api/Ticket");
            Ticket ticketEncontrado = null;
            for (int i = 0; i < tickets.Count; i++)
            {
                if (tickets[i].Id == id)
                {
                    ticketEncontrado = tickets[i];
                    break;
                }
            }
            if (ticketEncontrado == null)
                return NotFound();
            ViewBag.Ticket = ticketEncontrado;

            return View(ticketEncontrado);
        }

        /// <summary>
        /// Função usada para eliminar um ticket da base de dados
        /// </summary>
        /// <param name="name">Nome do ticket a ser eliminado</param>
        /// <returns>retorna a view da lista de tickets</returns>
        [HttpPost, ActionName("DeletePost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTicketAsync(int id)
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

                    List<Ticket> tickets = await httpClient.GetFromJsonAsync<List<Ticket>>("/api/Ticket");
                    Ticket ticketEncontrado = null;

                    for (int i = 0; i < tickets.Count; i++)
                    {
                        if (tickets[i].Id == id)
                        {
                            ticketEncontrado = tickets[i];
                            break;
                        }
                    }
                    HttpResponseMessage response = await httpClient.DeleteAsync($"/api/Ticket/{ticketEncontrado.Id}");
                    if (response.IsSuccessStatusCode)
                    {
                        // O item foi excluído com sucesso.
                        Console.WriteLine("Ticket excluído com sucesso!");
                    }
                    else
                    {
                        Console.WriteLine($"Ocorreu um erro: {response.StatusCode}");
                    }

                }
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Método que permite alterar o estado do ticket
        /// </summary>
        /// <param name="ticketId">Id do ticket</param>
        /// <param name="NovoEstado">Novo estado do mesmo</param>
        /// <returns>Retorna a view de tickets</returns>
        [HttpPost]
        public async Task<IActionResult> AlterarEstado(int ticketId, string NovoEstado)
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

                    List<Ticket> tickets = await httpClient.GetFromJsonAsync<List<Ticket>>("/api/Ticket");
                    List<ContaUtilizador> contas = await httpClient.GetFromJsonAsync<List<ContaUtilizador>>("/api/ContaUtilizador");

                    Ticket ticketEncontrado = tickets.FirstOrDefault(p => p.Id == ticketId);
                    ContaUtilizador contaEncontrada = contas.FirstOrDefault(t => t.Email == ticketEncontrado.ContaUtilizador.Email);
                    ContaUtilizador contaUtilizadorEncontrada = contas.FirstOrDefault(c => c.Id == contaEncontrada.Id);


                    if (ticketEncontrado != null)
                    {
                        if (NovoEstado.Equals("Aberto"))
                        {
                            ticketEncontrado.Status = (EnumStatusTicket)0;
                        }
                        else if (NovoEstado.Equals("Fechado"))
                        {
                            ticketEncontrado.Status = (EnumStatusTicket)1;
                        }
                        HttpResponseMessage response = httpClient.PutAsJsonAsync($"api/Ticket/{ticketEncontrado.Id}", ticketEncontrado).Result;
                        if (!response.IsSuccessStatusCode)
                        {
                            // Lógica para lidar com falhas na comunicação com a API
                            var statusCode = response.StatusCode;
                            ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Código de status: {statusCode}");
                            var errorContent = response.Content.ReadAsStringAsync().Result;
                            ModelState.AddModelError(string.Empty, $"Erro na comunicação com a API. Detalhes: {errorContent}");
                            return BadRequest(ticketEncontrado);
                        }
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return NotFound();
                    }

                }
            }

            return RedirectToAction("Index");
        }


    }
}
