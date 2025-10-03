using BLL;
using DAL;
using Services;
using Services.Models;
using Services.Perfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Windows;
using UAIDesarrolloArquitectura.Models.ViewModel;


namespace UAIDesarrolloArquitectura.Controllers
{
    public class CrearPerfilesController : Controller
    {
        DAL_Permission DAL_Permission = new DAL_Permission();
        public ActionResult CrearPerfiles()
        {
            if (!SessionManager.IsLogged())
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                List<Role> roles = DAL_Permission.GetRoles();
                List<Permission> permissions = DAL_Permission.GetPermissions();
                var viewModel = new CrearPerfilViewModel
                {
                    Roles = roles,
                    Permisos = permissions
                };
                return View(viewModel);
            }
        }
        [HttpPost]
        public JsonResult GuardarPerfil(PerfilViewModel perfilData)
        {
            try
            {
                DAL_Permission.SaveProfile(perfilData.NombrePerfil);
                // Procesa la lista de permisos y roles
                foreach (var permiso in perfilData.Permisos)
                {
                    DAL_Permission.AddAuthToProfile(permiso.Nombre);
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log de error
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}