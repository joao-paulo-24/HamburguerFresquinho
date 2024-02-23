using Microsoft.AspNetCore.Mvc;
using ProjetoLDS.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using WebAPI.Models;

namespace MVC.Controllers
{
    public class AuthMVCController : Controller
    {

        private readonly HttpClient httpClient;

        public AuthMVCController()
        {
            // Configurar o HttpClient para fazer chamadas à sua Web API
            httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7235/") // Defina sua URL da Web API aqui
            };
        }

        /// <summary>
        /// Método que retorna a view de login
        /// </summary>
        /// <returns>View de login</returns>
        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Método que permite fazer login na aplicação, crinado a cookie
        /// </summary>
        /// <param name="user">Conta de Utilizador</param>
        /// <returns>view para o menu principal, ou caso o login não seja sucedido, view de login novamente</returns>
        [HttpPost]
        public async Task<IActionResult> Login(ContaUtilizador user)
        {
            try
            {
                // Aqui você pode construir o objeto com os dados para enviar à API
                var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

                // Faça a requisição HTTP para o endpoint da sua API que gera o token
                var response = await httpClient.PostAsync("api/ContaUtilizador/token", content);

                var userResponse = await httpClient.GetAsync("api/ContaUtilizador");
                if (response.IsSuccessStatusCode)
                {
                    var apiUsers = await userResponse.Content.ReadFromJsonAsync<List<ContaUtilizador>>();
                    var userFound = apiUsers.FirstOrDefault(u => u.Email == user.Email);

                    // Leitura do token retornado pela WebAPI
                    var token = await response.Content.ReadAsStringAsync();
                    ContaUtilizador userToCookie = new ContaUtilizador();
                    if (userFound != null)
                    {
                        userToCookie.Email = userFound.Email;
                        userToCookie.Id = userFound.Id;
                        userToCookie.Role = userFound.Role;
                        userToCookie.Username = userFound.Username;
                        userToCookie.Pontos = userFound.Pontos;
                        userToCookie.Password = userFound.Password;
                    }
                    var userCookieValue = JsonSerializer.Serialize(userToCookie);
                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddHours(1),
                        HttpOnly = true
                    };
                    // Aqui você pode armazenar o token em um cookie, session, local storage, ou realizar outra ação necessária para a autenticação no seu projeto MVC
                    Response.Cookies.Append("UserCookie", userCookieValue, cookieOptions);

                    return RedirectToAction("Index", "Home"); // Redirecionamento após autenticação bem-sucedida
                }
                else
                {
                    // Caso a requisição não seja bem-sucedida (ex: credenciais inválidas), trate aqui
                    ModelState.AddModelError(string.Empty, "Credenciais inválidas");
                    return View(user); // Ou outra ação adequada para lidar com credenciais inválidas
                }
            }
            catch (Exception ex)
            {
                // Trate outras exceções caso ocorram durante o processo de autenticação
                ModelState.AddModelError(string.Empty, "Erro durante a autenticação: " + ex.Message);
                return View(user); // Ou outra ação adequada para lidar com erros
            }
        }

        /// <summary>
        /// Método que retorna a view de registo
        /// </summary>
        /// <returns>View de registo</returns>
        public ActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Método que permite o registo de uma conta de utilizadro
        /// </summary>
        /// <param name="newUser">Nova conta de utilizador</param>
        /// <returns>View de login ou, caso o registo não seja concluido com sucesso, view de registo novamente</returns>
        [HttpPost]
        public async Task<IActionResult> Register(ContaUtilizador newUser)
        {
            try
            {
                // Aqui você pode construir o objeto com os dados para enviar à API
                var content = new StringContent(JsonSerializer.Serialize(newUser), Encoding.UTF8, "application/json");

                // Faça a requisição HTTP para o endpoint da sua API que faz o registro
                var response = await httpClient.PostAsync("api/ContaUtilizador/register", content);

                if (response.IsSuccessStatusCode)
                {
                    // Registro bem-sucedido
                    return RedirectToAction("Login"); // Ou outra ação adequada após o registro bem-sucedido
                }
                else
                {
                    // Trate o caso em que o registro não é bem-sucedido
                    ModelState.AddModelError(string.Empty, "Erro durante o registro");
                    return View(newUser); // Ou outra ação adequada para lidar com erros de registro
                }
            }
            catch (Exception ex)
            {
                // Trate outras exceções caso ocorram durante o processo de registro
                ModelState.AddModelError(string.Empty, "Erro durante o registro: " + ex.Message);
                return View(newUser); // Ou outra ação adequada para lidar com erros
            }
        }

        /// <summary>
        /// Função que apaga a cookie fazendo o logout do utilizador
        /// </summary>
        /// <returns>view de login</returns>
        public IActionResult Logout()
        {
            // Limpar o cookie "UserCookie"
            Response.Cookies.Delete("UserCookie");

            // Redirecionar para a página de login ou outra página após o logout
            return RedirectToAction("Login", "AuthMVC");
        }

        /// <summary>
        /// Método que mostra a view que permite editar um perfil de utilizador
        /// </summary>
        /// <returns>view de edição de perfil, caso o login esteja efetuado</returns>
        public async Task<IActionResult> Edit()
        {
            if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
            {
                var cookie = JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);

                ViewBag.User = cookie;

                return View(cookie);
            }

            return View();
        }

        /// <summary>
        /// Método que permite a edição da conta do utilizador
        /// </summary>
        /// <param name="user">Conta de utilizador a editar</param>
        /// <returns>Menu principal caso a edição seja concluida, View de edição case a mesma não tenha sido feita corretamente</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContaUtilizador user)
        {

            if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
            {
                var cookie = JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);
                cookie.Username = user.Username;
                cookie.Email = user.Email;
                cookie.Password = user.Password;
                HttpResponseMessage response = httpClient.PutAsJsonAsync($"/api/ContaUtilizador/{cookie.Id}/Update", cookie).Result;

                if (!response.IsSuccessStatusCode)
                {
                    // Lógica para lidar com falhas na comunicação com a API
                    // Pode ser útil logar ou exibir mensagens de erro
                    var statusCode = response.StatusCode;
                    ModelState.AddModelError(string.Empty, $"{statusCode}");
                    var errorContent = response.Content.ReadAsStringAsync().Result;
                    ModelState.AddModelError(string.Empty, $"{errorContent}");
                    return View(user);
                }

                // Salva ou atualiza o cookie com os novos dados
                var serializedCookie = JsonSerializer.Serialize(cookie);
                Response.Cookies.Append("UserCookie", serializedCookie);
                return RedirectToAction("Index", "Home");
            }
            return View(user); // Retorna a View com erros de validação, se houverem

        }
    }
}
