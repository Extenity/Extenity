using System;
using System.Collections;
using System.Timers;
using Extenity.IMGUIToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.ParallelToolbox.Editor
{

	public static class EditorCoroutine
	{
		/// <summary>
		/// Runs coroutine inside EditorApplication.update calls. This sometimes would hang in the middle of the operation because
		/// Unity may decide not to call updates.
		/// </summary>
		public static void StartCoroutineInEditorUpdate(this IEnumerator update, Action onFinished = null)
		{
			EditorApplication.CallbackFunction onUpdate = null;

			onUpdate = () =>
			{
				try
				{
					EditorApplication.delayCall += () => { }; // A little trick to keep EditorApplication.update calls coming.
					if (update.MoveNext() == false)
					{
						if (onFinished != null)
							onFinished();
						EditorApplication.update -= onUpdate;
					}
				}
				catch (Exception ex)
				{
					if (onFinished != null)
						onFinished();
					Debug.LogException(ex);
					EditorApplication.update -= onUpdate;
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
		/// Runs coroutines inside System.Timers.Timer. Runs really fast when given small intervals. Runs in the thread.
		/// </summary>
		public static void StartCoroutineInTimer(this IEnumerator update, bool autoRefreshUI = true, float interval = 0.001f, Action onFinished = null)
		{
			var timer = new Timer(1000f * interval);
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
				catch (Exception ex)
				{
					if (timer != null)
					{
						timer.Enabled = false;
						timer.Dispose();
						timer = null;
					}
					if (onFinished != null)
						onFinished();
					Debug.LogException(ex);
					if (autoRefreshUI)
						EditorGUITools.SafeRepaintAllViews();
				}
			};

			timer.Enabled = true;
		}
	}

}
