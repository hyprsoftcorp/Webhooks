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
        public IActionResult SampleCreated(SampleCreatedWebhookEvent @event)
        {
            try
            {
                _logger.LogInformation($"Received event '{nameof(SampleCreatedWebhookEvent)}' with payload '{JsonConvert.SerializeObject(@event)}'.");
                //throw new InvalidOperationException("This webhook is misbehaving.");
            }
            catch (Exception ex)
            {
                return WebhookException(ex);
            }
            return WebhookOk();
        }

        [HttpPost]
        public IActionResult SampleIsActiveChanged(SampleIsActiveChangedWebhookEvent @event)
        {
            try
            {
                _logger.LogInformation($"Received event '{nameof(SampleIsActiveChangedWebhookEvent)}' with payload '{JsonConvert.SerializeObject(@event)}'.");
            }
            catch (Exception ex)
            {
                return WebhookException(ex);
            }
            return WebhookOk();
        }

        [HttpPost]
        public IActionResult SampleDeleted(SampleDeletedWebhookEvent @event)
        {
            try
            {
                _logger.LogInformation($"Received event '{nameof(SampleDeletedWebhookEvent)}' with payload '{JsonConvert.SerializeObject(@event)}'.");
            }
            catch (Exception ex)
            {
                return WebhookException(ex);
            }
            return WebhookOk();
        }

        #endregion
    }
}
