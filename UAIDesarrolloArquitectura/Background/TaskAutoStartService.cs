using System;
using System.Threading;
using BLL;

namespace UAIDesarrolloArquitectura.Background
{
	public class TaskAutoStartService : IDisposable
	{
		private readonly Timer _timer;
		private readonly TimeSpan _period;
		private int _running =0;

		public TaskAutoStartService(TimeSpan period)
		{
			_period = period;
			_timer = new Timer(OnTick, null, Timeout.Infinite, Timeout.Infinite);
		}

		public void Start(){ _timer.Change(TimeSpan.Zero,_period); }
		public void Stop(){ _timer.Change(Timeout.Infinite,Timeout.Infinite); }

		private void OnTick(object state)
		{
			if(Interlocked.Exchange(ref _running,1)==1) return;
			try
			{
				var bll=new BLL_Robot();
				// Usar hora local (DB guarda fechas locales) en lugar de UTC para coincidencia de programaci?n
				var ahoraLocal = DateTime.Now;
				var pendientes=bll.ObtenerTareasParaAutoInicio(ahoraLocal,100);
				foreach(var t in pendientes){ try{ bll.IniciarTareaSiRobotActivo(t.Id); } catch { } }
				var aFinalizar=bll.ObtenerTareasParaAutoFin(ahoraLocal,100);
				foreach(var t in aFinalizar){ try{ bll.FinalizarTareaAuto(t.Id); } catch { } }
			}
			finally { _running=0; }
		}

		public void Dispose(){ Stop(); _timer.Dispose(); }
	}
}
