using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Idioma
{
    public class Translate
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public int Id { get; set; }
        public Translate(object[] itemarray)
        {
            Id = int.Parse(itemarray[0].ToString());
            Name = itemarray[1].ToString();
            Text = itemarray[2].ToString();
        }
    }
}
