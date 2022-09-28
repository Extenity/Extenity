#if ExtenityAudio

using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.Audio.Effects.Editor
{

	[CustomEditor(typeof(MotorSound))]
	public class MotorSoundInspector : ExtenityEditorBase<MotorSound>
	{
		private bool IsDisplayingClipDetails;

		protected override void OnEnableDerived()
		{
			AutoRepaintInspectorPeriod = 0.05f;
			IsAutoRepaintInspectorEnabled = true;
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnBeforeDefaultInspectorGUI()
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayoutTools.ProgressBar("Load", Me.Load);
			EditorGUI.EndDisabledGroup();
			GUILayout.Space(10f);
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			if (Me.ClipConfigurations != null && Me.ClipConfigurations.Count > 0)
			{
				const int height = 100;
				const int separator = 6;
				const int lineWidth = 2;
				Color valueLineColor = Color.magenta;
				Color transparentColor = Color.clear;
				Color volumeLineColor = Color.red;
				Color frequencyLineColor = new Color(0f, 0.6f, 0f, 1f);

				var volumeRange = new Rect(0, 0, Me.MaxRPM, 1);
				var frequencyRange = new Rect(0, 0, Me.MaxRPM, 3);


				GUILayout.Space(20f);
				GUILayout.Label("Overlapping Volumes");
				var overlappingVolumeRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(height));
				GUI.Box(overlappingVolumeRect, "");

				Vector2 lineStart = Vector2.zero;
				Vector2 lineEnd = Vector2.zero;
				var lineX = Me.RPM.NormalizedValue * overlappingVolumeRect.width;
				if (!float.IsNaN(lineX))
				{
					lineStart = new Vector2(overlappingVolumeRect.min.x + lineX, overlappingVolumeRect.min.y);
					lineEnd = new Vector2(overlappingVolumeRect.min.x + lineX, overlappingVolumeRect.max.y);
					GUITools.DrawLine(lineStart, lineEnd, valueLineColor, lineWidth);
				}
				for (int i = 0; i < Me.ClipConfigurations.Count; i++)
				{
					var configuration = Me.ClipConfigurations[i];
					EditorGUIUtility.DrawCurveSwatch(overlappingVolumeRect, configuration.VolumeCurve, null, volumeLineColor, transparentColor, volumeRange);
				}

				IsDisplayingClipDetails = EditorGUILayout.Foldout(IsDisplayingClipDetails, "Clip Details");
				if (IsDisplayingClipDetails)
				{
					for (int i = 0; i < Me.ClipConfigurations.Count; i++)
					{
						var configuration = Me.ClipConfigurations[i];

						GUILayout.Space(separator);
						GUILayout.Label("Clip " + i);
						var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(height));
						GUI.Box(rect, "");

						if (!float.IsNaN(lineX))
						{
							lineStart.y = rect.min.y;
							lineEnd.y = rect.max.y;
							GUITools.DrawLine(lineStart, lineEnd, valueLineColor, lineWidth);
						}

						EditorGUIUtility.DrawCurveSwatch(rect, configuration.VolumeCurve, null, volumeLineColor, transparentColor, volumeRange);
						EditorGUIUtility.DrawCurveSwatch(rect, configuration.FrequencyCurve, null, frequencyLineColor, transparentColor, frequencyRange);

						//if (Event.current.type == EventType.MouseDown && Event.current.button < 2)
						//{
						//	if (rect.Contains(Event.current.mousePosition))
						//	{
						//		EditorGUI.ShowCurvePopup(GUIView.current, ranges);
						//		Event.current.Use();
						//	}
						//}

						rect.y += height + separator;
					}
				}

				GUILayout.Space(20f);
			}
		}
	}

}

#endif
