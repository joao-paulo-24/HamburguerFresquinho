using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using ProjetoLDS.Models;
using System.Diagnostics;
using System.Text.Json;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Método que apresenta o ecrã inicial
        /// </summary>
        /// <returns>View do ecrã inicial</returns>
        public IActionResult Index()
        {
            if (Request.Cookies.TryGetValue("UserCookie", out string cookieExists))
            {
                var cookie = JsonSerializer.Deserialize<ContaUtilizador>(cookieExists);
                bool userCookieExists = cookie != null;
                ViewBag.UserCookieExists = userCookieExists;
            }
            return View();
        }

        /// <summary>
        /// Método que apresenta a privacy
        /// </summary>
        /// <returns>View do privacy</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Método responsável pelos erros
        /// </summary>
        /// <returns>View do erro</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
