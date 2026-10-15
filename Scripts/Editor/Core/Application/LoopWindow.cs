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

		/// <summary>Names of the callback lists that are unfolded to show their listeners.</summary>
		private readonly HashSet<string> UnfoldedCallbackLists = new HashSet<string>();

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

			// Display every callback list, in the order they are invoked in a frame.
			var totalCount = 0;
			foreach (var (_, callbacks) in instance.AllCallbackLists)
			{
				totalCount += callbacks._Listeners.Count;
			}
			EditorGUILayoutTools.DrawHeader($"Callbacks ({totalCount})");
			foreach (var (name, callbacks) in instance.AllCallbackLists)
			{
				DrawListenersList(name, callbacks._Listeners);
			}

			EditorGUILayoutTools.DrawHorizontalLine();
			GUILayout.Space(8f);

			GUILayout.EndScrollView();
		}

		private void DrawListenersList(string name, List<ExtenityEvent.Listener> listeners)
		{
			var wasUnfolded = UnfoldedCallbackLists.Contains(name);
			EditorGUI.BeginDisabledGroup(listeners.Count == 0);
			var isUnfolded = EditorGUILayout.Foldout(wasUnfolded && listeners.Count > 0, $"{name} ({listeners.Count})", true);
			EditorGUI.EndDisabledGroup();
			if (listeners.Count > 0 && isUnfolded != wasUnfolded)
			{
				if (isUnfolded)
					UnfoldedCallbackLists.Add(name);
				else
					UnfoldedCallbackLists.Remove(name);
			}

			if (isUnfolded)
			{
				EditorGUI.indentLevel += 2;
				foreach (var listener in listeners)
				{
					EditorGUILayout.LabelField($"{listener.Order} \t {listener.Callback.FullNameOfTargetAndMethod(3)}");
				}
				EditorGUI.indentLevel -= 2;
			}
		}

		#endregion
	}

}
