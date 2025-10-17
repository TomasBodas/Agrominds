using BLL;
using DAL;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using UAIDesarrolloArquitectura.ServicePrecios;

namespace UAIDesarrolloArquitectura.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Startup()
        {
            return View("Index");
        }
        [HttpGet]
        public ActionResult Index()
        {
            return View("Index");
        }
        public ActionResult Plans()
        {
            // Crear una instancia del cliente del servicio web
            var servicioPrecios = new ObtPrecios(); // Asegúrate de que 'ObtPrecios' sea el nombre correcto del proxy

            // Llamar al método del WebService para obtener los precios
            var precios = servicioPrecios.ObtenerPrecios(); // Llama al método directamente

            // Pasar los precios a la vista usando ViewBag
            ViewBag.Precios = precios;

            return View("Plans");
        }

        public ActionResult Bitacora()
        {
            return View("Bitacora");
        }

        public ActionResult Logout()
        {
            
            DAL_User dalUser = new DAL_User();
            dalUser.EventLog(SessionManager.GetInstance.User.id, DateTime.Now.ToString(), "Cierre de sesión", "Se cerró sesión");
            BLL_CheckDigitsManager bll_dvmanager = new BLL_CheckDigitsManager();
            bll_dvmanager.SetCheckDigits();
            SessionManager.logout();
            return View("Index");
        }
    }
}