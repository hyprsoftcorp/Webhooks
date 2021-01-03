using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hyprsoft.Webhooks.AspNetCore;
using Hyprsoft.Webhooks.Core.Events;
using Newtonsoft.Json;
using System;

namespace Hyprsoft.Webhooks.Client.Web.V1.Controllers
{
    [ApiVersion("1.0")]
    public class WebhooksController : WebhooksControllerBase
    {
        #region Fields

        private readonly ILogger<WebhooksController> _logger;

        #endregion

        #region Constructors

        public WebhooksController(ILogger<WebhooksController> logger)
        {
            _logger = logger;
        }

        #endregion

        #region Methods

        [HttpPost]
        public IActionResult Ping(PingWebhookEvent @event)
        {
            try
            {
                _logger.LogInformation($"Received event '{nameof(PingWebhookEvent)}' with payload '{JsonConvert.SerializeObject(@event)}'.");
                if (@event.IsException)
                    throw new InvalidOperationException("This webhook is misbehaving.");
                return WebhookOk();
            }
            catch (Exception ex)
            {
                return WebhookException(ex);
            }
        }

        [HttpPost]
        public IActionResult HealthSummary(WebhooksHealthEvent @event)
        {
            try
            {
                _logger.LogInformation($"Received event '{nameof(WebhooksHealthEvent)}' with payload '{JsonConvert.SerializeObject(@event)}'.");
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
