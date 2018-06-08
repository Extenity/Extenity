using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.GraphPlotting.Editor
{

	[CustomEditor(typeof(MonitorAudioSource))]
	public class MonitorAudioSourceEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			MonitorAudioSource monitorAudioSource = target as MonitorAudioSource;

			EditorGUILayout.Space();

			bool newShowVolume = EditorGUILayout.ToggleLeft(" Volume", monitorAudioSource.showVolume);
			if (newShowVolume != monitorAudioSource.showVolume)
			{
				Undo.RecordObject(monitorAudioSource, "Toggle volume");
				monitorAudioSource.showVolume = newShowVolume;
			}

			bool newShowPitch = EditorGUILayout.ToggleLeft(" Pitch", monitorAudioSource.showPitch);
			if (newShowPitch != monitorAudioSource.showPitch)
			{
				Undo.RecordObject(monitorAudioSource, "Toggle pitch");
				monitorAudioSource.showPitch = newShowPitch;
			}

			if (monitorAudioSource.showPitch)
			{
				float newMin, newMax;
				Utils.AxisSettings(monitorAudioSource, ref monitorAudioSource.pitchMode, monitorAudioSource.pitchMin, out newMin, monitorAudioSource.pitchMax, out newMax);
				if (newMin != monitorAudioSource.pitchMin)
				{
					monitorAudioSource.pitchMin = newMin;
					if (monitorAudioSource.monitor_pitch != null)
					{
						monitorAudioSource.monitor_pitch.Min = monitorAudioSource.pitchMin;
					}
				}

				if (newMax != monitorAudioSource.pitchMax)
				{
					monitorAudioSource.pitchMax = newMax;
					if (monitorAudioSource.monitor_pitch != null)
					{
						monitorAudioSource.monitor_pitch.Max = monitorAudioSource.pitchMax;
					}
				}
			}

			bool newShowIsPlaying = EditorGUILayout.ToggleLeft(" Is playing", monitorAudioSource.showIsPlaying);
			if (newShowIsPlaying != monitorAudioSource.showIsPlaying)
			{
				Undo.RecordObject(monitorAudioSource, "Toggle Is playing");
				monitorAudioSource.showIsPlaying = newShowIsPlaying;
			}

			Utils.OpenButton(monitorAudioSource.gameObject);

			monitorAudioSource.UpdateMonitors();

			if (GUI.changed)
				EditorUtility.SetDirty(target);
		}
	}

}