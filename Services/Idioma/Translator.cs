using System;
using System.Linq;

namespace Services.Idioma
{
    public class Translator
    {
        public string GetTraduction(LanguageService pService, string name)
        {
            if (pService == null || pService.SelectedLanguage == null)
                return "NOWORD";
            var list = pService.SelectedLanguage.ListTranslate;
            if (list == null)
                return "NOWORD";
            var item = list.FirstOrDefault(t => t != null && t.Name == name);
            var text = item?.Text;
            return string.IsNullOrEmpty(text) ? "NOWORD" : text;
        }
    }
}
