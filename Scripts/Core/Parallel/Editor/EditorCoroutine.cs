using System;
using System.Collections;
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
	}

}
