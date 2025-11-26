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
using Services.Models; // agregado para Robot

namespace UAIDesarrolloArquitectura.Controllers
{
    public class HomeController : Controller
    {
        private readonly BLL_Robot _bllRobot = new BLL_Robot(); // BLL para robots y estados

        private ActionResult RequireLogin()
        {
            if (!SessionManager.IsLogged())
            {
                return RedirectToAction("Login", "Login");
            }
            return null;
        }

        public ActionResult Startup()
        {
            return View("Index");
        }
        [HttpGet]
        public ActionResult Index()
        {
            return View("Index");
        }

        // Nueva acción para Landing de clientes
        [HttpGet]
        public ActionResult Robots()
        {
            var guard = RequireLogin(); if (guard != null) return guard;
            // Obtener robots y estados vía BLL (ya no directamente desde DAL)
            IList<Robot> robots = _bllRobot.GetRobotsForCurrentUser();
            ViewBag.EstadosRobot = _bllRobot.GetEstadosRobot();
            return View("Robots", robots);
        }

        // Nueva acción para la página "Nosotros" / AboutUs
        [HttpGet]
        public ActionResult Nosotros()
        {
            return View("AboutUs");
        }

        // Nueva acción para la página "Contacto" / Contact
        [HttpGet]
        public ActionResult Contact()
        {
            return View("Contact");
        }

        // Nueva acción para la página "Privacy"
        [HttpGet]
        public ActionResult Privacy()
        {
            return View("Privacy");
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
            if (SessionManager.IsLogged())
            {
                dalUser.EventLog(SessionManager.GetInstance.User.id, DateTime.Now.ToString(), "Cierre de sesión", "Se cerró sesión");
                BLL_CheckDigitsManager bll_dvmanager = new BLL_CheckDigitsManager();
                bll_dvmanager.SetCheckDigits();
            }
            SessionManager.logout();
            return View("Index");
        }

        // Nueva acción para cargar mantenimientos de robots
        [HttpGet]
        public ActionResult Mantenimientos()
        {
            var guard = RequireLogin(); if (guard != null) return guard;
            var dalRobot = new DAL_Robot();
            var dalList = dalRobot.GetAllRobots();
            var mantenimientosPorRobot = new Dictionary<int, IList<MantenimientoRobot>>();
            foreach (var r in dalList)
            {
                var mts = dalRobot.GetMaintenances(r.Id, null, null,100);
                mantenimientosPorRobot[r.Id] = mts;
            }
            ViewBag.Robots = dalList; // lista de robots para referencias
            return View("Mantenimientos", mantenimientosPorRobot);
        }

        // Nueva acción para Telemetría (reemplaza Estadísticas)
        [HttpGet]
        public ActionResult Telemetria()
        {
            var guard = RequireLogin(); if (guard != null) return guard;
            var dalRobot = new DAL_Robot();
            var robots = dalRobot.GetAllRobots();
            var telemetriaPorRobot = new Dictionary<int, IList<TelemetriaRobot>>();
            foreach (var r in robots)
            {
                var tel = dalRobot.GetTelemetry(r.Id, null, null,100);
                telemetriaPorRobot[r.Id] = tel;
            }
            ViewBag.Robots = robots;
            return View("Telemetria", telemetriaPorRobot);
        }

        // Nueva acción ContactUs para sidebar
        [HttpGet]
        public ActionResult ContactUs()
        {
            var guard = RequireLogin(); if (guard != null) return guard;
            return View("ContactUs");
        }
    }
}