
namespace Marketplace.Web.Models.Requests.Auth;

public class LoginApiRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
