using System;

namespace Services.Models
{
	public class TelemetriaRobot
	{
		// Id bigint en la base
		public long Id { get; set; }
		public int IdRobot { get; set; }
		public DateTime FechaHora { get; set; }
		// Nueva columna NivelBateria (antes Bateria). Se mantiene alias Bateria para compatibilidad.
		public int? NivelBateria { get; set; }
		public int? Bateria
		{
			get => NivelBateria;
			set => NivelBateria = value;
		}
		// Nueva columna Temperatura decimal(5,2)
		public decimal? Temperatura { get; set; }
		// Nueva columna Estado (varchar50)
		public string Estado { get; set; }
		public string DatosJSON { get; set; }
	}
}
