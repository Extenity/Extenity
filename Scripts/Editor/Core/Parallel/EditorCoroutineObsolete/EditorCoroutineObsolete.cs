using System;
using System.Collections;
using Extenity.ApplicationToolbox.Editor;
using Extenity.IMGUIToolbox.Editor;
using UnityEditor;

namespace Extenity.ParallelToolbox.Editor
{

	public static class EditorCoroutineObsolete
	{
		/// <summary>
		/// Runs coroutine inside EditorApplication.update calls. This sometimes would hang in the middle of the operation because
		/// Unity may decide not to call updates.
		/// </summary>
		/// <param name="keepUpdatesComing">This is costly. This makes sure EditorApplication.update calls will be called continuously by Unity.</param>
		public static void StartCoroutineInEditorUpdate(this IEnumerator update, bool autoRefreshUI = true, bool keepUpdatesComing = true, Action onFinished = null)
		{
			//int callCount = 0;
			EditorApplication.CallbackFunction onUpdate = null;

			onUpdate = () =>
			{
				//callCount++;
				try
				{
					if (keepUpdatesComing)
						EditorApplicationTools.GuaranteeNextUpdateCall();
					else
						EditorApplicationTools.IncreaseChancesOfNextUpdateCall();

					if (update.MoveNext() == false)
					{
						EditorApplication.update -= onUpdate;
						if (onFinished != null)
							onFinished();
						//Log.Info("callCount : " + callCount);
					}
					if (autoRefreshUI)
						EditorGUITools.SafeRepaintAllViews();
				}
				catch (Exception exception)
				{
					Log.Error(exception);
					EditorApplication.update -= onUpdate;
					if (onFinished != null)
						onFinished();
					if (autoRefreshUI)
						EditorGUITools.SafeRepaintAllViews();
					//Log.Info("callCount : " + callCount);
				}
			};

			EditorApplication.update += onUpdate;
		}

		public static void StartCoroutineInDelayCalls(this IEnumerator update, Action onFinished = null)
		{
			//EditorApplication.delayCall
			throw new NotImplementedException();
		}

		/// <summary>
		/// Runs coroutines inside System.Threading.Timer. Runs really fast when given small intervals. Runs in the thread.
		/// </summary>
		public static void StartCoroutineInThreadingTimer(this IEnumerator update, bool autoRefreshUI = true, int interval = 1, Action onFinished = null)
		{
			bool isProcessing = false;

			System.Threading.Timer timer = null;
			timer = new System.Threading.Timer(
				state =>
				{
					if (isProcessing)
						return;
					isProcessing = true;

					try
					{
						if (update.MoveNext() == false)
						{
							if (timer != null)
							{
								timer.Dispose();
								timer = null;
							}
							if (onFinished != null)
								onFinished();
							if (autoRefreshUI)
								EditorGUITools.SafeRepaintAllViews();
							isProcessing = false;
						}
						else
						{
							if (autoRefreshUI)
								EditorGUITools.SafeRepaintAllViews();
							isProcessing = false;
						}
					}
					catch (Exception exception)
					{
						Log.Error(exception);
						if (timer != null)
						{
							timer.Dispose();
							timer = null;
						}
						if (onFinished != null)
							onFinished();
						if (autoRefreshUI)
							EditorGUITools.SafeRepaintAllViews();
						isProcessing = false;
					}
				},
				null,
				interval,
				interval
			);
		}


		/// <summary>
		/// Runs coroutines inside System.Timers.Timer. Runs really fast when given small intervals. Runs in the thread.
		/// </summary>
		public static void StartCoroutineInSystemTimer(this IEnumerator update, bool autoRefreshUI = true, float interval = 0.001f, Action onFinished = null)
		{
			var timer = new System.Timers.Timer(1000f * interval);
			timer.AutoReset = false;

			timer.Elapsed += (sender, args) =>
			{
				try
				{
					if (update.MoveNext() == false)
					{
						if (timer != null)
						{
							timer.Enabled = false;
							timer.Dispose();
							timer = null;
						}
						if (onFinished != null)
							onFinished();
						if (autoRefreshUI)
							EditorGUITools.SafeRepaintAllViews();
					}
					else
					{
						timer.Enabled = true;
						if (autoRefreshUI)
							EditorGUITools.SafeRepaintAllViews();
					}
				}
				catch (Exception exception)
				{
					Log.Error(exception);
					if (timer != null)
					{
						timer.Enabled = false;
						timer.Dispose();
						timer = null;
					}
					if (onFinished != null)
						onFinished();
					if (autoRefreshUI)
						EditorGUITools.SafeRepaintAllViews();
				}
			};

			timer.Enabled = true;
		}

		#region Log

		private static readonly Logger Log = new(nameof(EditorCoroutineObsolete));

		#endregion
	}

}
