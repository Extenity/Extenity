using Extenity.IMGUIToolbox.Editor;
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

		public TagsPane ProcessTagsPane = new TagsPane();
		public string[] ProcessTags;

		protected override void OnBeforeDefaultInspectorGUI()
		{
			GUILayout.Space(20f);

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			{
				GUILayout.Space(6f);

				// Process Tags
				GUILayout.BeginHorizontal();
				GUILayout.Label("Process Tags:", GUILayout.ExpandWidth(false));
				ProcessTags = GUILayoutTools.TagsPane(ProcessTags, ProcessTagsPane, 200);
				GUILayout.EndHorizontal();

				// Process Button
				if (GUILayout.Button("Process All", BigButtonHeight))
				{
					var changedObjectCount = Me.ProcessAll(ProcessTags);
					if (changedObjectCount > 0)
					{
						EditorSceneManager.MarkAllScenesDirty();
					}
				}

				GUILayout.Space(6f);
			}
			EditorGUILayout.EndVertical();

			GUILayout.Space(20f);
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
		}
	}

}
