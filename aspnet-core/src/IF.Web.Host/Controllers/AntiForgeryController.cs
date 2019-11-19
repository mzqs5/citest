using Microsoft.AspNetCore.Antiforgery;
using IF.Controllers;

namespace IF.Web.Host.Controllers
{
    public class AntiForgeryController : IFControllerBase
    {
        private readonly IAntiforgery _antiforgery;

        public AntiForgeryController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        public void GetToken()
        {
            _antiforgery.SetCookieTokenAndHeader(HttpContext);
        }
    }
}
