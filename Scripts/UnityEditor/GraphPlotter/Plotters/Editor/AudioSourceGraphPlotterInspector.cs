using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.GraphPlotting.Editor
{

	[CustomEditor(typeof(AudioSourceGraphPlotter))]
	public class AudioSourceGraphPlotterInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			var Me = target as AudioSourceGraphPlotter;

			EditorGUILayout.Space();

			var newShowVolume = EditorGUILayout.ToggleLeft(" Volume", Me.showVolume);
			if (newShowVolume != Me.showVolume)
			{
				Undo.RecordObject(Me, "Toggle volume");
				Me.showVolume = newShowVolume;
			}

			var newShowPitch = EditorGUILayout.ToggleLeft(" Pitch", Me.showPitch);
			if (newShowPitch != Me.showPitch)
			{
				Undo.RecordObject(Me, "Toggle pitch");
				Me.showPitch = newShowPitch;
			}

			if (Me.showPitch)
			{
				float newMin, newMax;
				Utils.AxisSettings(Me, ref Me.pitchMode, Me.pitchMin, out newMin, Me.pitchMax, out newMax);
				if (newMin != Me.pitchMin)
				{
					Me.pitchMin = newMin;
					if (Me.monitor_pitch != null)
					{
						Me.monitor_pitch.Min = Me.pitchMin;
					}
				}

				if (newMax != Me.pitchMax)
				{
					Me.pitchMax = newMax;
					if (Me.monitor_pitch != null)
					{
						Me.monitor_pitch.Max = Me.pitchMax;
					}
				}
			}

			var newShowIsPlaying = EditorGUILayout.ToggleLeft(" Is playing", Me.showIsPlaying);
			if (newShowIsPlaying != Me.showIsPlaying)
			{
				Undo.RecordObject(Me, "Toggle Is playing");
				Me.showIsPlaying = newShowIsPlaying;
			}

			Utils.OpenButton(Me.gameObject);

			Me.UpdateMonitors();

			if (GUI.changed)
				EditorUtility.SetDirty(target);
		}
	}

}