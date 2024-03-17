using Asp.Versioning;
using Hyprsoft.Webhooks.Events;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Hyprsoft.Webhooks.Client.Web.V1.Controllers
{
    [ApiVersion("1.0")]
    public sealed class WebhooksController : WebhooksControllerBase
    {
        #region Fields

        private readonly ILogger<WebhooksController> _logger;

        #endregion

        #region Constructors

        public WebhooksController(ILogger<WebhooksController> logger) => _logger = logger;

        #endregion

        #region Methods

        [HttpPost]
        public IActionResult Ping(PingEvent @event)
        {
            try
            {
                _logger.LogInformation("Received event '{eventName}' with payload '{payload}'.", nameof(PingEvent), JsonConvert.SerializeObject(@event));
                if (@event.IsException)
                    throw new InvalidOperationException("This webhook is misbehaving.");
                return WebhookOk();
            }
            catch (Exception ex)
            {
                return WebhookException(ex);
            }
        }

        #endregion
    }
}
