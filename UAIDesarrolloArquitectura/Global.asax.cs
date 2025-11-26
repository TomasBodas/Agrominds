using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using UAIDesarrolloArquitectura.Background;

namespace UAIDesarrolloArquitectura
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static TaskAutoStartService _taskService;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Iniciar servicio en segundo plano cada60 segundos
            _taskService = new TaskAutoStartService(TimeSpan.FromSeconds(60));
            _taskService.Start();
        }

        protected void Application_End()
        {
            if (_taskService != null)
            {
                _taskService.Stop();
                _taskService.Dispose();
                _taskService = null;
            }
        }
    }
}
