using System;
using System.Linq;
using System.Web.Mvc;
using BLL;
using Services.Models;

namespace UAIDesarrolloArquitectura.Controllers
{
	public class RobotController : Controller
	{
		private readonly BLL_Robot _bll = new BLL_Robot();

		private bool HasRobotPermission()
		{
			return Services.SessionManager.IsLogged() &&
				Services.SessionManager.GetInstance.User.permissionList.Any(p => p == "1004");
		}

		private ActionResult ForbidIfNoPermission()
		{
			if (!HasRobotPermission())
			{
				return new HttpStatusCodeResult(403, "Permiso insuficiente");
			}
			return null;
		}

		// GET: /Robot
		[HttpGet]
		public ActionResult Index()
		{
			var forbid = ForbidIfNoPermission(); if (forbid != null) return forbid;
			// Obtiene robots y estados v?a BLL
			var robots = _bll.GetRobotsForCurrentUser();
			var estados = _bll.GetEstadosRobot();
			ViewBag.EstadosRobot = estados; // disponible en la vista
			return View("Robots", robots);
		}

		// GET: /Robot/Details/5
		[HttpGet]
		public ActionResult Details(int id)
		{
			var forbid = ForbidIfNoPermission(); if (forbid != null) return forbid;
			var robots = _bll.GetRobotsForCurrentUser();
			if (robots.All(r => r.Id != id)) return new HttpStatusCodeResult(403, "Acceso denegado");
			var robot = robots.FirstOrDefault(r => r.Id == id);
			if (robot == null) return HttpNotFound();
			ViewBag.EstadosRobot = _bll.GetEstadosRobot();
			ViewBag.RobotTasks = _bll.ObtenerTareas(id, null,200); // lista de tareas para la vista detalle
			ViewBag.EstadosTarea = _bll.ObtenerEstadosTarea();
			return View("RobotDetalle", robot);
		}

		// POST: /Robot/Create
		[HttpPost]
		public ActionResult Create(Robot robot)
		{
			try
			{
				var forbid = ForbidIfNoPermission(); if (forbid != null) return forbid;
				int userId = Services.SessionManager.IsLogged() ? Services.SessionManager.GetInstance.User.id :0;
				int id = _bll.CrearRobot(robot, userId);
				return Json(new { success = true, id });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}

		// POST: /Robot/Update
		[HttpPost]
		public ActionResult Update(Robot robot)
		{
			try
			{
				var forbid = ForbidIfNoPermission(); if (forbid != null) return forbid;
				int userId = Services.SessionManager.IsLogged() ? Services.SessionManager.GetInstance.User.id :0;
				_bll.ActualizarRobot(robot, userId);
				return Json(new { success = true });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}

		// POST: /Robot/Delete/5
		[HttpPost]
		public ActionResult Delete(int id)
		{
			try
			{
				var forbid = ForbidIfNoPermission(); if (forbid != null) return forbid;
				int userId = Services.SessionManager.IsLogged() ? Services.SessionManager.GetInstance.User.id :0;
				_bll.EliminarRobot(id, userId);
				return Json(new { success = true });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}

		// POST: /Robot/UpdateBattery
		[HttpPost]
		public ActionResult UpdateBattery(int id, int battery)
		{
			try
			{
				var forbid = ForbidIfNoPermission(); if (forbid != null) return forbid;
				int userId = Services.SessionManager.IsLogged() ? Services.SessionManager.GetInstance.User.id :0;
				_bll.ActualizarBateria(id, battery, userId);
				return Json(new { success = true });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}

		// POST: /Robot/ChangeState
		[HttpPost]
		public ActionResult ChangeState(int id, int newState)
		{
			try
			{
				var forbid = ForbidIfNoPermission(); if (forbid != null) return forbid;
				int userId = Services.SessionManager.IsLogged() ? Services.SessionManager.GetInstance.User.id :0;
				_bll.CambiarEstado(id, newState, userId);
				return Json(new { success = true });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}

		// POST: /Robot/ScheduleTask
		[HttpPost]
		public ActionResult ScheduleTask(TareaRobot tarea)
		{
			try
			{
				var forbid = ForbidIfNoPermission(); if (forbid != null) return forbid;
				int userId = Services.SessionManager.IsLogged() ? Services.SessionManager.GetInstance.User.id :0;
				int id = _bll.ProgramarTarea(tarea, userId);
				return Json(new { success = true, id });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}

		// POST: /Robot/UpdateTaskState
		[HttpPost]
		public ActionResult UpdateTaskState(int taskId, int newState, DateTime? start = null, DateTime? end = null)
		{
			try
			{
				var forbid = ForbidIfNoPermission(); if (forbid != null) return forbid;
				int userId = Services.SessionManager.IsLogged() ? Services.SessionManager.GetInstance.User.id :0;
				_bll.ActualizarEstadoTarea(taskId, newState, start, end, userId);
				return Json(new { success = true });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}

		// POST: /Robot/RegisterTelemetry
		[HttpPost]
		public ActionResult RegisterTelemetry(TelemetriaRobot telemetria)
		{
			try
			{
				var forbid = ForbidIfNoPermission(); if (forbid != null) return forbid;
				int userId = Services.SessionManager.IsLogged() ? Services.SessionManager.GetInstance.User.id :0;
				_bll.RegistrarTelemetria(telemetria, true, userId);
				return Json(new { success = true });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}

		// GET: /Robot/Telemetry
		[HttpGet]
		public ActionResult Telemetry(int id, DateTime? from = null, DateTime? to = null, int top =100)
		{
			var forbid = ForbidIfNoPermission(); if (forbid != null) return forbid;
			var data = _bll.ObtenerTelemetria(id, from, to, top);
			return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
		}

		// POST: /Robot/RegisterMaintenance
		[HttpPost]
		public ActionResult RegisterMaintenance(MantenimientoRobot m)
		{
			try
			{
				var forbid = ForbidIfNoPermission(); if (forbid != null) return forbid;
				int userId = Services.SessionManager.IsLogged() ? Services.SessionManager.GetInstance.User.id :0;
				int id = _bll.RegistrarMantenimiento(m, userId);
				return Json(new { success = true, id });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}

		// POST: /Robot/CloseMaintenance
		[HttpPost]
		public ActionResult CloseMaintenance(int maintenanceId, int? duracionHoras = null, decimal? costoEstimado = null, string observaciones = null)
		{
			try
			{
				var forbid = ForbidIfNoPermission(); if (forbid != null) return forbid;
				int userId = Services.SessionManager.IsLogged() ? Services.SessionManager.GetInstance.User.id :0;
				_bll.CerrarMantenimiento(maintenanceId, duracionHoras, costoEstimado, observaciones, userId);
				return Json(new { success = true });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}

		// POST: /Robot/UpdateTaskData
		[HttpPost]
		public ActionResult UpdateTaskData(int id, DateTime? fechaProgramada, string parametrosJSON, string observaciones, DateTime? fechaInicio = null, DateTime? fechaFin = null)
		{
			try
			{
				var forbid = ForbidIfNoPermission(); if (forbid != null) return forbid;
				int userId = Services.SessionManager.IsLogged() ? Services.SessionManager.GetInstance.User.id :0;
				var tarea = new TareaRobot { Id = id, FechaProgramada = fechaProgramada, ParametrosJSON = parametrosJSON, Observaciones = observaciones, FechaInicio = fechaInicio, FechaFin = fechaFin };
				_bll.ActualizarTareaDatos(tarea, userId);
				return Json(new { success = true });
			}
			catch (System.Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}

		// GET: /Robot/GetTaskStates
		[HttpGet]
		public ActionResult GetTaskStates(int robotId)
		{
			try
			{
				var tareas = _bll.ObtenerTareas(robotId, null,200);
				var estados = _bll.ObtenerEstadosTarea();
				var data = tareas.Select(t => new { id = t.Id, estadoId = t.IdEstadoTarea, estadoNombre = estados.ContainsKey(t.IdEstadoTarea)? estados[t.IdEstadoTarea] : t.IdEstadoTarea.ToString() }).ToList();
				return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
			}
		}
	}
}
