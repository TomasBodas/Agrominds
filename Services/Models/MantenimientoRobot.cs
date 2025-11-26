using System;

namespace Services.Models
{
	public class MantenimientoRobot
	{
		public int Id { get; set; }
		public int IdRobot { get; set; }
		public int IdTipoMantenimiento { get; set; }
		public int IdUsuarioResponsable { get; set; }
		public DateTime Fecha { get; set; }
		// Ajuste: en la base es decimal(5,2)
		public decimal? DuracionHoras { get; set; }
		// decimal(18,2)
		public decimal? CostoEstimado { get; set; }
		// En la base el campo se llama Descripcion (varchar500)
		public string Descripcion { get; set; }
		// Nuevo flag de cierre (bit en DB). True = cerrado, False = abierto.
		public bool Cerrado { get; set; }
		// Alias previo para compatibilidad con vistas antiguas
		public string Observaciones
		{
			get => Descripcion;
			set => Descripcion = value;
		}
	}
}
