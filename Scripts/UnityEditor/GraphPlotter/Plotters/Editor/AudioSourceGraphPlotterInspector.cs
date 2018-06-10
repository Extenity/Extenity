using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.GraphPlotting.Editor
{

	[CustomEditor(typeof(AudioSourceGraphPlotter))]
	public class AudioSourceGraphPlotterInspector : ExtenityEditorBase<AudioSourceGraphPlotter>
	{
		protected override void OnEnableDerived()
		{
			IsDefaultInspectorDrawingEnabled = false;
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			EditorGUILayout.Space();

			var newShowVolume = EditorGUILayout.ToggleLeft(" Volume", Me.showVolume);
			if (newShowVolume != Me.showVolume)
			{
				Undo.RecordObject(Me, "Toggle volume");
				Me.showVolume = newShowVolume;
			}

			if (Me.showVolume)
			{
				Utils.DrawAxisRangeConfiguration(Me, Me.monitor_volume, ref Me.VolumeRange);
			}

			var newShowPitch = EditorGUILayout.ToggleLeft(" Pitch", Me.showPitch);
			if (newShowPitch != Me.showPitch)
			{
				Undo.RecordObject(Me, "Toggle pitch");
				Me.showPitch = newShowPitch;
			}

			if (Me.showPitch)
			{
				Utils.DrawAxisRangeConfiguration(Me, Me.monitor_pitch, ref Me.PitchRange);
			}

			var newShowIsPlaying = EditorGUILayout.ToggleLeft(" Is playing", Me.showIsPlaying);
			if (newShowIsPlaying != Me.showIsPlaying)
			{
				Undo.RecordObject(Me, "Toggle Is playing");
				Me.showIsPlaying = newShowIsPlaying;
			}

			Utils.OpenButton(Me.gameObject);

			if (GUI.changed)
				Me.UpdateMonitors();
		}
	}

}