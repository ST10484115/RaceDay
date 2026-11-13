
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.MVC.Filters;

public class SessionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string? _requiredRole;

    public SessionAuthorizeAttribute(string? requiredRole = null)
    {
        _requiredRole = requiredRole;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var role = context.HttpContext.Session.GetString("UserRole");

        // Redirect users who are not logged in.
        if (string.IsNullOrWhiteSpace(role))
        {
            context.Result = new RedirectToActionResult(
                "Login", "Auth", null);
            return Task.CompletedTask;
        }

        // Restrict access to the required role.
        if (_requiredRole != null &&
            !string.Equals(role, _requiredRole,
                StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new RedirectToActionResult(
                "Index", role, null);
        }

        return Task.CompletedTask;
    }
}
