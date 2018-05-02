using Extenity.IMGUIToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.UIToolbox.Editor
{

	[CustomEditor(typeof(KeyValueList))]
	public class KeyValueListInspector : ExtenityEditorBase<KeyValueList>
	{
		protected override void OnEnableDerived()
		{
		}

		protected override void OnDisableDerived()
		{
		}

		private string _Tool_Key = "";
		private string _Tool_Value = "";
		private int _Tool_InsertIndex = 0;

		protected override void OnAfterDefaultInspectorGUI()
		{
			GUILayout.BeginVertical();
			GUILayout.Space(15f);

			DrawHorizontalLine();

			// Add Row button
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.BeginVertical(GUILayoutTools.DontExpandWidth);
					{
						GUILayout.Label("Key", GUILayoutTools.DontExpandWidth);
						GUILayout.Label("Value", GUILayoutTools.DontExpandWidth);
					}
					GUILayout.EndVertical();
					GUILayout.BeginVertical(GUILayoutTools.ExpandWidth);
					{
						_Tool_Key = GUILayout.TextField(_Tool_Key, GUILayoutTools.ExpandWidth);
						_Tool_Value = GUILayout.TextField(_Tool_Value, GUILayoutTools.ExpandWidth);
					}
					GUILayout.EndVertical();
				}
				GUILayout.BeginVertical(GUILayoutTools.DontExpandWidth);
				{
					if (GUILayout.Button("Add Row", GUILayoutTools.DontExpandWidth))
					{
						EditorApplication.delayCall += () =>
						{
							Undo.RecordObject(Me.gameObject, "Add row");
							Me.AddRow(_Tool_Key, _Tool_Value);
						};
					}
					GUILayout.BeginHorizontal(GUILayoutTools.DontExpandWidth);
					{
						var insertIndexAsText = GUILayout.TextArea(_Tool_InsertIndex.ToString());
						int.TryParse(insertIndexAsText, out _Tool_InsertIndex);
						if (GUILayout.Button("Insert", GUILayoutTools.DontExpandWidth))
						{
							EditorApplication.delayCall += () =>
							{
								Undo.RecordObject(Me.gameObject, "Insert row");
								Me.InsertRow(_Tool_Key, _Tool_Value, _Tool_InsertIndex);
							};
						}
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}

			DrawHorizontalLine();

			// Remove Row button
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Key", GUILayoutTools.DontExpandWidth);
				_Tool_Key = GUILayout.TextField(_Tool_Key, GUILayoutTools.ExpandWidth);
				if (GUILayout.Button("Remove Row", GUILayoutTools.DontExpandWidth))
				{
					EditorApplication.delayCall += () =>
					{
						Undo.RecordObject(Me.gameObject, "Remove row");
						Me.RemoveRow(_Tool_Key);
					};
				}
				GUILayout.EndHorizontal();
			}

			DrawHorizontalLine();

			GUILayout.EndVertical();
		}
	}

}
