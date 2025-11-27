using BLL;
using DAL;
using Services;
using Services.Models;
using Services.Perfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using UAIDesarrolloArquitectura.Models.ViewModel;

namespace UAIDesarrolloArquitectura.Controllers
{
    public class LoginController : Controller
    {
        private readonly DAL_User _dalUser = new DAL_User();

        [HttpGet]
        public ActionResult Login()
        {
            SessionManager.logout();
            return View("Login");
        }
        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    User user;
                    DAL_User dalUser = new DAL_User();
                    string emailHash = PasswordEncrypter.EncryptData(model.Email);
                    user = dalUser.findByEmail(emailHash);
                    string Hash = PasswordEncrypter.EncryptPassword(model.Password);
                    if (user != null)
                    {
                        if (dalUser.userPasswordMatcher(user.Password, Hash))
                        {
                            //Singleton setup
                            SessionManager.login(user);
                            CreatePermissionsList(user);
                            //DV Check
                            BLL_CheckDigitsManager checkDigitsManager = new BLL_CheckDigitsManager();
                            if (!checkDigitsManager.CheckDigits())
                            {
                                return RedirectToAction("ErrorDV", "Backup");
                            }
                            else dalUser.EventLog(user.id, DateTime.Now.ToString(), "Inicio de sesión", "Se inició sesión");
                            checkDigitsManager.SetCheckDigits();

                            // Redireccion directa para perfil Cliente
                            if (SessionManager.GetInstance.User.profile != null &&
                                string.Equals(SessionManager.GetInstance.User.profile.Name, "Cliente", StringComparison.OrdinalIgnoreCase))
                            {
                                return RedirectToAction("Robots", "Home"); // cambiado desde 'Clientes'
                            }
                        }
                        else throw new Exception();
                        return RedirectToAction("Index", "Home");
                    }
                    else throw new Exception();
                }
            }
            catch (Exception) { ModelState.AddModelError("MissingUser", "No existe un usuario con estos datos"); }

            return View(model);
        }
        public void CreatePermissionsList(User pUser)
        {
            DAL_Permission dalPermission = new DAL_Permission();
            if (SessionManager.IsLogged())
            {

                Profile userProfile = SessionManager.GetInstance.User.profile;
                dalPermission.FillProfileAuths(userProfile);
                pUser.permissionList.Clear();
                foreach (Auth auth in userProfile.Children)
                {
                    if (auth is Permission)
                    {
                        pUser.permissionList.Add(auth.Permission.ToString());
                    }
                    else Recorrer(auth, pUser);
                }
            }
        }
        private void Recorrer(Auth pAuth, User pUser)
        {
            foreach (Auth auth in pAuth.Children)
            {
                if (auth is Role) Recorrer(auth, pUser);
                else pUser.permissionList.Add(auth.Permission.ToString());
            }
        }

        [HttpPost]
        public ActionResult ForgotPassword()
        {
            try
            {
                Request.InputStream.Position =0;
                using (var reader = new System.IO.StreamReader(Request.InputStream))
                {
                    var body = reader.ReadToEnd();
                    var email = Newtonsoft.Json.Linq.JObject.Parse(body).Value<string>("email");
                    if (string.IsNullOrWhiteSpace(email)) return Json(new { success = false, error = "Email requerido" });
                    var tempPass = Guid.NewGuid().ToString("N").Substring(0,10);
                    bool ok = _dalUser.UpdatePasswordByEmail(email, tempPass);
                    if (!ok) return Json(new { success = false, error = "Usuario no encontrado" });
                    // Recalcular dígitos verificadores tras actualizar la base
                    try { new BLL_CheckDigitsManager().SetCheckDigits(); } catch { }
                    try
                    {
                        var msg = new MailMessage("no-reply@agrominds.test", email)
                        { Subject = "Recuperación de contraseña", Body = $"Su nueva contraseña temporal es: {tempPass}" };
                        // Use Web.config system.net/mailSettings
                        using (var client = new SmtpClient()) { client.Send(msg); }
                    }
                    catch (Exception mailEx)
                    {
                        System.Diagnostics.Trace.TraceError($"SMTP send failed: {mailEx}");
                        // swallow email errors but keep password changed
                    }
                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult ChangePassword()
        {
            if (!SessionManager.IsLogged()) return new HttpStatusCodeResult(401);
            try
            {
                Request.InputStream.Position =0;
                using (var reader = new System.IO.StreamReader(Request.InputStream))
                {
                    var body = reader.ReadToEnd();
                    var obj = Newtonsoft.Json.Linq.JObject.Parse(body);
                    var current = (string)obj["currentPassword"];
                    var newPass = (string)obj["newPassword"];
                    if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(newPass))
                        return Json(new { success = false, error = "Datos inválidos" });

                    var user = SessionManager.GetInstance.User;
                    // validar current contra hash
                    var currentHash = PasswordEncrypter.EncryptPassword(current);
                    if (!_dalUser.userPasswordMatcher(user.Password, currentHash))
                        return Json(new { success = false, error = "Contraseña actual incorrecta" });

                    var ok = _dalUser.UpdatePasswordById(user.id, newPass);
                    if (!ok) return Json(new { success = false, error = "No se pudo actualizar" });

                    try { new BLL_CheckDigitsManager().SetCheckDigits(); } catch { }
                    _dalUser.EventLog(user.id, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Login", "Cambio de contraseña");
                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}