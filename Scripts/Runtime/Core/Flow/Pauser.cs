using System;
using System.Collections.Generic;

namespace Extenity.FlowToolbox
{

	public class PauseHandler
	{
		public readonly int ID;
		public readonly Pauser Pauser;
		public readonly string Description;

		private static int _LastGivenID = 100;

		internal PauseHandler(Pauser pauser, string description)
		{
			ID = ++_LastGivenID;
			Pauser = pauser;
			Description = description;
		}

		/// <summary>
		/// Resumes the related pause call. It's safe to call this more than once. Only first call will be processed and others will be silently ignored.
		/// </summary>
		public void Resume()
		{
			Pauser.Resume(this);
		}
	}

	public class Pauser
	{
		#region Initialization

		public Pauser(string id)
		{
			ID = id;
		}

		#endregion

		#region Metadata - ID

		public readonly string ID;

		#endregion

		#region Active Pause Handlers

		private List<PauseHandler> ActivePauseHandlers = new List<PauseHandler>(10);

		public string[] GetActivePauseHandlerDescriptions()
		{
			var list = new string[ActivePauseHandlers.Count];
			for (int i = 0; i < ActivePauseHandlers.Count; i++)
			{
				list[i] = ActivePauseHandlers[i].Description;
			}
			return list;
		}

		#endregion

		#region Pause / Resume

		public bool IsPaused { get { return ActivePauseHandlers.Count > 0; } }
		public int PauseCount { get { return ActivePauseHandlers.Count; } }

		public PauseHandler Pause(string description)
		{
			if (IsLoggingEnabled)
			{
				Log.Info($"Pausing for '{description}' in Pauser '{ID}'.");
			}

			var handler = new PauseHandler(this, description);
			ActivePauseHandlers.Add(handler);
			return handler;
		}

		public bool Resume(PauseHandler handler)
		{
			if (handler == null)
			{
				Log.Fatal($"Tried to resume Pauser '{ID}' but pause handler is null.");
				return false;
			}

			var resumed = ActivePauseHandlers.Remove(handler);

			if (IsLoggingEnabled)
			{
				if (resumed)
				{
					Log.Info($"Resuming '{handler.Description}' in Pauser '{ID}'.");
				}
				else
				{
					Log.Info($"Resuming '{handler.Description}' (though it was already resumed or cut loose) in Pauser '{ID}'.");
				}
			}

			return resumed;
		}

		/// <summary>
		/// Overrides all pause  handlers and removes all handlers from active handler list, making them not function anymore. It's safe to call resume on these loose pause handlers. These calls will be ignored silently.
		/// </summary>
		public void ResumeAll()
		{
			if (IsPaused)
			{
				if (IsLoggingEnabled)
				{
					Log.Info($"Resuming all {PauseCount} handler(s) '{string.Join(", ", GetActivePauseHandlerDescriptions())}' in Pauser '{ID}'.");
				}

				ActivePauseHandlers.Clear();
			}
			else
			{
				if (IsLoggingEnabled)
				{
					Log.Info($"Resuming all handlers (though there were none) in Pauser '{ID}'.");
				}
			}
		}

		#endregion

		#region Logging

		public bool IsLoggingEnabled = true;

		[NonSerialized]
		public Logger Log = new(nameof(Pauser));

		#endregion
	}

}
