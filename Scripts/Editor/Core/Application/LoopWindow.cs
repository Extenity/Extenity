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

		[MenuItem(ExtenityMenu.Application + "Loop", priority = ExtenityMenu.ApplicationPriority + 1)]
		private static void Init()
		{
			EditorWindowTools.ToggleWindow<LoopWindow>();
		}

		protected override void OnEnableDerived()
		{
			IsRightMouseButtonScrollingEnabled = true;
		}

		#endregion

		#region GUI

		private bool FixedUpdateFold;
		private bool UpdateFold;
		private bool LateUpdateFold;

		protected override void OnGUIDerived()
		{
			var instance = Loop.Instance;
			if (!instance)
			{
				GUILayout.Label("Loop instance is not available.");
				return;
			}

			ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, false, true);

			DrawListenersList("FixedUpdate",
			                  instance.PreFixedUpdateCallbacks._Listeners,
			                  instance.FixedUpdateCallbacks._Listeners,
			                  instance.PostFixedUpdateCallbacks._Listeners,
			                  ref FixedUpdateFold);

			DrawListenersList("Update",
			                  instance.PreUpdateCallbacks._Listeners,
			                  instance.UpdateCallbacks._Listeners,
			                  instance.PostUpdateCallbacks._Listeners,
			                  ref UpdateFold);

			DrawListenersList("LateUpdate",
			                  instance.PreLateUpdateCallbacks._Listeners,
			                  instance.LateUpdateCallbacks._Listeners,
			                  instance.PostLateUpdateCallbacks._Listeners,
			                  ref LateUpdateFold);

			GUILayout.EndScrollView();
		}

		private void DrawListenersList(string header,
		                               List<ExtenityEvent.Listener> preListeners,
		                               List<ExtenityEvent.Listener> defaultListeners,
		                               List<ExtenityEvent.Listener> postListeners,
		                               ref bool fold)
		{
			EditorGUILayoutTools.DrawHeader(header);

			var totalCount = preListeners.Count + defaultListeners.Count + postListeners.Count;
			fold = EditorGUILayout.Foldout(fold, "Listeners: " + totalCount);
			if (fold)
			{
				GUILayout.BeginVertical();
				DrawListenersList("Listeners (pre-step): ", preListeners);
				DrawListenersList("Listeners (default-step): ", defaultListeners);
				DrawListenersList("Listeners (post-step): ", postListeners);
				GUILayout.EndVertical();
			}

			GUILayout.Space(20f);
		}

		private static void DrawListenersList(string title, List<ExtenityEvent.Listener> listeners)
		{
			GUILayout.Label(title + listeners.Count);
			foreach (var listener in listeners)
			{
				GUILayout.Label($"{listener.Order} \t {listener.Callback.FullNameOfTargetAndMethod(3, " \t ")}");
			}
		}

		#endregion
	}

}
