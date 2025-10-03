using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
    public class Log
    {
        public int id { get; set; }
        public string User { get; set; }
        public DateTime Fecha { get; set; }
        public string Modulo { get; set; }
        public string Descripcion { get; set; }
    }
}
