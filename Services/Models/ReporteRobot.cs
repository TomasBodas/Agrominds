using System;

namespace Services.Models
{
	public class ReporteRobot
	{
		public int Id { get; set; }
		public int IdRobot { get; set; }
		public string NombreRobot { get; set; }
		public DateTime FechaGeneracion { get; set; }
		public string Tipo { get; set; } // Ej: "Uso", "Mantenimiento", "Bater?a"
		public string Resumen { get; set; }
		public int? DuracionHorasTareas { get; set; }
		public int? CantidadTareas { get; set; }
		public int? CantidadMantenimientos { get; set; }
		public decimal? CostoMantenimientos { get; set; }
		public int? BateriaPromedio { get; set; }
	}
}