using Asp.Versioning;
using Hyprsoft.Webhooks.Client;
using Hyprsoft.Webhooks.Events;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Hyprsoft.Webhooks.Server.Web.V1.Controllers
{
    /// <summary>
    /// Webhooks Server API.
    /// </summary>
    [ApiVersion("1.0")]
    public sealed class WebhooksController : WebhooksControllerBase
    {
        #region Fields

        private readonly ILogger<WebhooksController> _logger;
        private readonly IWebhooksManager _webhooksManager;

        #endregion

        #region Constructors

        public WebhooksController(ILogger<WebhooksController> logger, IWebhooksManager webhooksManger)
        {
            _logger = logger;
            _webhooksManager = webhooksManger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Subscribe to a webhook event.
        /// </summary>
        /// <param name="request">The subscription request.</param>
        /// <returns>A webhook response indicating success or failure and the reason for the failure if applicable.</returns>
        [HttpPut]
        public async Task<ActionResult<WebhookResponse>> Subscribe(SubscriptionRequest request)
        {
            try
            {
                _logger.LogDebug("Creating subscription for '{eventName}' with webhook '{webhookUri}'.", request.EventName, request.WebhookUri);
                await _webhooksManager.SubscribeAsync(request.EventName, request.WebhookUri, request.Filter);
                return WebhookOk();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscribe failed.");
                return WebhookException(ex);
            }
        }

        /// <summary>
        /// Unsubscribe from a webhook event.
        /// </summary>
        /// <param name="request">The subscription request.</param>
        /// <returns>A webhook response indicating success or failure and the reason for the failure if applicable.</returns>
        [HttpDelete]
        public async Task<ActionResult<WebhookResponse>> Unsubscribe(SubscriptionRequest request)
        {
            try
            {
                _logger.LogDebug("Removing subscription for '{eventName}' with webhook '{webhookUri}'.", request.EventName, request.WebhookUri);
                await _webhooksManager.UnsubscribeAsync(request.EventName, request.WebhookUri);
                return WebhookOk();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unsubscribe failed.");
                return WebhookException(ex);
            }
        }

        /// <summary>
        /// Publishes a webhook event to all subscribers.
        /// </summary>
        /// <param name="event">The event to publish.</param>
        /// <returns>A webhook response indicating success or failure and the reason for the failure if applicable.</returns>
        [HttpPost]
        public async Task<ActionResult<WebhookResponse>> Publish(WebhookEvent @event)
        {
            try
            {
                // TODO: Depending on log level setting, this COULD log sensitive information.
                _logger.LogDebug("Publishing event '{fullName}' with payload '{payload}'.", @event.GetType().FullName, JsonConvert.SerializeObject(@event));
                await _webhooksManager.PublishAsync(@event);
                return WebhookOk();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Publish failed.");
                return WebhookException(ex);
            }
        }

        #endregion
    }
}
