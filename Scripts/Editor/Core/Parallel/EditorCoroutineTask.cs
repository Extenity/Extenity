using System;

namespace Extenity.ParallelToolbox.Editor
{

	public static class EditorCoroutineTask
	{
		public static void StartInEditorUpdate(this CoroutineTask task, bool autoRefreshUI = true, bool keepUpdatesComing = true, Action onFinished = null)
		{
			task._InternalStart();
			task.CoroutineEmulator().StartCoroutineInEditorUpdate(autoRefreshUI, keepUpdatesComing, onFinished);
		}

		public static void StartInDelayCalls(this CoroutineTask task, Action onFinished = null)
		{
			task._InternalStart();
			task.CoroutineEmulator().StartCoroutineInDelayCalls(onFinished);
		}

		public static void StartInThreadingTimer(this CoroutineTask task, bool autoRefreshUI = true, int interval = 1, Action onFinished = null)
		{
			task._InternalStart();
			task.CoroutineEmulator().StartCoroutineInThreadingTimer(autoRefreshUI, interval, onFinished);
		}

		public static void StartInSystemTimer(this CoroutineTask task, bool autoRefreshUI = true, float interval = 0.001f, Action onFinished = null)
		{
			task._InternalStart();
			task.CoroutineEmulator().StartCoroutineInSystemTimer(autoRefreshUI, interval, onFinished);
		}
	}

}
