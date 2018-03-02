using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Extenity.UnityEditorToolbox.Editor
{

	[CustomEditor(typeof(BatchObjectProcessor))]
	public class BatchObjectProcessorInspector : ExtenityEditorBase<BatchObjectProcessor>
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

			if (GUILayout.Button("Process All", BigButtonHeight))
			{
				var changedObjectCount = Me.ProcessAll();
				if (changedObjectCount > 0)
				{
					EditorSceneManager.MarkAllScenesDirty();
				}
			}

			GUILayout.Space(20f);
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
		}
	}

}
