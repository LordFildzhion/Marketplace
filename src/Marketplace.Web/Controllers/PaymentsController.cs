
using System.Security.Claims;
using Marketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Web.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService) => _paymentService = paymentService;

    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost("{orderId:guid}/pay")]
    public async Task<ActionResult> Pay(Guid orderId)
    {
        var result = await _paymentService.ProcessOrderPaymentAsync(orderId, UserId);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }
}
