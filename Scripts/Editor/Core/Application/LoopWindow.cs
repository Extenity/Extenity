using System.Collections.Generic;
using Extenity.IMGUIToolbox.Editor;
using Extenity.MessagingToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity
{

	public class LoopWindow : ExtenityEditorWindowBase
	{
		#region Configuration

		protected override WindowSpecifications Specifications => new WindowSpecifications
		{
			Title = "Loop",
		};

		#endregion

		#region Initialization

		[MenuItem("Tools/Loop")]
		private static void Init()
		{
			EditorWindowTools.ToggleWindow<LoopWindow>();
		}

		#endregion

		#region GUI

		private bool FixedUpdateFold;
		private bool UpdateFold;
		private bool LateUpdateFold;

		protected override void OnGUIDerived()
		{
			DrawCallbacksList("FixedUpdate", Loop.FixedUpdateCallbacks.Callbacks, ref FixedUpdateFold);
			DrawCallbacksList("Update", Loop.UpdateCallbacks.Callbacks, ref UpdateFold);
			DrawCallbacksList("LateUpdate", Loop.LateUpdateCallbacks.Callbacks, ref LateUpdateFold);

			var instance = Loop.Instance;
			if (!instance)
			{
				GUILayout.Label("Loop instance is not available.");
				return;
			}
		}

		private void DrawCallbacksList(string header, List<ExtenityEvent.Entry> callbacks, ref bool fold)
		{
			EditorGUILayoutTools.DrawHeader(header);

			fold = EditorGUILayout.Foldout(fold, "Callbacks: " + callbacks.Count);
			if (fold)
			{
				GUILayout.BeginVertical();
				foreach (var callback in callbacks)
				{
					GUILayout.Label($"{callback.Order} \t {callback.Callback.FullNameOfTargetAndMethod(3, '/', " \t ")}");
				}
				GUILayout.EndVertical();
			}

			GUILayout.Space(20f);
		}

		#endregion
	}

}
