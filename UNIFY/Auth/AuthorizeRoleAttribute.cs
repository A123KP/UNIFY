using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;

namespace UNIFY.Auth
{
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userRole = context.HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole)) //check for user role multiple roles
            {
                // Redirect to login or access denied page
                context.Result = new RedirectToActionResult("AccessDenied","Home", null);
            }
        }
    }
}




