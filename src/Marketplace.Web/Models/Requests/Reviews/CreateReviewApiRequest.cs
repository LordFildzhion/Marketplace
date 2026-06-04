
namespace Marketplace.Web.Models.Requests.Reviews;

public class CreateReviewApiRequest
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
