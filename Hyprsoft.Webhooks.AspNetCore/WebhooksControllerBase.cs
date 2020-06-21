using Microsoft.AspNetCore.Mvc;
using Hyprsoft.Webhooks.Core.Rest;
using System;

namespace Hyprsoft.Webhooks.AspNetCore
{
    [ApiController]
    [Route("[controller]/v{version:apiVersion}/[action]")]
    public class WebhooksControllerBase : ControllerBase
    {
        public OkObjectResult WebhookOk() => new OkObjectResult(new WebhookResponse { IsSuccess = true });
        
        public ObjectResult WebhookException(Exception ex) => new ObjectResult(new WebhookResponse { IsSuccess = false, ErrorMessage = ex.ToString() }) { StatusCode = 500 };
    }
}
