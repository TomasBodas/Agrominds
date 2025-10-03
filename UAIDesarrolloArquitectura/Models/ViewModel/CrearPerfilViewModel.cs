using Services.Perfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAIDesarrolloArquitectura.Models.ViewModel
{
    public class CrearPerfilViewModel
    {
        public List<Role> Roles { get; set; }
        public List<Permission> Permisos { get; set; }
    }
    public class PerfilViewModel
    {
        public string NombrePerfil { get; set; }
        public List<PermisoViewModel> Permisos { get; set; }
    }
    public class PermisoViewModel
    {
        public string Nombre { get; set; }
    }
}