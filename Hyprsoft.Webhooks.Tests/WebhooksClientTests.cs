using Hyprsoft.Webhooks.Client.Web;
using Hyprsoft.Webhooks.Client.Web.V1.Controllers;
using Hyprsoft.Webhooks.Core;
using Hyprsoft.Webhooks.Core.Events;
using Hyprsoft.Webhooks.Core.Management;
using Hyprsoft.Webhooks.Core.Rest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Tests
{
    [TestClass]
    public class WebhooksClientTests
    {
        private static IWebhooksManager _webhooksManager;

        [ClassInitialize]
#pragma warning disable IDE0060 // Remove unused parameter
        public static void Initialize(TestContext context)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var webhooksServer = Host.CreateDefaultBuilder();
            webhooksServer.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Hyprsoft.Webhooks.Server.Web.Startup>();
                webBuilder.UseUrls(WebhooksHttpClientOptions.DefaultServerBaseUri.ToString());
                webBuilder.UseEnvironment("UnitTest");
            });
            var host = webhooksServer.Start();
            _webhooksManager = host.Services.GetRequiredService<IWebhooksManager>();

            var webhooksClient = Host.CreateDefaultBuilder();
            webhooksClient.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Hyprsoft.Webhooks.Client.Web.Startup>();
                webBuilder.UseUrls(WebhooksWorkerOptions.DefaultWebhooksBaseUri.ToString());
                webBuilder.UseEnvironment("UnitTest");
            });
            webhooksClient.Start();
        }

        [TestMethod]
        public async Task Subscribe_Unsubscribe_Success()
        {
            // Filter our linq predicate so other unit tests don't interfere.
            static bool eventNameFilter(Subscription x) => x.EventName == typeof(PingWebhookEvent).FullName;
            using var client = new WebhooksClient(new WebhooksHttpClientOptions());

            // Subscribe should be idempotent.
            var uri = new Uri("http://mydomain.com/webhooks/v1/blah");
            await client.SubscribeAsync<PingWebhookEvent>(uri);
            Assert.AreEqual(1, _webhooksManager.Subscriptions.Count());
            await client.SubscribeAsync<PingWebhookEvent>(uri);
            Assert.AreEqual(1, _webhooksManager.Subscriptions.Where(eventNameFilter).Count());

            // Unsubscribe should be idempotent.
            await client.UnsubscribeAsync<PingWebhookEvent>(new Uri("http://mydomain.com/webhooks/v1/doesntexist"));
            Assert.AreEqual(1, _webhooksManager.Subscriptions.Where(eventNameFilter).Count());
            Assert.AreEqual(1, _webhooksManager.Subscriptions.Where(x => eventNameFilter(x) && x.Filter == null && x.FilterExpression == null && x.IsActive && x.WebhookUri == uri).Count());
            await client.UnsubscribeAsync<PingWebhookEvent>(uri);
            Assert.AreEqual(0, _webhooksManager.Subscriptions.Where(eventNameFilter).Count());
            await client.UnsubscribeAsync<PingWebhookEvent>(uri);
            Assert.AreEqual(0, _webhooksManager.Subscriptions.Where(eventNameFilter).Count());
        }

        [TestMethod]
        public async Task Publish_Success()
        {
            using var client = new WebhooksClient(new WebhooksHttpClientOptions());

            var uri = new Uri($"{WebhooksWorkerOptions.DefaultWebhooksBaseUri}webhooks/v1/{nameof(WebhooksController.Ping)}");
            await client.SubscribeAsync<PingWebhookEvent>(uri);
            await client.PublishAsync(new PingWebhookEvent());
            await client.UnsubscribeAsync<PingWebhookEvent>(uri);
        }

        [TestMethod]
        public async Task Publish_Fail()
        {
            var payload = new PingWebhookEvent { IsException = true };
            using var client = new WebhooksClient(new WebhooksHttpClientOptions());

            var uri = new Uri($"{WebhooksWorkerOptions.DefaultWebhooksBaseUri}webhooks/v1/{nameof(WebhooksController.Ping)}");
            await client.SubscribeAsync<PingWebhookEvent>(uri);
            await ThrowsWebhookExceptionContainingErrorText(() => client.PublishAsync(payload), "This webhook is misbehaving.");
            await client.UnsubscribeAsync<PingWebhookEvent>(uri);

            uri = new Uri($"{WebhooksWorkerOptions.DefaultWebhooksBaseUri}webhooks/v1/doesntexist");
            await client.SubscribeAsync<PingWebhookEvent>(uri);
            await ThrowsWebhookExceptionContainingErrorText(() => client.PublishAsync(payload), "Status: 404 Not Found");
            await client.UnsubscribeAsync<PingWebhookEvent>(uri);

            // Publish system event
            uri = new Uri($"{WebhooksWorkerOptions.DefaultWebhooksBaseUri}webhooks/v1/{nameof(WebhooksController.HealthSummary)}");
            await client.SubscribeAsync<WebhooksHealthEvent>(uri);
            await ThrowsWebhookExceptionContainingErrorText(() => client.PublishAsync(new WebhooksHealthEvent()), "system event and cannot be published");
            await client.UnsubscribeAsync<WebhooksHealthEvent>(uri);

            // Test a non WebhookResponse failure.
            uri = new Uri("https://www.google.com/webhooks/v1/test");
            await client.SubscribeAsync<PingWebhookEvent>(uri);
            await ThrowsWebhookExceptionContainingErrorText(() => client.PublishAsync(payload), "Status: 404 Not Found");
            await client.UnsubscribeAsync<PingWebhookEvent>(uri);
        }

        [TestMethod]
        public async Task PayloadSigningSecret_Fail()
        {
            using var client = new WebhooksClient(new WebhooksHttpClientOptions { PayloadSigningSecret = "xyz" });

            await ThrowsWebhookExceptionContainingErrorText(() =>
            {
                var uri = new Uri("http://mydomain.com/webhooks/v1/blah");
                return client.SubscribeAsync<PingWebhookEvent>(uri);
            }, "Status: 403 Forbidden");
        }

        [TestMethod]
        public void Create_PingWebhookEvent_Success()
        {
            var @event = new PingWebhookEvent();
            Assert.IsNotNull(@event.WebhookId);
            Assert.IsTrue(@event.CreatedUtc <= DateTime.UtcNow);
            Assert.AreEqual(Dns.GetHostName(), @event.OriginatorHostname);
            Assert.IsFalse(@event.IsException);
        }

        private static async Task ThrowsWebhookExceptionContainingErrorText(Func<Task> callback, string errorText)
        {
            try
            {
                await callback.Invoke();
            }
            catch (WebhookException ex) when (ex.Message.Contains(errorText))
            {
                return;
            }
            catch (Exception)
            {
                throw;
            }

            throw new WebhookException($"This webhook should have thrown an exception containing the following text '{errorText}' but did not.");
        }
    }
}
