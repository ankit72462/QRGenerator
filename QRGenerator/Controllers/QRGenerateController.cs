using Microsoft.AspNetCore.Mvc;

namespace QRGenerator.Controllers
{
    public class QRGenerateController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
