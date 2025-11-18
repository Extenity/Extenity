using Extenity.DebugToolbox.GraphPlotting;
using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.FlowToolbox
{

	public class FPSAnalyzerWindow : ExtenityEditorWindowBase
	{
		#region Configuration

		protected override WindowSpecifications Specifications => new WindowSpecifications
		{
			Title = "FPS Analyzer",
		};

		#endregion

		#region Initialization

		[MenuItem(ExtenityMenu.Application + "FPS Analyzer", priority = ExtenityMenu.ApplicationPriority + 2)]
		private static void Init()
		{
			EditorWindowTools.ToggleWindow<FPSAnalyzerWindow>();
		}

		protected override void OnEnableDerived()
		{
			IsRightMouseButtonScrollingEnabled = true;
			IsAutoRepaintInspectorEnabled = true;
			AutoRepaintInspectorPeriod = 0.2f;
		}

		protected override void OnDisableDerived()
		{
			// Disable FPS analyzer when window is closed
			if (FPSAnalyzer.IsEnabled)
			{
				FPSAnalyzer.Disable();
			}
		}

		#endregion

		#region GUI

		protected override void OnGUIDerived()
		{
			GUILayout.BeginHorizontal();

			if (FPSAnalyzer.IsEnabled)
			{
				if (GUILayout.Button("Disable FPS Analyzer", GUILayout.Height(30)))
				{
					FPSAnalyzer.Disable();
				}
			}
			else
			{
				if (GUILayout.Button("Enable FPS Analyzer", GUILayout.Height(30)))
				{
					FPSAnalyzer.Enable();
				}
			}

			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			if (FPSAnalyzer.IsEnabled && FPSAnalyzer.Analyzer != null)
			{
				EditorGUILayoutTools.DrawHeader("Statistics");
				EditorGUI.indentLevel++;

				var analyzer = FPSAnalyzer.Analyzer;
				EditorGUILayout.LabelField("FPS (Current)", analyzer.TicksPerSecond.ToString());
				EditorGUILayout.LabelField("FPS (Average)", (1.0 / analyzer.MeanElapsedTime).ToString("F2"));
				EditorGUILayout.LabelField("Frame Time (ms)", (analyzer.LastElapsedTime * 1000.0).ToString("F2"));
				EditorGUILayout.LabelField("Frame Time Avg (ms)", (analyzer.MeanElapsedTime * 1000.0).ToString("F2"));
				EditorGUILayout.LabelField("Frame Time Deviation (ms)", (analyzer.LastElapsedTimeDeviation * 1000.0).ToString("F2"));

				EditorGUI.indentLevel--;
				GUILayout.Space(10);

				// Graph is drawn automatically by the TickAnalyzer/TickPlotter system
				EditorGUILayout.HelpBox("Graph is displayed in the Game view when FPS Analyzer is enabled.", MessageType.Info);
			}
			else
			{
				EditorGUILayout.HelpBox("FPS Analyzer is disabled. Click 'Enable FPS Analyzer' to start tracking.", MessageType.Info);
			}
		}

		#endregion
	}

}
