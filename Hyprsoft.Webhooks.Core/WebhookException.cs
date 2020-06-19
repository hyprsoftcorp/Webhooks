using System;

namespace Hyprsoft.Webhooks.Core
{
    public class WebhookException : Exception
    {
        public WebhookException() : base() { }

        public WebhookException(string message) : base(message) { }

        public WebhookException(string message, Exception exception) : base(message, exception) { }
    }
}
