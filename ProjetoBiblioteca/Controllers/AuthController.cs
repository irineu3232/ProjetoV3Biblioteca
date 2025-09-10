using Microsoft.AspNetCore.Mvc;

namespace ProjetoBiblioteca.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
