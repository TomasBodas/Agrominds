using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Idioma
{
    public class Translator
    {
        public string GetTraduction(LanguageService pService, string name)
        {
            return pService.SelectedLanguage.ListTranslate.FirstOrDefault(t => t.Name == name).Text;
        }
    }
}
