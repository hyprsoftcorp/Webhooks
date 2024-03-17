using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hyprsoft.Webhooks.Client
{
    [ApiController]
    [Authorize(AuthenticationSchemes = ApiKeyDefaults.AuthenticationScheme)]
    [Route("[controller]/v{version:apiVersion}/[action]")]
    public abstract class WebhooksControllerBase : ControllerBase
    {
        protected static OkObjectResult WebhookOk() => new(new WebhookResponse { IsSuccess = true });

        protected static ObjectResult WebhookException(Exception ex) => new(new WebhookResponse { ErrorMessage = ex.Message }) { StatusCode = 500 };
    }
}
