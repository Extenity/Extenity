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
					GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
					{
						GUILayout.Label("Key", GUILayout.ExpandWidth(false));
						GUILayout.Label("Value", GUILayout.ExpandWidth(false));
					}
					GUILayout.EndVertical();
					GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
					{
						_Tool_Key = GUILayout.TextField(_Tool_Key, GUILayout.ExpandWidth(true));
						_Tool_Value = GUILayout.TextField(_Tool_Value, GUILayout.ExpandWidth(true));
					}
					GUILayout.EndVertical();
				}
				GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
				{
					if (GUILayout.Button("Add Row", GUILayout.ExpandWidth(false)))
					{
						EditorApplication.delayCall += () =>
						{
							Undo.RecordObject(Me.gameObject, "Add row");
							Me.AddRow(_Tool_Key, _Tool_Value);
						};
					}
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
					{
						var insertIndexAsText = GUILayout.TextArea(_Tool_InsertIndex.ToString());
						int.TryParse(insertIndexAsText, out _Tool_InsertIndex);
						if (GUILayout.Button("Insert", GUILayout.ExpandWidth(false)))
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
				GUILayout.Label("Key", GUILayout.ExpandWidth(false));
				_Tool_Key = GUILayout.TextField(_Tool_Key, GUILayout.ExpandWidth(true));
				if (GUILayout.Button("Remove Row", GUILayout.ExpandWidth(false)))
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
