#if ExtenityAudio

using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.DebugToolbox.GraphPlotting.Editor
{

	[CustomEditor(typeof(AudioSourceGraphPlotter))]
	public class AudioSourceGraphPlotterInspector : ExtenityEditorBase<AudioSourceGraphPlotter>
	{
		protected override void OnEnableDerived()
		{
			IsDefaultInspectorDrawingEnabled = false;

			// Try to connect the link automatically
			if (!Me.AudioSource)
			{
				Undo.RecordObject(Me, "Automatic linking");
				Me.AudioSource = Me.GetComponent<AudioSource>();
			}
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(GetProperty("AudioSource"));

			EditorGUILayout.Space();

			var newShowVolume = EditorGUILayout.ToggleLeft(" Volume", Me.PlotVolume);
			if (newShowVolume != Me.PlotVolume)
			{
				Undo.RecordObject(Me, "Toggle volume");
				Me.PlotVolume = newShowVolume;
			}

			if (Me.PlotVolume)
			{
				CommonEditor.DrawVerticalRangeConfiguration(Me, Me.VolumeGraph, ref Me.VolumeRange);
			}

			var newShowPitch = EditorGUILayout.ToggleLeft(" Pitch", Me.PlotPitch);
			if (newShowPitch != Me.PlotPitch)
			{
				Undo.RecordObject(Me, "Toggle pitch");
				Me.PlotPitch = newShowPitch;
			}

			if (Me.PlotPitch)
			{
				CommonEditor.DrawVerticalRangeConfiguration(Me, Me.PitchGraph, ref Me.PitchRange);
			}

			var newShowIsPlaying = EditorGUILayout.ToggleLeft(" Is playing", Me.PlotIsPlaying);
			if (newShowIsPlaying != Me.PlotIsPlaying)
			{
				Undo.RecordObject(Me, "Toggle Is playing");
				Me.PlotIsPlaying = newShowIsPlaying;
			}

			CommonEditor.OpenGraphPlotterButton(Me.gameObject);

			if (GUI.changed)
				Me.SetupGraph();
		}
	}

}

#endif
