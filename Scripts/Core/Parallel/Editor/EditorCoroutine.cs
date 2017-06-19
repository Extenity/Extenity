using System;
using System.Collections;
using System.Timers;
using UnityEngine;
using UnityEditor;

namespace Extenity.ParallelToolbox.Editor
{

	public static class EditorCoroutine
	{
		public static void StartCoroutineInEditorUpdate(this IEnumerator update, Action onFinished = null)
		{
			EditorApplication.CallbackFunction onUpdate = null;

			onUpdate = () =>
			{
				try
				{
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

		public static void StartCoroutineInTimer(this IEnumerator update, float interval = 0.05f, Action onFinished = null)
		{
			var timer = new Timer(interval);
			timer.AutoReset = false;

			timer.Elapsed += (sender, args) =>
			{
				throw new NotImplementedException();
				Debug.Log("## timer");
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
					}
					else
					{
						timer.Enabled = true;
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
				}
			};

			timer.Enabled = true;
		}
	}

}
