using DAL;
using Services.Idioma;
using Services;
using System.Collections.Generic;
using System.Web.Mvc;
using UAIDesarrolloArquitectura.Models;
using BLL;
using System;

namespace UAIDesarrolloArquitectura.Controllers
{
    public class IdiomaController : Controller
    {
        DAL_Language dal_language = new DAL_Language();
        // GET: Idioma
        static Services.LanguageService service;

        public void InitializeController()
        {
            if (service == null)
            {
                service = new Services.LanguageService();
                DAL_Language dal = new DAL_Language();
                List<Language> aux = dal.GetLanguages();
                service.InitializeService(aux);
            }
        }

        public LanguageService GetService()
        {
            return service;
        }

        // POST: Language
        [HttpPost]
        public RedirectResult SetLanguage(LanguageDTO data)
        {
            string valorSeleccionado = data.Valor;
            string urlActual = data.Url;

            // Asegurar redireccion valida: usar ruta relativa del referrer si es absoluta
            if (!string.IsNullOrEmpty(urlActual))
            {
                Uri uri;
                if (Uri.TryCreate(urlActual, UriKind.Absolute, out uri))
                {
                    // Conservar path, query y fragmento
                    urlActual = uri.PathAndQuery + uri.Fragment;
                }
            }
            else
            {
                urlActual = "/"; // fallback
            }

            BLL_CheckDigitsManager checkDigitsManager = new BLL_CheckDigitsManager();

            if (SessionManager.IsLogged())
            {
                SessionManager.GetInstance.User.LanguageId = int.Parse(valorSeleccionado);
                DAL_User dal_user = new DAL_User();
                dal_user.UpdateUser(SessionManager.GetInstance.User, true);
                checkDigitsManager.SetCheckDigits();
            }

            service.ChangeLanguage(valorSeleccionado);
            return Redirect(urlActual);
        }

        public ActionResult ABMIdioma()
        {
            if (!SessionManager.IsLogged())
            {
                return RedirectToAction("Login", "Login");
            }
            // Solo usuario webmaster por Id (14)
            var u = SessionManager.GetInstance.User;
            if (u == null || u.id !=14)
            {
                return RedirectToAction("Index", "Home");
            }
            List<Language> langList = dal_language.GetLanguages();
            return View(langList);
        }

        [HttpPost]
        public ActionResult AddLanguage(string name)
        {
            // Solo usuario Id14
            if (!SessionManager.IsLogged() || SessionManager.GetInstance.User?.id !=14)
            {
                return new HttpStatusCodeResult(403);
            }
            List<Language> langList = dal_language.GetLanguages();
            List<string> tags = new List<string>();

            foreach (Translate tr in langList[0].ListTranslate)
            {
                tags.Add(tr.Name);
            }

            dal_language.AddLanguage(name);
            int id = dal_language.GetLastId();
            dal_language.AddTags(id, tags);

            List<int> ids = new List<int>();
            foreach (Language l in langList)
            {
                ids.Add(l.Id);
            }

            ids.Add(id);
            dal_language.AddTagForItself(ids, name);
            langList = dal_language.GetLanguages();

            return View("ABMIdioma", langList);
        }

        [HttpPost]
        public ActionResult AddWord(string tag)
        {
            // Solo usuario Id14
            if (!SessionManager.IsLogged() || SessionManager.GetInstance.User?.id !=14)
            {
                return new HttpStatusCodeResult(403);
            }
            if (string.IsNullOrWhiteSpace(tag))
            {
                ModelState.AddModelError("tag", "Tag requerido");
            }
            else
            {
                List<Language> langList = dal_language.GetLanguages();
                List<int> ids = new List<int>();
                foreach (Language l in langList) ids.Add(l.Id);
                dal_language.AddTagForItself(ids, tag.Trim());
            }
            return View("ABMIdioma", dal_language.GetLanguages());
        }

        [HttpPost]
        public ActionResult UpdateTranslate(int id, string tag, string text)
        {
            // Solo usuario Id14
            if (!SessionManager.IsLogged() || SessionManager.GetInstance.User?.id !=14)
            {
                return new HttpStatusCodeResult(403);
            }
            dal_language.ModifyTranslate(id, tag, text);
            List<Language> langList = dal_language.GetLanguages();
            return View("ABMIdioma", langList);
        }

        [HttpPost]
        public ActionResult UpdatePage(int id, string tag)
        {
            if (!SessionManager.IsLogged())
            {
                return RedirectToAction("Login", "Login");
            }

            // Solo usuario Id14
            if (SessionManager.GetInstance.User?.id ==14)
            {
                List<Language> langList = dal_language.GetLanguages();
                Language lang = langList.Find(x => x.Id == id);
                Translate tran = lang.ListTranslate.Find(x => x.Name == tag);

                ViewBag.tran = tran;
                ViewBag.lan = lang;

                return View(tran);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
