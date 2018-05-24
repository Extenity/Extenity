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

		public EditorTools.TagsPane JobTagsPane = new EditorTools.TagsPane();
		public string[] JobTags;

		protected override void OnBeforeDefaultInspectorGUI()
		{
			GUILayout.Space(20f);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Job Tags:", GUILayout.ExpandWidth(false));
			JobTags = EditorTools.DrawTags(JobTags, JobTagsPane, 200);
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Process All", BigButtonHeight))
			{
				var changedObjectCount = Me.ProcessAll(JobTags);
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
