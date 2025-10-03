using Antlr.Runtime.Tree;
using BLL;
using DAL;
using Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Windows;
using UAIDesarrolloArquitectura.Models.ViewModel;
using WebGrease.Activities;


namespace UAIDesarrolloArquitectura.Controllers
{
    public class BackupController : Controller
    {
        public ActionResult ErrorDV()
        {
            if (!SessionManager.IsLogged())
            {
                return RedirectToAction("Login", "Login");
            }
            return View("CorruptDatabaseMessage", BLL_CheckDigitsManager.GetErrors());
        }
        public ActionResult Backup()
        {
            DataBaseServices.BackupDatabase();
            var model = new BackupViewModel
            {
                BackupSuccess = true,
                Message = "El respaldo de la base de datos se creo con exito."
            };
            return View("BackupRestore", model);
        }
        [HttpPost]
        public ActionResult Restore(string path, string ReturnUrl)
        {
            //Le pide al que se encarga de la base de datos de restaurarla mandandole la ruta del archivo que necesita
            //El encargado le dice si se puedo hacer o no
            bool IsSuccess = DataBaseServices.RestoreDatabase(path);
            //El controlador notifica en el diccionario en la clave IsSuccess si se pudo hacer o no para que la vista lo obtenga
            TempData["IsSuccess"] = IsSuccess;
            if (ReturnUrl == "/Backup/CorrupDatabaseMessage")
            {
                return View("Login");
            }
            else return Redirect(ReturnUrl);
        }
        public ActionResult SetCheckDigits()
        {
            BLL_CheckDigitsManager checkDigitsManager = new BLL_CheckDigitsManager();
            checkDigitsManager.SetCheckDigits();
            ViewData["SetCheckDigitsIsSuccess"] = true;
            return View("CorruptDatabaseMessage");
        }
        public ActionResult BackupRestore()
        {
            if (!SessionManager.IsLogged())
            {
                return RedirectToAction("Login", "Login");
            }
            else return View("BackupRestore");
        }
    }
}