using System;

namespace Services.Models
{
	public class Robot
	{
		public int Id { get; set; }
		public string Nombre { get; set; }
		public string NumeroSerie { get; set; }
		public int Bateria { get; set; }
		public int IdEstadoRobot { get; set; }
		public DateTime? FechaAlta { get; set; }
		public DateTime? UltimaConexion { get; set; } // Nueva propiedad
		public decimal? Latitud { get; set; } // Nueva propiedad
		public decimal? Longitud { get; set; } // Nueva propiedad
		public int IdUsuarioResponsable { get; set; }
	}
}
