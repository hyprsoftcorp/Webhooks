using Hyprsoft.Webhooks.Core.Management;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Server.Web.V1.Controllers
{
    /// <summary>
    /// Webhooks Client API.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("[controller]/v{version:apiVersion}/[action]")]
    public class ApiController : ControllerBase
    {
        #region Fields

        private readonly IWebhooksManager _webhooksManager;
        private readonly WebhooksHealthWorkerOptions _workerOptions;
        // TODO: Since we use different serialization settings for the webhooks controller, find a better approach to use the default serialization settings for this controller.
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings { DateFormatString = "yyyy-MM-ddTHH:mm:ssZ", ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() } };

        #endregion

        #region Constructors

        public ApiController(IWebhooksManager webhooksManger, IOptions<WebhooksHealthWorkerOptions> workerOptions)
        {
            _webhooksManager = webhooksManger;
            _workerOptions = workerOptions.Value;
        }

        #endregion

        /// <summary>
        /// Gets the UTC build date and the version of the Webhooks Server API.
        /// </summary>
        /// <returns>The UTC build date and the version of the Webhooks Server API.</returns>
        [HttpGet]
        public IActionResult BuildInfo() => new JsonResult(Core.BuildInfo.FromAssembly(GetType().Assembly), _serializerSettings);

        /// <summary>
        /// Gets a health summary of the Webhooks Server API.
        /// </summary>
        /// <returns>A health summary of the Webhooks Server API.</returns>
        [HttpGet]
        public async Task<IActionResult> Health()
        {
            var summary = await _webhooksManager.GetHealthSummaryAsync(_workerOptions.PublishHealthEventInterval);
            summary.PublishIntervalMinutes = (int)_workerOptions.PublishHealthEventInterval.TotalMinutes;
            summary.ServerStartDateUtc = WebhooksHealthWorker.ServerStartDateUtc;
            
            return new JsonResult(summary, _serializerSettings);
        }

        /// <summary>
        /// Gets a list of the current webhooks event subscriptions and their endpoints.
        /// </summary>
        /// <returns>A list of the current webhooks event subscriptions and their endpoints.</returns>
        [HttpGet]
        public IActionResult Subscriptions() => new JsonResult(_webhooksManager.Subscriptions, _serializerSettings);
    }
}
