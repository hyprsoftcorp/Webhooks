using Hyprsoft.Webhooks.Core.Rest;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.AspNetCore
{
    public sealed class WebhooksAuthorizationMiddleware
    {
        #region Fields

        private readonly RequestDelegate _next;

        #endregion

        #region Constructors

        public WebhooksAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        #endregion

        #region Methods

        public async Task Invoke(HttpContext httpContext, WebhooksAuthorizationOptions options)
        {
            if (httpContext.Request.Path.StartsWithSegments(options.Segments))
            {
                if (httpContext.Request.Headers.ContainsKey(WebhooksHttpClient.PayloadSignatureHeaderName))
                {
                    if (String.IsNullOrWhiteSpace(options.PayloadSigningSecret))
                        throw new InvalidOperationException("Missing payload signing secret.  Please check your configuration.");

                    httpContext.Request.EnableBuffering();
                    var requestPayload = new byte[Convert.ToInt32(httpContext.Request.ContentLength)];
                    await httpContext.Request.Body.ReadAsync(requestPayload, 0, requestPayload.Length).ConfigureAwait(false);
                    httpContext.Request.Body.Position = 0;

                    if (httpContext.Request.Headers[WebhooksHttpClient.PayloadSignatureHeaderName] != WebhooksHttpClient.GetSignature(options.PayloadSigningSecret, Encoding.UTF8.GetString(requestPayload)))
                    {
                        httpContext.Response.ContentType = "application/json";
                        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                        var error = await CreateResponseBodyAsync("The request payload does not match the request payload signature.").ConfigureAwait(false);
                        await httpContext.Response.Body.WriteAsync(error, 0, error.Length).ConfigureAwait(false);
                        return;
                    }
                }
                else
                {
                    httpContext.Response.ContentType = "application/json";
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    var error = await CreateResponseBodyAsync($"The '{WebhooksHttpClient.PayloadSignatureHeaderName}' request header is missing.").ConfigureAwait(false);
                    await httpContext.Response.Body.WriteAsync(error, 0, error.Length).ConfigureAwait(false);
                    return;
                }
            }
            await _next(httpContext);
        }

        private async Task<byte[]> CreateResponseBodyAsync(string error) => await new WebhookContent(new WebhookResponse { ErrorMessage = error }).ReadAsByteArrayAsync().ConfigureAwait(false);

        #endregion
    }
}
