using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.PhysicsToolbox
{

	[CustomEditor(typeof(RigidbodySleepyStart))]
	public class RigidbodySleepyStartInspector : ExtenityEditorBase<RigidbodySleepyStart>
	{
		protected override void OnEnableDerived()
		{
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnBeforeDefaultInspectorGUI()
		{
			GUILayout.Space(20f);

			if (GUILayout.Button("Get All Rigidbodies In Loaded Scenes", BigButtonHeight))
			{
				Undo.RecordObject(Me, "Gather all rigidbodies");
				Me.GatherAllRigidbodiesInScene(true);
			}

			GUILayout.Space(20f);
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
		}
	}

}
