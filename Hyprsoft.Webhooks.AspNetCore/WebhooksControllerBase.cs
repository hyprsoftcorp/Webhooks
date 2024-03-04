using Microsoft.AspNetCore.Mvc;
using Hyprsoft.Webhooks.Core.Rest;
using System;
using Microsoft.AspNetCore.Authorization;
using AspNetCore.Authentication.ApiKey;

namespace Hyprsoft.Webhooks.AspNetCore
{
    [Authorize(AuthenticationSchemes = ApiKeyDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("[controller]/v{version:apiVersion}/[action]")]
    public class WebhooksControllerBase : ControllerBase
    {
        protected static OkObjectResult WebhookOk() => new(new WebhookResponse { IsSuccess = true });

        protected static ObjectResult WebhookException(Exception ex) => new(new WebhookResponse { ErrorMessage = ex.Message }) { StatusCode = 500 };
    }
}
