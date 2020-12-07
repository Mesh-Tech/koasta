using System;
using Microsoft.AspNetCore.Mvc;

namespace Koasta.Shared.Middleware
{
    public static class MiddlewareExtensions
    {
        public static AuthContext GetAuthContext(this Controller controller)
        {
            object context;
            if (!controller.HttpContext.Items.TryGetValue("pubcrawlauthcontext", out context))
            {
                throw new InvalidOperationException("Authorisation middleware is not active in this application");
            }

            if (!(context is AuthContext))
            {
                throw new InvalidOperationException("Authorisation middleware is not active in this application");
            }

            return (AuthContext)context;
        }
    }
}
