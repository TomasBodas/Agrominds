using BLL;
using DAL;
using Services;
using Services.Models;
using Services.Perfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAIDesarrolloArquitectura.Models.ViewModel;
using Services.Email; // agregado
using System.Security.Cryptography;
using System.Text;

namespace UAIDesarrolloArquitectura.Controllers
{
    public class LoginController : Controller
    {
        private readonly DAL_User _dalUser = new DAL_User();
        private readonly EmailService _emailService = new EmailService();

        private static readonly string ResetSecret = System.Configuration.ConfigurationManager.AppSettings["ResetSecret"] ?? "changeme-secret";
        private static readonly TimeSpan ResetTokenTtl = TimeSpan.FromMinutes(30);

        private static long GetUnixTimeSeconds()
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(DateTime.UtcNow - epoch).TotalSeconds;
        }
        private static DateTime FromUnixTimeSeconds(long seconds)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(seconds);
        }

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
        public async System.Threading.Tasks.Task<ActionResult> ForgotPassword()
        {
            try
            {
                Request.InputStream.Position =0;
                using (var reader = new System.IO.StreamReader(Request.InputStream))
                {
                    var body = reader.ReadToEnd();
                    var email = Newtonsoft.Json.Linq.JObject.Parse(body).Value<string>("email");
                    if (string.IsNullOrWhiteSpace(email)) return Json(new { success = false, error = "Email requerido" });

                    // Validar que exista usuario
                    var emailHash = PasswordEncrypter.EncryptData(email);
                    var user = _dalUser.findByEmail(emailHash);
                    if (user == null) return Json(new { success = false, error = "Usuario no encontrado" });

                    // Generar token firmado (email | ts | signature)
                    var ts = GetUnixTimeSeconds().ToString();
                    var payload = email + "|" + ts;
                    var sig = ComputeHmac(payload, ResetSecret);
                    var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload + "|" + sig));
                    var resetUrl = Url.Action("ConfirmReset", "Login", new { t = token }, Request.Url.Scheme);

                    // Enviar email con link
                    bool sent = false;
                    try
                    {
                        var html = $"<p>Solicitaste recuperar tu contraseña.</p><p>Haz click en el siguiente enlace para confirmar y establecer una nueva:</p><p><a href=\"{resetUrl}\">Recuperar contraseña</a></p><p>Este enlace vence en30 minutos.</p>";
                        sent = await _emailService.SendEmailAsync(email, "Recuperación de contraseña", html).ConfigureAwait(false);
                    }
                    catch (Exception mailEx)
                    {
                        System.Diagnostics.Trace.TraceError($"MailKit send failed: {mailEx}");
                    }
                    return Json(new { success = true, emailSent = sent });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult ConfirmReset(string t)
        {
            var info = ValidateResetToken(t);
            if (info == null) return View("CorruptDatabaseMessage"); // vista genérica de error
            // Mostrar formulario para nueva contraseña
            ViewBag.Email = info.Item1;
            ViewBag.Token = t;
            return View("LoginResetPassword"); // crear vista simple si no existe
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
                    // Email de confirmación (no bloqueante, y log de resultado)
                    bool sent = false;
                    try { sent = _emailService.SendPasswordChangedAsync(user.Email, user.Name).Result; } catch { }
                    try { _dalUser.EventLog(user.id, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Login", sent ? "Email cambio de contraseña enviado" : "Email cambio de contraseña falló"); } catch { }
                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult CompleteReset(string t, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword)) return Json(new { success = false, error = "Nueva contraseña requerida" });
            var info = ValidateResetToken(t);
            if (info == null) return Json(new { success = false, error = "Token inválido o vencido" });
            var email = info.Item1;
            var emailHash = PasswordEncrypter.EncryptData(email);
            var user = _dalUser.findByEmail(emailHash);
            if (user == null) return Json(new { success = false, error = "Usuario no encontrado" });
            var ok = _dalUser.UpdatePasswordByEmail(email, newPassword);
            if (!ok) return Json(new { success = false, error = "No se pudo actualizar" });
            try { new BLL_CheckDigitsManager().SetCheckDigits(); } catch { }
            _dalUser.EventLog(user.id, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Login", "Recuperación de contraseña confirmada");
            try { new BLL_CheckDigitsManager().SetCheckDigits(); } catch { }
            return Json(new { success = true });
        }

        private static string ComputeHmac(string data, string secret)
        {
            using (var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                return Convert.ToBase64String(h.ComputeHash(Encoding.UTF8.GetBytes(data)));
            }
        }

        // returns (email, timestamp) if valid; otherwise null
        private Tuple<string, string> ValidateResetToken(string token)
        {
            try
            {
                var raw = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = raw.Split('|');
                if (parts.Length !=3) return null;
                var email = parts[0];
                var tsStr = parts[1];
                var sig = parts[2];
                var expected = ComputeHmac(email + "|" + tsStr, ResetSecret);
                if (!string.Equals(sig, expected, StringComparison.Ordinal)) return null;
                long ts;
                if (!long.TryParse(tsStr, out ts)) return null;
                var issued = FromUnixTimeSeconds(ts);
                if (DateTime.UtcNow - issued > ResetTokenTtl) return null;
                return Tuple.Create(email, tsStr);
            }
            catch { return null; }
        }
    }
}