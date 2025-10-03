using Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAIDesarrolloArquitectura.Models.ViewModel
{
    public class PerfilesUsuariosViewModel
    {
        public List<string> Perfiles { get; set; }
        public List<User> Usuarios { get; set; }
    }
}