
using Microsoft.AspNetCore.Mvc.Filters;

namespace Marketplace.Web.Filters;

public class AuditLogAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await next();
    }
}
