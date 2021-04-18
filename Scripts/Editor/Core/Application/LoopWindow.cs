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

			DrawListenersList("FixedUpdate ({0})",
			                  instance.PreFixedUpdateCallbacks._Listeners,
			                  instance.FixedUpdateCallbacks._Listeners,
			                  instance.PostFixedUpdateCallbacks._Listeners,
			                  ref FixedUpdateFold);

			DrawListenersList("Update ({0})",
			                  instance.PreUpdateCallbacks._Listeners,
			                  instance.UpdateCallbacks._Listeners,
			                  instance.PostUpdateCallbacks._Listeners,
			                  ref UpdateFold);

			DrawListenersList("LateUpdate ({0})",
			                  instance.PreLateUpdateCallbacks._Listeners,
			                  instance.LateUpdateCallbacks._Listeners,
			                  instance.PostLateUpdateCallbacks._Listeners,
			                  ref LateUpdateFold);

			EditorGUILayoutTools.DrawHorizontalLine();
			GUILayout.Space(8f);

			GUILayout.EndScrollView();
		}

		private void DrawListenersList(string header,
		                               List<ExtenityEvent.Listener> preListeners,
		                               List<ExtenityEvent.Listener> defaultListeners,
		                               List<ExtenityEvent.Listener> postListeners,
		                               ref bool fold)
		{
			EditorGUILayoutTools.DrawHorizontalLine();
			GUILayout.Space(8f);

			var totalCount = preListeners.Count + defaultListeners.Count + postListeners.Count;
			fold = EditorGUILayout.BeginFoldoutHeaderGroup(fold, string.Format(header, totalCount));
			if (fold)
			{
				GUILayout.BeginVertical();
				DrawListenersList("Pre-step listeners\t({0})", preListeners);
				DrawListenersList("Default listeners\t({0})", defaultListeners);
				DrawListenersList("Post-step listeners\t({0})", postListeners);
				GUILayout.EndVertical();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		private static void DrawListenersList(string title, List<ExtenityEvent.Listener> listeners)
		{
			EditorGUI.indentLevel += 2;
			EditorGUILayoutTools.DrawHeader(string.Format(title, listeners.Count));
			EditorGUI.indentLevel -= 2;
			foreach (var listener in listeners)
			{
				GUILayout.Label($"{listener.Order} \t {listener.Callback.FullNameOfTargetAndMethod(3, " \t ")}");
			}
		}

		#endregion
	}

}
