using System;
using System.Collections.Generic;
using DAL;
using Services.Models;
using Services;

namespace BLL
{
	/// <summary>
	/// Capa de negocio para Robots y Tareas.
	/// </summary>
	public class BLL_Robot
	{
		private readonly DAL_Robot _dal = new DAL_Robot();
		private readonly DAL_User _dalUser = new DAL_User();
		private readonly BLL_CheckDigitsManager _dvManager = new BLL_CheckDigitsManager();

		// Estados de tarea definidos por el usuario
		public const int EstadoTarea_Pending =1;
		public const int EstadoTarea_InProgress =2;
		public const int EstadoTarea_Finished =3;
		public const int EstadoTarea_Cancelled =4;

		// Actualizado:1=Activo,3=Inactivo. Permitimos toggle directo1<->3 más otros existentes.
		private static readonly Dictionary<int, int[]> _allowedTransitions = new Dictionary<int, int[]>()
		{
			{1, new[]{3,2}},
			{3, new[]{1}},
			{2, new[]{1,4}},
			{4, new[]{1}}
		};

		public int CrearRobot(Robot robot, int usuarioId =0)
		{
			ValidarRobot(robot);
			robot.FechaAlta = robot.FechaAlta ?? DateTime.UtcNow;
			// Setear el usuario responsable al creador
			robot.IdUsuarioResponsable = usuarioId;
			int id = _dal.CreateRobot(robot);
			RegistrarLog(usuarioId, "Robot", $"Se creó robot #{id} ({robot.Nombre})");
			RecalcularDigitosVerificadores();
			return id;
		}

		public void ActualizarRobot(Robot robot, int usuarioId =0)
		{
			ValidarRobot(robot, true);
			_dal.UpdateRobot(robot);
			RegistrarLog(usuarioId, "Robot", $"Se actualizó robot #{robot.Id}");
			RecalcularDigitosVerificadores();
		}

		public void EliminarRobot(int robotId, int usuarioId =0)
		{
			_dal.DeleteRobot(robotId);
			RegistrarLog(usuarioId, "Robot", $"Se eliminó robot #{robotId}");
			RecalcularDigitosVerificadores();
		}

		public void ActualizarBateria(int robotId, int nuevaBateria, int usuarioId =0)
		{
			if (nuevaBateria <0 || nuevaBateria >100) throw new ArgumentOutOfRangeException(nameof(nuevaBateria), "Batería fuera de rango (0-100)");
			_dal.UpdateRobotBattery(robotId, nuevaBateria);
			RegistrarLog(usuarioId, "Robot", $"Actualizada batería robot #{robotId} a {nuevaBateria}%");
			RecalcularDigitosVerificadores();
		}

		public void CambiarEstado(int robotId, int nuevoEstado, int usuarioId =0)
		{
			var robot = _dal.GetRobotById(robotId) ?? throw new InvalidOperationException("Robot no encontrado");
			if (!_allowedTransitions.ContainsKey(robot.IdEstadoRobot)) throw new InvalidOperationException("Transiciones no definidas para el estado actual");
			if (Array.IndexOf(_allowedTransitions[robot.IdEstadoRobot], nuevoEstado) <0)
				throw new InvalidOperationException("Transición de estado no permitida");
			_dal.UpdateRobotEstado(robotId, nuevoEstado);
			if (nuevoEstado ==3)
			{
				_dal.UpdateRobotUltimaConexion(robotId, DateTime.Now);
			}
			RegistrarLog(usuarioId, "Robot", $"Estado robot #{robotId} {robot.IdEstadoRobot} -> {nuevoEstado}");
			RecalcularDigitosVerificadores();
		}

		public int ProgramarTarea(TareaRobot tarea, int usuarioId =0)
		{
			ValidarTarea(tarea);
			DetectarSolapamientos(tarea);
			int id = _dal.CreateTask(tarea);
			RegistrarLog(usuarioId, "TareaRobot", $"Se programó tarea #{id} para robot #{tarea.IdRobot}");
			RecalcularDigitosVerificadores();
			return id;
		}

		public void ActualizarEstadoTarea(int tareaId, int nuevoEstado, DateTime? inicio = null, DateTime? fin = null, int usuarioId =0)
		{
			_dal.UpdateTaskStatus(tareaId, nuevoEstado, inicio, fin);
			RegistrarLog(usuarioId, "TareaRobot", $"Actualizado estado tarea #{tareaId} a {nuevoEstado}");
			RecalcularDigitosVerificadores();
		}

		// Auto-inicio: tareas en estado Pending cuya FechaProgramada llegó
		public List<TareaRobot> ObtenerTareasParaAutoInicio(DateTime ahoraLocal, int top =100)
			=> _dal.GetTasksToAutoStart(EstadoTarea_Pending, ahoraLocal, top);

		public void IniciarTarea(int tareaId)
			=> _dal.UpdateTaskStatus(tareaId, EstadoTarea_InProgress, DateTime.Now, null); // cambiado a hora local

		// Auto-finalización: tareas InProgress cuya FechaFin (planificada) llegó
		public List<TareaRobot> ObtenerTareasParaAutoFin(DateTime ahoraLocal, int top =100)
			=> _dal.GetTasksToAutoFinish(EstadoTarea_InProgress, ahoraLocal, top);

		public void FinalizarTarea(int tareaId)
		{
			_dal.UpdateTaskStatus(tareaId, EstadoTarea_Finished, null, DateTime.Now);
			AgregarTelemetriaFinTarea(tareaId);
		}

		public bool FinalizarTareaAuto(int tareaId)
		{
			var tarea = _dal.GetTaskById(tareaId);
			if (tarea == null) return false;
			if (tarea.IdEstadoTarea != EstadoTarea_InProgress) return false;
			_dal.UpdateTaskStatus(tareaId, EstadoTarea_Finished, null, DateTime.Now);
			AgregarTelemetriaFinTarea(tareaId, tarea);
			RegistrarLog(0, "TareaRobot", $"Auto fin tarea #{tareaId}");
			RecalcularDigitosVerificadores();
			return true;
		}

		private void AgregarTelemetriaFinTarea(int tareaId, TareaRobot tareaLoaded = null)
		{
			var tarea = tareaLoaded ?? _dal.GetTaskById(tareaId);
			if (tarea == null) return;
			var tele = new TelemetriaRobot
			{
				IdRobot = tarea.IdRobot,
				FechaHora = DateTime.Now,
				Estado = "TaskFinished",
				DatosJSON = $"{{\"taskId\":{tareaId},\"finishedAt\":\"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\"}}"
			};
			try { _dal.InsertTelemetry(tele); } catch { }
		}

		public Dictionary<int,string> ObtenerEstadosTarea() => _dal.GetEstadosTarea();

		public void ActualizarTareaDatos(TareaRobot t, int usuarioId =0)
		{
			if (t == null) throw new ArgumentNullException("t");
			if (t.Id <=0) throw new ArgumentException("Id de tarea inválido");
			_dal.UpdateTaskData(t.Id, t.FechaProgramada, t.ParametrosJSON, t.Observaciones, t.FechaInicio, t.FechaFin);
			RegistrarLog(usuarioId, "TareaRobot", $"Actualizados datos tarea #{t.Id}");
			RecalcularDigitosVerificadores();
		}

		public void RegistrarTelemetria(TelemetriaRobot telemetria, bool actualizarBateria = true, int usuarioId =0)
		{
			if (telemetria == null) throw new ArgumentNullException(nameof(telemetria));
			if (telemetria.FechaHora == default) telemetria.FechaHora = DateTime.UtcNow;
			_dal.InsertTelemetry(telemetria);
			// tras escribir en telemetria_robot recalcular DV
			RecalcularDigitosVerificadores();
			if (actualizarBateria && telemetria.NivelBateria.HasValue)
			{
				ActualizarBateria(telemetria.IdRobot, telemetria.NivelBateria.Value, usuarioId);
			}
			if (telemetria.NivelBateria.HasValue && telemetria.NivelBateria.Value <=15)
			{
				RegistrarLog(usuarioId, "Telemetria", $"Robot #{telemetria.IdRobot} batería baja ({telemetria.NivelBateria.Value}%)");
			}
		}

		public List<TelemetriaRobot> ObtenerTelemetria(int robotId, DateTime? desde = null, DateTime? hasta = null, int top =100)
			=> _dal.GetTelemetry(robotId, desde, hasta, top);

		public List<TareaRobot> ObtenerTareas(int robotId, int? estado = null, int top =200)
			=> _dal.GetTasks(robotId, estado, top);

		public List<MantenimientoRobot> ObtenerMantenimientos(int robotId, DateTime? desde = null, DateTime? hasta = null, int top =100)
			=> _dal.GetMaintenances(robotId, desde, hasta, top);

		public int RegistrarMantenimiento(MantenimientoRobot m, int usuarioId =0)
		{
			ValidarMantenimiento(m);
			int id = _dal.CreateMaintenance(m);
			RegistrarLog(usuarioId, "MantenimientoRobot", $"Mantenimiento #{id} creado para robot #{m.IdRobot}");
			RecalcularDigitosVerificadores();
			return id;
		}

		public void CerrarMantenimiento(int mantenimientoId, int? duracionHoras = null, decimal? costoEstimado = null, string observaciones = null, int usuarioId =0)
		{
			_dal.CloseMaintenance(mantenimientoId, duracionHoras, costoEstimado, observaciones);
			RegistrarLog(usuarioId, "MantenimientoRobot", $"Mantenimiento #{mantenimientoId} cerrado");
			RecalcularDigitosVerificadores();
		}

		public List<Robot> GetAllRobots() => _dal.GetAllRobots();
		public Robot GetRobotById(int id) => _dal.GetRobotById(id);
		public Dictionary<int, string> GetEstadosRobot() => _dal.GetEstadosRobot();

		#region Validaciones
		private void ValidarRobot(Robot r, bool update = false)
		{
			if (r == null) throw new ArgumentNullException(nameof(r));
			if (!update && r.Id !=0) throw new InvalidOperationException("El Id debe ser0 al crear");
			if (string.IsNullOrWhiteSpace(r.Nombre)) throw new ArgumentException("Nombre requerido");
			if (string.IsNullOrWhiteSpace(r.NumeroSerie)) throw new ArgumentException("NumeroSerie requerido");
			if (r.Bateria <0 || r.Bateria >100) throw new ArgumentOutOfRangeException("Batería fuera de rango");
		}

		private void ValidarTarea(TareaRobot t)
		{
			if (t == null) throw new ArgumentNullException(nameof(t));
			if (t.IdRobot <=0) throw new ArgumentException("Robot inválido");
			if (t.IdTipoTarea <=0) throw new ArgumentException("Tipo de tarea inválido");
			if (t.IdEstadoTarea <=0) throw new ArgumentException("Estado de tarea inválido");
			if (t.FechaProgramada.HasValue && t.FechaFin.HasValue && t.FechaProgramada > t.FechaFin)
				throw new ArgumentException("Fecha programada posterior a Fecha Fin");
		}

		private void ValidarMantenimiento(MantenimientoRobot m)
		{
			if (m == null) throw new ArgumentNullException(nameof(m));
			if (m.IdRobot <=0) throw new ArgumentException("Robot inválido");
			if (m.IdTipoMantenimiento <=0) throw new ArgumentException("Tipo mantenimiento inválido");
			if (m.Fecha == default) m.Fecha = DateTime.UtcNow;
			if (m.DuracionHoras.HasValue && m.DuracionHoras <0) throw new ArgumentException("Duración negativa");
			if (m.CostoEstimado.HasValue && m.CostoEstimado <0) throw new ArgumentException("Costo negativo");
		}

		private void DetectarSolapamientos(TareaRobot nueva)
		{
			if (!nueva.FechaProgramada.HasValue || !nueva.FechaFin.HasValue) return;
			var existentes = _dal.GetTasks(nueva.IdRobot, null,500);
			foreach (var t in existentes)
			{
				if (!t.FechaProgramada.HasValue || !t.FechaFin.HasValue) continue;
				bool solapa = nueva.FechaProgramada < t.FechaFin && nueva.FechaFin > t.FechaProgramada;
				if (solapa) throw new InvalidOperationException($"La nueva tarea se solapa con tarea #{t.Id}");
			}
		}
		#endregion

		#region Auxiliares
		private void RegistrarLog(int usuarioId, string modulo, string descripcion)
		{
			try { _dalUser.EventLog(usuarioId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), modulo, descripcion); } catch { }
		}

		private void RecalcularDigitosVerificadores()
		{
			try { _dvManager.SetCheckDigits(); } catch { }
		}
		#endregion

		public bool IniciarTareaSiRobotActivo(int tareaId)
		{
			var tarea = _dal.GetTaskById(tareaId);
			if (tarea == null) return false;
			if (tarea.IdEstadoTarea != EstadoTarea_Pending) return false;
			var robot = _dal.GetRobotById(tarea.IdRobot);
			if (robot == null || robot.IdEstadoRobot !=1) return false; // robot inactivo
			_dal.UpdateTaskStatus(tareaId, EstadoTarea_InProgress, DateTime.Now, null); // hora local
			RegistrarLog(0, "TareaRobot", $"Auto inicio tarea #{tareaId}");
			RecalcularDigitosVerificadores();
			return true;
		}

		public List<Robot> GetRobotsForCurrentUser()
		{
			if (!SessionManager.IsLogged()) return new List<Robot>();
			var uid = SessionManager.GetInstance.User.id;
			return _dal.GetRobotsByUser(uid);
		}

		public List<Robot> GetRobotsForUser(int userId)
		{
			return _dal.GetRobotsByUser(userId);
		}
	}
}
