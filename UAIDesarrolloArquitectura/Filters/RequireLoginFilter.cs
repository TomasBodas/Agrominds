using System;
using System.Linq;
using System.Web.Mvc;
using Services;
using System.Configuration;

namespace UAIDesarrolloArquitectura.Filters
{
    // Authorization filter that requires login except for whitelisted actions
    public class RequireLoginFilter : AuthorizeAttribute
    {
        // Actions that do NOT require login
        private static readonly string[] Whitelist = new[]
        {
            "Startup","Index","Nosotros","Contact","Privacy","Plans",
			// Common auth and public endpoints
			"Login","Register",
			// Public device endpoints (secured by X-Api-Key at controller level)
			"RegisterTelemetry","ChangeState"
        };

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var action = filterContext.ActionDescriptor.ActionName;
            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

            // Allow if action is in whitelist
            if (Whitelist.Any(a => string.Equals(a, action, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }
            // Allow controllers explicitly public (Login, Register)
            if (string.Equals(controller, "Login", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(controller, "Register", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            // Exception: allow language switch endpoint from combobox (Idioma.SetLanguage)
            if (string.Equals(controller, "Idioma", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(action, "SetLanguage", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Allow Robot endpoints when X-Api-Key header is valid (simuladores/dispositivos)
            if (string.Equals(controller, "Robot", StringComparison.OrdinalIgnoreCase))
            {
                var apiKeyHeader = filterContext.HttpContext.Request.Headers["X-Api-Key"];
                var deviceApiKey = ConfigurationManager.AppSettings["DeviceApiKey"] ?? string.Empty;
                if (!string.IsNullOrEmpty(apiKeyHeader) && string.Equals(apiKeyHeader, deviceApiKey, StringComparison.Ordinal))
                {
                    return;
                }
            }

            // Block if not logged in
            if (!SessionManager.IsLogged())
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(new { controller = "Login", action = "Login" }));
            }
        }
    }
}
