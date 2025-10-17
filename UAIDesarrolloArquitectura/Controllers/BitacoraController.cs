using DAL;
using Services;
using Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using System.Windows;
using System.Xml.Linq;
using UAIDesarrolloArquitectura.Models.ViewModel;


namespace UAIDesarrolloArquitectura.Controllers
{
    public class BitacoraController : Controller
    {
        DAL_Log dAL_Log = new DAL_Log();
        public ActionResult Bitacora()
        {
            if (!SessionManager.IsLogged())
            {
                return RedirectToAction("Login", "Login");
            }
            if (SessionManager.GetInstance.User.Name == "Administrador")
            {
                List<Log> list = dAL_Log.getLog();
                foreach (Log log in list)
                {
                    log.User = PasswordEncrypter.DecryptData(log.User);
                }
                return View(list);
            }
            else return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public ActionResult Bitacora(DateTime? Fechainicio, DateTime? Fechafinal, string Modulo = null)
        {
            //lista de todos los registros
            List<Log> list = dAL_Log.getLog();
            //si se ingresaron fechas
            if (Fechainicio != null && Fechafinal != null)
            {
                //nueva lista de registros desde una fecha hasta la otra
                list = list.Where(p => p.Fecha.Date >= Fechainicio.Value.Date && p.Fecha.Date <= Fechafinal.Value.Date).ToList();
            }
            //si se ingreso el modulo
            if (Modulo != null && !Modulo.IsEmpty())
            {
                //nueva lista de registros por modulo
                list = list.Where(p => p.Modulo == Modulo).ToList();
            }
            //desencriptamos los emails
            foreach (Log log in list)
            {
                log.User = PasswordEncrypter.DecryptData(log.User);
            }
            return View(list);
        }

        [HttpPost]
        public ActionResult ExportXML()
        {
            List<Log> logs = dAL_Log.getLog();

            // Crear el documento XML
            var xmlDocument = new XDocument(
                new XElement("Logs",
                    logs.Select(log =>
                        new XElement("Log",
                            new XElement("ID", log.id),
                            new XElement("Usuario", log.User),
                            new XElement("Fecha", log.Fecha),
                            new XElement("Modulo", log.Modulo),
                            new XElement("Descripcion", log.Descripcion)
                        )
                    )
                )
            );

            // Convertir el documento XML a una cadena de bytes
            var xmlBytes = System.Text.Encoding.UTF8.GetBytes(xmlDocument.ToString());

            // Devolver el archivo XML como respuesta para descargar
            return File(xmlBytes, "application/xml", "Bitacora.xml");
        }
    }
}