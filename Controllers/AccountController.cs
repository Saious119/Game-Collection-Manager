using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace GameCollectionManager.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ExternalLogin()
        {
            var redirectUrl = new PathString("/signin-discord");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Discord");
        }
        public IActionResult Callback()
        {
            // Handle the callback after successful authentication
            // Redirect to the desired page or perform additional actions
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
