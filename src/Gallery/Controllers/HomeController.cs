using Gallery.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gallery.Controllers
{
    public class HomeController : Controller
    {
        private APISettings _APISettings;
        private InterfaceSettings _InterfaceSettings;

        public HomeController(IOptions<APISettings> APISettings, IOptions<InterfaceSettings> InterfaceSettings)
        {
            _APISettings = APISettings.Value;
            _InterfaceSettings = InterfaceSettings.Value;
        }

        public IActionResult Index(string Lang)
        {
            // Handle missing and invalid languages.
            if (Lang == null || !_APISettings.Languages.ContainsKey(Lang))
            {
                var newLang = LanguageManager.SelectLanguage(_APISettings.Languages, Request.Headers["Accept-Language"], "en");
                return new RedirectResult($"/{newLang}/");
            }
            
            var viewModel = new HomeViewModel
            {
                Strings = _InterfaceSettings.Strings[Lang],
                Language = Lang,
                Languages = _APISettings.Languages
            };

            return View(viewModel);
        }

        [Route("/Home/Contact")]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Wow";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
