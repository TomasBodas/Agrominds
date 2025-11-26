using System.Web;
using System.Web.Mvc;
using UAIDesarrolloArquitectura.Filters;

namespace UAIDesarrolloArquitectura
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            // Global auth filter
            filters.Add(new RequireLoginFilter());
        }
    }
}
