using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.MessagingToolbox;
using Extenity.UnityEditorToolbox;
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

		[MenuItem(ExtenityMenu.Application + "Loop")]
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
			DrawListenersList("FixedUpdate", Loop.Instance.FixedUpdateCallbacks._Listeners, ref FixedUpdateFold);
			DrawListenersList("Update", Loop.Instance.UpdateCallbacks._Listeners, ref UpdateFold);
			DrawListenersList("LateUpdate", Loop.Instance.LateUpdateCallbacks._Listeners, ref LateUpdateFold);

			var instance = Loop.Instance;
			if (!instance)
			{
				GUILayout.Label("Loop instance is not available.");
				return;
			}
		}

		private void DrawListenersList(string header, List<ExtenityEvent.Listener> listeners, ref bool fold)
		{
			EditorGUILayoutTools.DrawHeader(header);

			fold = EditorGUILayout.Foldout(fold, "Listeners: " + listeners.Count);
			if (fold)
			{
				GUILayout.BeginVertical();
				foreach (var listener in listeners)
				{
					GUILayout.Label($"{listener.Order} \t {listener.Callback.FullNameOfTargetAndMethod(3, " \t ")}");
				}
				GUILayout.EndVertical();
			}

			GUILayout.Space(20f);
		}

		#endregion
	}

}
