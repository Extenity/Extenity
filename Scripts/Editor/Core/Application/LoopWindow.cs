using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.MessagingToolbox;
using Extenity.UnityEditorToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.FlowToolbox
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
			IsAutoRepaintInspectorEnabled = true;
			AutoRepaintInspectorPeriod = 0.2f;
		}

		#endregion

		#region GUI

		private bool FixedUpdateFold;
		private bool UpdateFold;
		private bool InfrequentUpdatesFold;
		private bool LateUpdateFold;

		protected override void OnGUIDerived()
		{
			var instance = Loop.Instance;
			if (instance == null)
			{
				GUILayout.Label("Loop instance is not available.");
				return;
			}

			ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, false, true);

			// Display counters
			EditorGUILayoutTools.DrawHeader("Counters");
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField("Frame Count", Loop.FrameCount.ToString());
			EditorGUILayout.LabelField("Fixed Update Count", Loop.FixedUpdateCount.ToString());
			EditorGUI.indentLevel--;
			GUILayout.Space(8f);

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

			DrawInfrequentUpdates(instance);

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

		private void DrawInfrequentUpdates(LoopCallbacks instance)
		{
			EditorGUILayoutTools.DrawHorizontalLine();
			GUILayout.Space(8f);

			var totalCount = instance.UpdateEvery10FramesCallbacks._Listeners.Count +
			                 instance.UpdateEvery100MillisecondsCallbacks._Listeners.Count +
			                 instance.UpdateEvery250MillisecondsCallbacks._Listeners.Count +
			                 instance.UpdateEvery500MillisecondsCallbacks._Listeners.Count +
			                 instance.UpdateEvery1000MillisecondsCallbacks._Listeners.Count +
			                 instance.UpdateEvery100MillisecondsUnscaledCallbacks._Listeners.Count +
			                 instance.UpdateEvery250MillisecondsUnscaledCallbacks._Listeners.Count +
			                 instance.UpdateEvery500MillisecondsUnscaledCallbacks._Listeners.Count +
			                 instance.UpdateEvery1000MillisecondsUnscaledCallbacks._Listeners.Count;
			InfrequentUpdatesFold = EditorGUILayout.BeginFoldoutHeaderGroup(InfrequentUpdatesFold, $"Infrequent Updates ({totalCount})");
			if (InfrequentUpdatesFold)
			{
				GUILayout.BeginVertical();
				DrawListenersList("UpdateEvery10Frames\t({0})",                 instance.UpdateEvery10FramesCallbacks._Listeners);
				DrawListenersList("UpdateEvery100Milliseconds\t({0})",          instance.UpdateEvery100MillisecondsCallbacks._Listeners);
				DrawListenersList("UpdateEvery250Milliseconds\t({0})",          instance.UpdateEvery250MillisecondsCallbacks._Listeners);
				DrawListenersList("UpdateEvery500Milliseconds\t({0})",          instance.UpdateEvery500MillisecondsCallbacks._Listeners);
				DrawListenersList("UpdateEvery1000Milliseconds\t({0})",         instance.UpdateEvery1000MillisecondsCallbacks._Listeners);
				DrawListenersList("UpdateEvery100MillisecondsUnscaled\t({0})",  instance.UpdateEvery100MillisecondsUnscaledCallbacks._Listeners);
				DrawListenersList("UpdateEvery250MillisecondsUnscaled\t({0})",  instance.UpdateEvery250MillisecondsUnscaledCallbacks._Listeners);
				DrawListenersList("UpdateEvery500MillisecondsUnscaled\t({0})",  instance.UpdateEvery500MillisecondsUnscaledCallbacks._Listeners);
				DrawListenersList("UpdateEvery1000MillisecondsUnscaled\t({0})", instance.UpdateEvery1000MillisecondsUnscaledCallbacks._Listeners);
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
				GUILayout.Label($"{listener.Order} \t {listener.Callback.FullNameOfTargetAndMethod(3)}");
			}
		}

		#endregion
	}

}
