using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Services; // Para DataBaseServices.getConnectionString()
using Services.Models; // DTOs movidos a Services.Models

namespace DAL
{
	/// <summary>
	/// DAL para gestionar robots y tablas relacionadas.
	/// </summary>
	public class DAL_Robot
	{
		private static readonly string CONNECTION_STRING = DataBaseServices.getConnectionString();

		#region Robot CRUD
		public List<Robot> GetAllRobots()
		{
			var list = new List<Robot>();
			using (var cn = new SqlConnection(CONNECTION_STRING))
			using (var cmd = new SqlCommand("SELECT Id, Nombre, NumeroSerie, Bateria, IdEstadoRobot, FechaAlta FROM robot", cn))
			{
				cn.Open();
				using (var rd = cmd.ExecuteReader())
				{
					while (rd.Read())
					{
						list.Add(new Robot
						{
							Id = rd.GetInt32(0),
							Nombre = rd.IsDBNull(1) ? null : rd.GetString(1),
							NumeroSerie = rd.IsDBNull(2) ? null : rd.GetString(2),
							Bateria = rd.IsDBNull(3) ?0 : rd.GetInt32(3),
							IdEstadoRobot = rd.IsDBNull(4) ?0 : rd.GetInt32(4),
							FechaAlta = rd.IsDBNull(5) ? (DateTime?)null : rd.GetDateTime(5)
						});
					}
				}
			}
			return list;
		}

		public Robot GetRobotById(int id)
		{
			Robot robot = null;
			using (var cn = new SqlConnection(CONNECTION_STRING))
			using (var cmd = new SqlCommand("SELECT Id, Nombre, NumeroSerie, Bateria, IdEstadoRobot, FechaAlta FROM robot WHERE Id=@id", cn))
			{
				cmd.Parameters.AddWithValue("@id", id);
				cn.Open();
				using (var rd = cmd.ExecuteReader())
				{
					if (rd.Read())
					{
						robot = new Robot
						{
							Id = rd.GetInt32(0),
							Nombre = rd.IsDBNull(1) ? null : rd.GetString(1),
							NumeroSerie = rd.IsDBNull(2) ? null : rd.GetString(2),
							Bateria = rd.IsDBNull(3) ?0 : rd.GetInt32(3),
							IdEstadoRobot = rd.IsDBNull(4) ?0 : rd.GetInt32(4),
							FechaAlta = rd.IsDBNull(5) ? (DateTime?)null : rd.GetDateTime(5)
						};
					}
				}
			}
			return robot;
		}

		public int CreateRobot(Robot robot)
		{
			using (var cn = new SqlConnection(CONNECTION_STRING))
			using (var cmd = new SqlCommand(@"INSERT INTO robot (Nombre, NumeroSerie, Bateria, IdEstadoRobot, FechaAlta) 
				VALUES (@Nombre, @NumeroSerie, @Bateria, @Estado, @FechaAlta); SELECT SCOPE_IDENTITY();", cn))
			{
				cmd.Parameters.AddWithValue("@Nombre", (object)robot.Nombre ?? DBNull.Value);
				cmd.Parameters.AddWithValue("@NumeroSerie", (object)robot.NumeroSerie ?? DBNull.Value);
				cmd.Parameters.AddWithValue("@Bateria", robot.Bateria);
				cmd.Parameters.AddWithValue("@Estado", robot.IdEstadoRobot);
				cmd.Parameters.AddWithValue("@FechaAlta", (object)robot.FechaAlta ?? DBNull.Value);
				cn.Open();
				var id = cmd.ExecuteScalar();
				return Convert.ToInt32(id);
			}
		}

		public void UpdateRobot(Robot robot)
		{
			using (var cn = new SqlConnection(CONNECTION_STRING))
			using (var cmd = new SqlCommand(@"UPDATE robot SET Nombre=@Nombre, NumeroSerie=@NumeroSerie, Bateria=@Bateria, IdEstadoRobot=@Estado, FechaAlta=@FechaAlta WHERE Id=@Id", cn))
			{
				cmd.Parameters.AddWithValue("@Id", robot.Id);
				cmd.Parameters.AddWithValue("@Nombre", (object)robot.Nombre ?? DBNull.Value);
				cmd.Parameters.AddWithValue("@NumeroSerie", (object)robot.NumeroSerie ?? DBNull.Value);
				cmd.Parameters.AddWithValue("@Bateria", robot.Bateria);
				cmd.Parameters.AddWithValue("@Estado", robot.IdEstadoRobot);
				cmd.Parameters.AddWithValue("@FechaAlta", (object)robot.FechaAlta ?? DBNull.Value);
				cn.Open();
				cmd.ExecuteNonQuery();
			}
		}

		public void DeleteRobot(int id)
		{
			using (var cn = new SqlConnection(CONNECTION_STRING))
			using (var cmd = new SqlCommand(@"DELETE FROM telemetria_robot WHERE IdRobot=@Id; 
				DELETE FROM mantenimiento_robot WHERE IdRobot=@Id; 
				DELETE FROM tarea_robot WHERE IdRobot=@Id; 
				DELETE FROM robot WHERE Id=@Id;", cn))
			{
				cmd.Parameters.AddWithValue("@Id", id);
				cn.Open();
				cmd.ExecuteNonQuery();
			}
		}

		public void UpdateRobotBattery(int robotId, int bateria)
		{
			using (var cn = new SqlConnection(CONNECTION_STRING))
			using (var cmd = new SqlCommand("UPDATE robot SET Bateria=@Bateria WHERE Id=@Id", cn))
			{
				cmd.Parameters.AddWithValue("@Id", robotId);
				cmd.Parameters.AddWithValue("@Bateria", bateria);
				cn.Open();
				cmd.ExecuteNonQuery();
			}
		}

		public void UpdateRobotEstado(int robotId, int estadoId)
		{
			using (var cn = new SqlConnection(CONNECTION_STRING))
			using (var cmd = new SqlCommand("UPDATE robot SET IdEstadoRobot=@Estado WHERE Id=@Id", cn))
			{
				cmd.Parameters.AddWithValue("@Id", robotId);
				cmd.Parameters.AddWithValue("@Estado", estadoId);
				cn.Open();
				cmd.ExecuteNonQuery();
			}
		}
		#endregion

		#region Estados Robot
		/// <summary>
		/// Obtiene todos los estados de robot (tabla estado_robot) como diccionario Id->Nombre.
		/// </summary>
		public Dictionary<int,string> GetEstadosRobot()
		{
			var dict = new Dictionary<int,string>();
			using (var cn = new SqlConnection(CONNECTION_STRING))
			using (var cmd = new SqlCommand("SELECT Id, Nombre FROM estado_robot", cn))
			{
				cn.Open();
				using (var rd = cmd.ExecuteReader())
				{
					while (rd.Read())
					{
						int id = rd.GetInt32(0);
						string nombre = rd.IsDBNull(1) ? null : rd.GetString(1);
						if (!dict.ContainsKey(id)) dict.Add(id, nombre);
					}
				}
			}
			return dict;
		}
		#endregion

		#region Telemetria
		public void InsertTelemetry(TelemetriaRobot t)
		{
			using (var cn = new SqlConnection(CONNECTION_STRING))
			using (var cmd = new SqlCommand(@"INSERT INTO telemetria_robot (IdRobot, FechaHora, NivelBateria, Temperatura, Estado, DatosJSON) 
				VALUES (@IdRobot, @FechaHora, @NivelBateria, @Temperatura, @Estado, @DatosJSON);", cn))
			{
				cmd.Parameters.AddWithValue("@IdRobot", t.IdRobot);
				cmd.Parameters.AddWithValue("@FechaHora", t.FechaHora);
				cmd.Parameters.AddWithValue("@NivelBateria", (object)t.NivelBateria ?? DBNull.Value);
				cmd.Parameters.AddWithValue("@Temperatura", (object)t.Temperatura ?? DBNull.Value);
				cmd.Parameters.AddWithValue("@Estado", (object)t.Estado ?? DBNull.Value);
				cmd.Parameters.AddWithValue("@DatosJSON", (object)t.DatosJSON ?? DBNull.Value);
				cn.Open();
				cmd.ExecuteNonQuery();
			}
		}

		public List<TelemetriaRobot> GetTelemetry(int robotId, DateTime? from = null, DateTime? to = null, int top =100)
		{
			var list = new List<TelemetriaRobot>();
			var sql = "SELECT TOP (@Top) Id, IdRobot, FechaHora, NivelBateria, Temperatura, Estado, DatosJSON FROM telemetria_robot WHERE IdRobot=@IdRobot";
			if (from.HasValue) sql += " AND FechaHora >= @From";
			if (to.HasValue) sql += " AND FechaHora <= @To";
			sql += " ORDER BY FechaHora DESC";
			using (var cn = new SqlConnection(CONNECTION_STRING))
			using (var cmd = new SqlCommand(sql, cn))
			{
				cmd.Parameters.AddWithValue("@Top", top);
				cmd.Parameters.AddWithValue("@IdRobot", robotId);
				if (from.HasValue) cmd.Parameters.AddWithValue("@From", from.Value);
				if (to.HasValue) cmd.Parameters.AddWithValue("@To", to.Value);
				cn.Open();
				using (var rd = cmd.ExecuteReader())
				{
					while (rd.Read())
					{
						list.Add(new TelemetriaRobot
						{
							Id = rd.GetInt64(0),
							IdRobot = rd.GetInt32(1),
							FechaHora = rd.GetDateTime(2),
							NivelBateria = rd.IsDBNull(3) ? (int?)null : rd.GetInt32(3),
							Temperatura = rd.IsDBNull(4) ? (decimal?)null : rd.GetDecimal(4),
							Estado = rd.IsDBNull(5) ? null : rd.GetString(5),
							DatosJSON = rd.IsDBNull(6) ? null : rd.GetString(6)
						});
					}
				}
			}
			return list;
		}
		#endregion

		#region Tareas
		public int CreateTask(TareaRobot t)
		{
			using (var cn = new SqlConnection(CONNECTION_STRING))
			using (var cmd = new SqlCommand(@"INSERT INTO tarea_robot (IdRobot, IdTipoTarea, IdEstadoTarea, FechaProgramada, FechaInicio, FechaFin, ParametrosJSON) 
				VALUES (@IdRobot, @IdTipoTarea, @IdEstadoTarea, @FechaProgramada, @FechaInicio, @FechaFin, @ParametrosJSON); SELECT SCOPE_IDENTITY();", cn))
			{
				cmd.Parameters.AddWithValue("@IdRobot", t.IdRobot);
				cmd.Parameters.AddWithValue("@IdTipoTarea", t.IdTipoTarea);
				cmd.Parameters.AddWithValue("@IdEstadoTarea", t.IdEstadoTarea);
				cmd.Parameters.AddWithValue("@FechaProgramada", (object)t.FechaProgramada ?? DBNull.Value);
				cmd.Parameters.AddWithValue("@FechaInicio", (object)t.FechaInicio ?? DBNull.Value);
				cmd.Parameters.AddWithValue("@FechaFin", (object)t.FechaFin ?? DBNull.Value);
				cmd.Parameters.AddWithValue("@ParametrosJSON", (object)t.ParametrosJSON ?? DBNull.Value);
				cn.Open();
				var id = cmd.ExecuteScalar();
				return Convert.ToInt32(id);
			}
		}

		public void UpdateTaskStatus(int tareaId, int nuevoEstadoId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
		{
			var sql = "UPDATE tarea_robot SET IdEstadoTarea=@Estado";
			if (fechaInicio.HasValue) sql += ", FechaInicio=@FechaInicio";
			if (fechaFin.HasValue) sql += ", FechaFin=@FechaFin";
			sql += " WHERE Id=@Id";
			using (var cn = new SqlConnection(CONNECTION_STRING))
			using (var cmd = new SqlCommand(sql, cn))
			{
				cmd.Parameters.AddWithValue("@Estado", nuevoEstadoId);
				cmd.Parameters.AddWithValue("@Id", tareaId);
				if (fechaInicio.HasValue) cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.Value);
				if (fechaFin.HasValue) cmd.Parameters.AddWithValue("@FechaFin", fechaFin.Value);
				cn.Open();
				cmd.ExecuteNonQuery();
			}
		}

		public List<TareaRobot> GetTasks(int robotId, int? estadoTareaId = null, int top =200)
		{
			var list = new List<TareaRobot>();
			var sql = "SELECT TOP (@Top) Id, IdRobot, IdTipoTarea, IdEstadoTarea, FechaProgramada, FechaInicio, FechaFin, ParametrosJSON FROM tarea_robot WHERE IdRobot=@IdRobot";
			if (estadoTareaId.HasValue) sql += " AND IdEstadoTarea=@Estado";
			sql += " ORDER BY FechaProgramada DESC";
			using (var cn = new SqlConnection(CONNECTION_STRING))
			using (var cmd = new SqlCommand(sql, cn))
			{
				cmd.Parameters.AddWithValue("@Top", top);
				cmd.Parameters.AddWithValue("@IdRobot", robotId);
				if (estadoTareaId.HasValue) cmd.Parameters.AddWithValue("@Estado", estadoTareaId.Value);
				cn.Open();
				using (var rd = cmd.ExecuteReader())
				{
					while (rd.Read())
					{
						list.Add(new TareaRobot
						{
							Id = rd.GetInt32(0),
							IdRobot = rd.GetInt32(1),
							IdTipoTarea = rd.GetInt32(2),
							IdEstadoTarea = rd.GetInt32(3),
							FechaProgramada = rd.IsDBNull(4) ? (DateTime?)null : rd.GetDateTime(4),
							FechaInicio = rd.IsDBNull(5) ? (DateTime?)null : rd.GetDateTime(5),
							FechaFin = rd.IsDBNull(6) ? (DateTime?)null : rd.GetDateTime(6),
							ParametrosJSON = rd.IsDBNull(7) ? null : rd.GetString(7)
						});
					}
				}
			}
			return list;
		}
		#endregion

		#region Mantenimiento
		/// <summary>
		/// Crea un mantenimiento. Ajustado a columna Descripcion y tipo decimal DuracionHoras.
		/// </summary>
		public int CreateMaintenance(MantenimientoRobot m)
		{
			using (var cn = new SqlConnection(CONNECTION_STRING))
			using (var cmd = new SqlCommand(@"INSERT INTO mantenimiento_robot (IdRobot, IdTipoMantenimiento, IdUsuarioResponsable, Fecha, Descripcion, DuracionHoras, CostoEstimado) 
				VALUES (@IdRobot, @IdTipoMantenimiento, @IdUsuarioResponsable, @Fecha, @Descripcion, @DuracionHoras, @CostoEstimado); SELECT SCOPE_IDENTITY();", cn))
			{
				cmd.Parameters.AddWithValue("@IdRobot", m.IdRobot);
				cmd.Parameters.AddWithValue("@IdTipoMantenimiento", m.IdTipoMantenimiento);
				cmd.Parameters.AddWithValue("@IdUsuarioResponsable", m.IdUsuarioResponsable);
				cmd.Parameters.AddWithValue("@Fecha", m.Fecha);
				cmd.Parameters.AddWithValue("@Descripcion", (object)m.Descripcion ?? DBNull.Value);
				cmd.Parameters.AddWithValue("@DuracionHoras", (object)m.DuracionHoras ?? DBNull.Value);
				cmd.Parameters.AddWithValue("@CostoEstimado", (object)m.CostoEstimado ?? DBNull.Value);
				cn.Open();
				var id = cmd.ExecuteScalar();
				return Convert.ToInt32(id);
			}
		}

		/// <summary>
		/// Cierra/actualiza un mantenimiento. Permite actualizar duraci?n, costo y descripci?n.
		/// </summary>
		public void CloseMaintenance(int mantenimientoId, decimal? duracionHoras = null, decimal? costoEstimado = null, string descripcion = null)
		{
			var setList = new List<string>();
			if (duracionHoras.HasValue) setList.Add(" DuracionHoras=@DuracionHoras");
			if (costoEstimado.HasValue) setList.Add(" CostoEstimado=@CostoEstimado");
			if (descripcion != null) setList.Add(" Descripcion=@Descripcion");
			if (setList.Count ==0) return;
			var sql = "UPDATE mantenimiento_robot SET" + string.Join(",", setList) + " WHERE Id=@Id";
			using (var cn = new SqlConnection(CONNECTION_STRING))
			using (var cmd = new SqlCommand(sql, cn))
			{
				cmd.Parameters.AddWithValue("@Id", mantenimientoId);
				if (duracionHoras.HasValue) cmd.Parameters.AddWithValue("@DuracionHoras", duracionHoras.Value);
				if (costoEstimado.HasValue) cmd.Parameters.AddWithValue("@CostoEstimado", costoEstimado.Value);
				if (descripcion != null) cmd.Parameters.AddWithValue("@Descripcion", descripcion);
				cn.Open();
				cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Obtiene mantenimientos filtrados por robot y rango de fechas.
		/// </summary>
		public List<MantenimientoRobot> GetMaintenances(int robotId, DateTime? from = null, DateTime? to = null, int top =100)
		{
			var list = new List<MantenimientoRobot>();
			var sql = "SELECT TOP (@Top) Id, IdRobot, IdTipoMantenimiento, Fecha, Descripcion, DuracionHoras, CostoEstimado, IdUsuarioResponsable FROM mantenimiento_robot WHERE IdRobot=@IdRobot";
			if (from.HasValue) sql += " AND Fecha >= @From";
			if (to.HasValue) sql += " AND Fecha <= @To";
			sql += " ORDER BY Fecha DESC";
			using (var cn = new SqlConnection(CONNECTION_STRING))
			using (var cmd = new SqlCommand(sql, cn))
			{
				cmd.Parameters.AddWithValue("@Top", top);
				cmd.Parameters.AddWithValue("@IdRobot", robotId);
				if (from.HasValue) cmd.Parameters.AddWithValue("@From", from.Value);
				if (to.HasValue) cmd.Parameters.AddWithValue("@To", to.Value);
				cn.Open();
				using (var rd = cmd.ExecuteReader())
				{
					while (rd.Read())
					{
						list.Add(new MantenimientoRobot
						{
							Id = rd.GetInt32(0),
							IdRobot = rd.GetInt32(1),
							IdTipoMantenimiento = rd.GetInt32(2),
							Fecha = rd.GetDateTime(3),
							Descripcion = rd.IsDBNull(4) ? null : rd.GetString(4),
							DuracionHoras = rd.IsDBNull(5) ? (decimal?)null : rd.GetDecimal(5),
							CostoEstimado = rd.IsDBNull(6) ? (decimal?)null : rd.GetDecimal(6),
							IdUsuarioResponsable = rd.IsDBNull(7) ?0 : rd.GetInt32(7)
						});
					}
				}
			}
			return list;
		}
		#endregion
	}
}
