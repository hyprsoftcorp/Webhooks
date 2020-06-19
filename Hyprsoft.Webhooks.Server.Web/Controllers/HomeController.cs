using Microsoft.AspNetCore.Mvc;
using Hyprsoft.Webhooks.Core;
using Hyprsoft.Webhooks.Core.Management;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Subscriptions() => View(await _webhooksManager.GetSubscriptionsAsync());

        [HttpGet("docs")]
        public IActionResult Docs() => View(WebhooksGlobalConfiguration.GetBuildAndVersionInfo(GetType().Assembly));
    }
}
