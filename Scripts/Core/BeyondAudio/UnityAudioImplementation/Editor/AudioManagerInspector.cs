using System.Linq;
using Extenity.DataToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.BeyondAudio.Editor
{

	[CustomEditor(typeof(AudioManager))]
	public class AudioManagerInspector : ExtenityEditorBase<AudioManager>
	{
		protected override void OnEnableDerived()
		{
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnBeforeDefaultInspectorGUI()
		{
			// Draw warning for events with unassigned outputs
			{
				var eventsWithUnassignedOutputs = Me.ListEventsWithUnassignedOutputs();
				if (eventsWithUnassignedOutputs.IsNotNullAndEmpty())
				{
					EditorGUILayout.HelpBox(string.Format("There are '{0}' event(s) with unassigned outputs:\n\n{1}",
							eventsWithUnassignedOutputs.Count,
							eventsWithUnassignedOutputs.Select(item => "   " + item.Name).ToList().Serialize('\n'))
						, MessageType.Warning);
					GUILayout.Space(20f);
				}
			}

			// Draw warning for events with unassigned clips
			{
				var eventsWithUnassignedClips = Me.ListEventsWithUnassignedClips();
				if (eventsWithUnassignedClips.IsNotNullAndEmpty())
				{
					EditorGUILayout.HelpBox(string.Format("There are '{0}' event(s) with unassigned clips:\n\n{1}",
							eventsWithUnassignedClips.Count,
							eventsWithUnassignedClips.Select(item => "   " + item.Name).ToList().Serialize('\n'))
						, MessageType.Warning);
					GUILayout.Space(20f);
				}
			}
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
		}
	}

}
