
namespace Marketplace.Domain.Interfaces;

public interface IEmailService
{
    Task SendOrderConfirmationAsync(Guid orderId, string userEmail, CancellationToken ct = default);
    Task SendLowStockAlertAsync(Guid productId, string sellerEmail, int currentStock, CancellationToken ct = default);
}
