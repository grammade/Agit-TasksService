using TaskService.Models;
using System.Security.Claims;

namespace TaskService.Services;

public class UserAuthorizationService
{
    private readonly ClaimsPrincipal? _user;
    private AuthorizedUser _authUser;
    public UserAuthorizationService(IHttpContextAccessor httpContext)
    {
        _user = httpContext.HttpContext?.User;
    }

    public AuthorizedUser getAuthorizedUser()
    {
        if (_user is null)
            return null;

        if (_authUser is not null)
            return _authUser;

        var username = _user.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Name))?.Value;
        var userId = _user.Claims.FirstOrDefault(c => c.Type.Equals("UserId"))?.Value;
        _authUser = new AuthorizedUser()
        {
            Id = Convert.ToInt32(userId),
            UserName = username
        };
        return _authUser;
    }
    public void setIP(string ip)
    {
        getAuthorizedUser().IP = ip;
    }
}
