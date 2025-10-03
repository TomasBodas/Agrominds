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
    public class AsignarPerfilController : Controller
    {
        public ActionResult AsignarPerfil()
        {
            if (!SessionManager.IsLogged())
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                List<string> profiles = DAL_Permission.GetProfiles();
                DAL_User user = new DAL_User();
                List<User> usuarios = user.getUsers();
                var viewModel = new PerfilesUsuariosViewModel
                {
                    Perfiles = profiles,
                    Usuarios = usuarios
                };
                return View(viewModel);
            }
        }
        [HttpPost]
        public JsonResult AsignarPerfilUsuario(AsignacionPerfilUsuarioModel datos)
        {
            try
            {
                if (datos == null || string.IsNullOrEmpty(datos.Perfil) || string.IsNullOrEmpty(datos.Usuario))
                {
                    return Json(new { success = false, error = "Datos inválidos." });
                }
                DAL_User usuario = new DAL_User();
                usuario.AssignProfile(datos.Perfil, datos.Usuario);
                return Json(new { success = true });
            }
            catch(Exception ex)
            {
                // Log de error
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}