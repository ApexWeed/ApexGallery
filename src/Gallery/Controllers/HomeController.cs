using Gallery.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gallery.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiSettings _apiSettings;
        private readonly InterfaceSettings _interfaceSettings;

        public HomeController(IOptions<ApiSettings> apiSettings, IOptions<InterfaceSettings> interfaceSettings)
        {
            _apiSettings = apiSettings.Value;
            _interfaceSettings = interfaceSettings.Value;
        }

        public IActionResult Index(string lang)
        {
            // Handle missing and invalid languages.
            if (lang == null || !_apiSettings.Languages.ContainsKey(lang))
            {
                var newLang = LanguageManager.SelectLanguage(_apiSettings.Languages, Request.Headers["Accept-Language"], "en");
                return new RedirectResult($"/{newLang}/");
            }
            
            var viewModel = new HomeViewModel
            {
                Strings = _interfaceSettings.Strings[lang],
                Language = lang,
                Languages = _apiSettings.Languages
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
