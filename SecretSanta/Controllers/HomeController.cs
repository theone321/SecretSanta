using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;

namespace SecretSanta.Controllers {
    public class HomeController : Controller {
        [HttpGet]
        public IActionResult Index() {
            return RedirectToAction("SignIn", "User");
        }

        [HttpGet]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
