using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using AdvancedInspector;
using Extenity.SceneManagement;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class KeyValueList : MonoBehaviour
	{
		[Group("Configuration", Expandable = false, Priority = 10), Inspect(Priority = 13)]
		public GameObject TextPrefab;
		[Group("Configuration"), Inspect(Priority = 15)]
		public RectTransform KeyContainer;
		[Group("Configuration"), Inspect(Priority = 17)]
		public RectTransform ValueContainer;

		[Group("Rows", Expandable = true, Priority = 20)]
		public List<KeyValueListRow> KeyValueListRows = new List<KeyValueListRow>(50);

		[Serializable]
		public struct KeyValueListRow
		{
			public Text KeyText;
			public Text ValueText;

			public KeyValueListRow(Text keyText, Text valueText)
			{
				KeyText = keyText;
				ValueText = valueText;
			}

			public void Destroy()
			{
				if (KeyText != null)
				{
					DestroyImmediate(KeyText.gameObject);
					KeyText = null;
				}
				if (ValueText != null)
				{
					DestroyImmediate(ValueText.gameObject);
					ValueText = null;
				}
			}
		}

		public void AddRow(string key, string value)
		{
			InsertRow(key, value, KeyValueListRows.Count);
		}

		public void InsertRow(string key, string value, int index)
		{
			if (index < -1 || index > KeyValueListRows.Count)
				throw new ArgumentOutOfRangeException("index");

			var row = new KeyValueListRow(
				GameObjectTools.InstantiateAndGetComponent<Text>(TextPrefab),
				GameObjectTools.InstantiateAndGetComponent<Text>(TextPrefab));

			row.KeyText.text = key;
			row.ValueText.text = value;

			row.KeyText.gameObject.SetActive(true);
			row.ValueText.gameObject.SetActive(true);

			row.KeyText.transform.SetParent(KeyContainer);
			row.ValueText.transform.SetParent(ValueContainer);
			row.KeyText.transform.localScale = Vector3.one;
			row.ValueText.transform.localScale = Vector3.one;

			if (index < KeyValueListRows.Count)
			{
				row.KeyText.transform.SetSiblingIndex(index);
				row.ValueText.transform.SetSiblingIndex(index);
			}

			KeyValueListRows.Insert(index, row);
		}

		public bool RemoveRow(string key)
		{
			for (int i = 0; i < KeyValueListRows.Count; i++)
			{
				if (KeyValueListRows[i].KeyText.text == key)
				{
					KeyValueListRows[i].Destroy();
					KeyValueListRows.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		public void ClearRows()
		{
			for (int i = 0; i < KeyValueListRows.Count; i++)
			{
				KeyValueListRows[i].Destroy();
			}
			KeyValueListRows.Clear();
		}

		public KeyValueListRow GetRow(string key)
		{
			for (int i = 0; i < KeyValueListRows.Count; i++)
			{
				if (KeyValueListRows[i].KeyText.text == key)
					return KeyValueListRows[i];
			}
			return default(KeyValueListRow);
		}

		public void SetRow(string key, string value)
		{
			for (int i = 0; i < KeyValueListRows.Count; i++)
			{
				if (KeyValueListRows[i].KeyText.text == key)
				{
					KeyValueListRows[i].ValueText.text = value;
					return;
				}
			}
		}

		public void SetOrAddRow(string key, string value)
		{
			for (int i = 0; i < KeyValueListRows.Count; i++)
			{
				if (KeyValueListRows[i].KeyText.text == key)
				{
					KeyValueListRows[i].ValueText.text = value;
					return;
				}
			}

			AddRow(key, value);
		}

		#region Inspector

#if UNITY_EDITOR

		private string _Tool_Key = "";
		private string _Tool_Value = "";
		private int _Tool_InsertIndex = 0;

		[Conditional("UNITY_EDITOR")]
		[Group("Tools", Expandable = false, Priority = 150)]
		[Inspect(Priority = 15), Method(MethodDisplay.Invoke)]
		private void _InternalDrawTools()
		{
			GUILayout.BeginVertical();

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
						UnityEditor.EditorApplication.delayCall += () =>
						{
							UnityEditor.Undo.RecordObject(gameObject, "Add row");
							AddRow(_Tool_Key, _Tool_Value);
						};
					}
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
					{
						var insertIndexAsText = GUILayout.TextArea(_Tool_InsertIndex.ToString());
						int.TryParse(insertIndexAsText, out _Tool_InsertIndex);
						if (GUILayout.Button("Insert", GUILayout.ExpandWidth(false)))
						{
							UnityEditor.EditorApplication.delayCall += () =>
							{
								UnityEditor.Undo.RecordObject(gameObject, "Insert row");
								InsertRow(_Tool_Key, _Tool_Value, _Tool_InsertIndex);
							};
						}
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}

			// Horizontal line
			GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

			// Remove Row button
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Key", GUILayout.ExpandWidth(false));
				_Tool_Key = GUILayout.TextField(_Tool_Key, GUILayout.ExpandWidth(true));
				if (GUILayout.Button("Remove Row", GUILayout.ExpandWidth(false)))
				{
					UnityEditor.EditorApplication.delayCall += () =>
					{
						UnityEditor.Undo.RecordObject(gameObject, "Remove row");
						RemoveRow(_Tool_Key);
					};
				}
				GUILayout.EndHorizontal();
			}

			GUILayout.EndVertical();
		}

#endif

		#endregion
	}

}
