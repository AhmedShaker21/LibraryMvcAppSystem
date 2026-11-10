using LibraryMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LibraryMvcApp.Controllers
{
    public class ErrorController : Controller
    {
        [AcceptVerbs("GET", "POST")]
        public IActionResult Index(string message, string? returnUrl = null)
        {
            var model = new ErrorViewModel
            {
                Message = message,
                ReturnUrl = returnUrl ?? Url.Action("Index", "Folder")
            };

            return View(model);
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        [Route("Error/HttpStatus")]
        public IActionResult HttpStatus(int code)
        {
            ViewBag.Code = code;

            var model = new ErrorViewModel
            {
                Message = $"HTTP {code}",
                ReturnUrl = Url.Action("Index", "Folder")
            };

            return code switch
            {
                404 => View("NotFound"),
                403 => View("AccessDenied"),
                _ => View("Error", model) // pass the model here
            };
        }
    }
}
