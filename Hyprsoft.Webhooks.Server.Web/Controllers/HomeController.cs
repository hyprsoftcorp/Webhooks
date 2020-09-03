using Hyprsoft.Webhooks.Core;
using Hyprsoft.Webhooks.Core.Management;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Server.Web.Controllers
{
    public class HomeController : Controller
    {
        #region Fields

        private readonly IWebhooksManager _webhooksManager;
        private readonly WebhooksHealthWorkerOptions _workerOptions;

        #endregion

        #region Constructors

        public HomeController(IWebhooksManager webhooksManger, IOptions<WebhooksHealthWorkerOptions> workerOptions)
        {
            _webhooksManager = webhooksManger;
            _workerOptions = workerOptions.Value;
        }

        #endregion

        [HttpGet]
        public IActionResult Index() => View(WebhooksGlobalConfiguration.GetBuildAndVersionInfo(GetType().Assembly));

        [HttpGet("health")]
        public async Task<IActionResult> Health()
        {
            var summary = await _webhooksManager.GetHealthSummaryAsync(_workerOptions.PublishHealthEventInterval);
            summary.PublishInterval = _workerOptions.PublishHealthEventInterval;
            summary.ServerStartDateUtc = WebhooksHealthWorker.ServerStartDateUtc;

            return View(summary);
        }

        [HttpGet("subscriptions")]
        public IActionResult Subscriptions() => View(_webhooksManager.Subscriptions);

        [HttpGet("docs")]
        public IActionResult Docs() => View(WebhooksGlobalConfiguration.GetBuildAndVersionInfo(GetType().Assembly));
    }
}
