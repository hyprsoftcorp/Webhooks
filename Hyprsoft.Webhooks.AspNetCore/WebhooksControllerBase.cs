using Microsoft.AspNetCore.Mvc;
using Hyprsoft.Webhooks.Core.Rest;
using System;

namespace Hyprsoft.Webhooks.AspNetCore
{
    [ApiController]
    [Route("[controller]/v{version:apiVersion}/[action]")]
    public class WebhooksControllerBase : ControllerBase
    {
        protected OkObjectResult WebhookOk() => new OkObjectResult(new WebhookResponse { IsSuccess = true });

        protected ObjectResult WebhookException(Exception ex) => new ObjectResult(new WebhookResponse { ErrorMessage = ex.Message }) { StatusCode = 500 };
    }
}
