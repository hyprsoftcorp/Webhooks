namespace Hyprsoft.Webhooks.Core.Events
{
    public abstract class SampleWebhookEvent : WebhookEvent
    {
        public int SampleId { get; set; }

        public int SampleType { get; set; }

        public int UserId { get; set; }
    }
}
