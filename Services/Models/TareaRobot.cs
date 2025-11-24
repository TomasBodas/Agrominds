using System;

namespace Services.Models
{
	public class TareaRobot
	{
		public int Id { get; set; }
		public int IdRobot { get; set; }
		public int IdTipoTarea { get; set; }
		public int IdEstadoTarea { get; set; }
		public DateTime? FechaProgramada { get; set; }
		public DateTime? FechaInicio { get; set; }
		public DateTime? FechaFin { get; set; }
		public string ParametrosJSON { get; set; }
	}
}
