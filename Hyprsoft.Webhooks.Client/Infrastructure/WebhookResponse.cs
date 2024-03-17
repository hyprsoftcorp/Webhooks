namespace Hyprsoft.Webhooks.Client
{
    public sealed class WebhookResponse
    {
        public bool IsSuccess { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
