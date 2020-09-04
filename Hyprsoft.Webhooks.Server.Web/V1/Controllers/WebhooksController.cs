using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hyprsoft.Webhooks.AspNetCore;
using Hyprsoft.Webhooks.Core.Events;
using Hyprsoft.Webhooks.Core.Management;
using Hyprsoft.Webhooks.Core.Rest;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Hyprsoft.Webhooks.Server.Web.V1.Controllers
{
    [ApiVersion("1.0")]
    public class WebhooksController : WebhooksControllerBase
    {
        #region Fields

        private readonly ILogger<WebhooksController> _logger;
        private readonly IWebhooksManager _webhooksManager;
        private static readonly List<Type> _systemEvents = new List<Type> { typeof(WebhooksHealthEvent) };

        #endregion

        #region Constructors

        public WebhooksController(ILogger<WebhooksController> logger, IWebhooksManager webhooksManger)
        {
            _logger = logger;
            _webhooksManager = webhooksManger;
        }

        #endregion

        #region Methods

        [HttpPut]
        public async Task<ActionResult<WebhookResponse>> Subscribe(SubscriptionRequest request)
        {
            try
            {
                _logger.LogDebug($"Creating subscription for '{request.EventName}' with webhook '{request.WebhookUri}'.");
                await _webhooksManager.SubscribeAsync(request.EventName, request.WebhookUri, request.Filter);
                return WebhookOk();
            }
            catch (Exception ex)
            {
                _logger.LogError("Subscribe failed.", ex);
                return WebhookException(ex);
            }
        }

        [HttpDelete]
        public async Task<ActionResult<WebhookResponse>> Unsubscribe(SubscriptionRequest request)
        {
            try
            {
                _logger.LogDebug($"Removing subscription for '{request.EventName}' with webhook '{request.WebhookUri}'.");
                await _webhooksManager.UnsubscribeAsync(request.EventName, request.WebhookUri);
                return WebhookOk();
            }
            catch (Exception ex)
            {
                _logger.LogError("Unsubscribe failed.", ex);
                return WebhookException(ex);
            }
        }

        [HttpPost]
        public async Task<ActionResult<WebhookResponse>> Publish(WebhookEvent @event)
        {
            try
            {
                if (_systemEvents.Contains(@event.GetType()))
                    throw new InvalidOperationException("The '{@event.GetType().FullName}' event is a system event and cannot be published.");

                // TODO: Depending on log level setting, this COULD log sensitive information.
                _logger.LogDebug($"Publishing event '{@event.GetType().FullName}' with payload '{JsonConvert.SerializeObject(@event)}'.");
                await _webhooksManager.PublishAsync(@event);
                return WebhookOk();
            }
            catch (Exception ex)
            {
                _logger.LogError("Publish failed.", ex);
                return WebhookException(ex);
            }
        }

        #endregion
    }
}
