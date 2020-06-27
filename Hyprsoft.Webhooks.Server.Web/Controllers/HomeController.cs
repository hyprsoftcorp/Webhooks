using Hyprsoft.Webhooks.Core;
using Hyprsoft.Webhooks.Core.Management;
using Microsoft.AspNetCore.Mvc;

namespace Hyprsoft.Webhooks.Server.Web.Controllers
{
    public class HomeController : Controller
    {
        #region Fields

        private readonly IWebhooksManager _webhooksManager;

        #endregion

        #region Constructors

        public HomeController(IWebhooksManager webhooksManger)
        {
            _webhooksManager = webhooksManger;
        }

        #endregion

        [HttpGet]
        public IActionResult Index() => View(WebhooksGlobalConfiguration.GetBuildAndVersionInfo(GetType().Assembly));

        [HttpGet("subscriptions")]
        public IActionResult Subscriptions() => View(_webhooksManager.Subscriptions);

        [HttpGet("docs")]
        public IActionResult Docs() => View(WebhooksGlobalConfiguration.GetBuildAndVersionInfo(GetType().Assembly));
    }
}
