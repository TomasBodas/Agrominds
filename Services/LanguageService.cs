using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Idioma;

namespace Services
{
    public class LanguageService
    {
        static List<Language> Languages;
        public Language SelectedLanguage;
        static List<Translator> Translators;
        public LanguageService()
        {
            if (Translators == null) Translators = new List<Translator>();
            if (Languages == null) Languages = new List<Language>();
        }
        public bool SelectedLanguageExists() { if (SelectedLanguage != null) return true; return false; }
        public void Susbcribe(Translator translator) { Translators.Add(translator); }
        public void Unsusbcribe(Translator translator) { Translators.Remove(translator); }
        public void InitializeService(List<Language> languages)
        {
            Languages = languages;
        }
        public List<Language> GetLanguages()
        {
            return Languages;
        }
        public void ChangeLanguage(string languageId)
        {
            if (int.Parse(languageId) != 0)
            {
                SelectedLanguage = Languages.Find(x => x.Id == int.Parse(languageId));
            }
        }
    }
}
