#if ExtenityAudio

using System.Linq;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.Audio.Editor
{

	// TODO: Convert this to Odin Inspector.
	[CustomEditor(typeof(AudioManager))]
	public class AudioManagerInspector : ExtenityEditorBase<AudioManager>
	{
		protected override void OnEnableDerived()
		{
		}

		protected override void OnDisableDerived()
		{
		}

		private int PlayerPopupIndex = -1;
		private string PlayerEventName;
		private float PlayerPin;

		protected override void OnBeforeDefaultInspectorGUI()
		{
			// Player
			EditorGUILayoutTools.DrawHeader("Player");
			EditorGUI.BeginDisabledGroup(!Application.isPlaying);
			{
				// Event selection
				EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
				EditorGUILayout.BeginVertical();
				GUILayout.Label("Event Name");
				EditorGUILayoutTools.PopupWithInputField(Me.EventNames, ref PlayerPopupIndex, ref PlayerEventName);

				// Pin
				EditorGUI.BeginDisabledGroup(PlayerPopupIndex < 0 || Me.Events[PlayerPopupIndex].Type != AudioEventType.WeightedGroups);
				GUILayout.Label("Pin (for weighted group selection)");
				PlayerPin = EditorGUILayout.Slider(PlayerPin, 0f, 1f);
				EditorGUI.EndDisabledGroup();

				EditorGUILayout.EndVertical();
				GUILayout.Space(10f);

				// Play button
				if (GUILayoutTools.Button("Play", PlayerPopupIndex >= 0, GUILayout.Width(80f), GUILayout.Height(80f)))
				{
					AudioManager.Play(PlayerEventName, PlayerPin);
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUI.EndDisabledGroup();

			GUILayout.Space(30f);

			// Draw warning for events with unassigned outputs
			{
				var eventsWithUnassignedOutputs = Me.ListEventsWithUnassignedOutputs();
				if (eventsWithUnassignedOutputs.IsNotNullAndEmpty())
				{
					EditorGUILayout.HelpBox($"There are '{eventsWithUnassignedOutputs.Count}' event(s) with unassigned outputs:\n\n{eventsWithUnassignedOutputs.Select(item => "   " + item.Name).ToList().Serialize('\n')}", MessageType.Warning);
					GUILayout.Space(20f);
					Release.List(ref eventsWithUnassignedOutputs);
				}
			}

			// Draw warning for events with unassigned clips
			{
				var eventsWithUnassignedClips = Me.ListEventsWithUnassignedClips();
				if (eventsWithUnassignedClips.IsNotNullAndEmpty())
				{
					EditorGUILayout.HelpBox($"There are '{eventsWithUnassignedClips.Count}' event(s) with unassigned clips:\n\n{eventsWithUnassignedClips.Select(item => "   " + item.Name).ToList().Serialize('\n')}", MessageType.Warning);
					GUILayout.Space(20f);
					Release.List(ref eventsWithUnassignedClips);
				}
			}
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
		}
	}

}

#endif
