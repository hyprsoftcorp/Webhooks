using Microsoft.AspNetCore.Http;
using Hyprsoft.Webhooks.Core.Rest;
using System;
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
                    await httpContext.Request.Body.ReadAsync(requestPayload, 0, requestPayload.Length);
                    httpContext.Request.Body.Position = 0;

                    if (httpContext.Request.Headers[WebhooksHttpClient.PayloadSignatureHeaderName] != WebhooksHttpClient.GetSignature(options.PayloadSigningSecret, Encoding.UTF8.GetString(requestPayload)))
                    {
                        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return;
                    }
                }
                else
                {
                    httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
            }
            await _next(httpContext);
        }

        #endregion
    }
}
