using Microsoft.AspNetCore.Mvc;

namespace ChapeauPOS.Controllers
{
    public class KitchenBarController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Kitchen()
        {
            return View();
        }
        public IActionResult Bar()
        {
            return View();
        }
    }
}
