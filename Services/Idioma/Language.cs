using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Idioma
{
    public class Language
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Translate> ListTranslate { get; set; }
        public Language(object[] itemarray)
        {
            Id = int.Parse(itemarray[0].ToString());
            Name = itemarray[1].ToString();
            ListTranslate = new List<Translate>();
        }
    }
}
