using Marketplace.Application.Common.Enums;
using Marketplace.Web.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Marketplace.Web.Filters;

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(
        ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(e => e.Value!.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!
                        .Errors
                        .Select(e => e.ErrorMessage)
                        .ToArray());

            context.Result = new BadRequestObjectResult(
                new ErrorResponse(
                    "Validation failed",
                    ErrorCode.Validation,
                    errors));
        }
    }
}