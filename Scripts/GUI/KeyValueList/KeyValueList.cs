using System;
using UnityEngine;
using System.Collections.Generic;
using Extenity.SceneManagement;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class KeyValueList : MonoBehaviour
	{
		[Header("Configuration")]
		public RectTransform KeyContainer;
		public RectTransform ValueContainer;
		public GameObject TextPrefab;

		[Header("Rows")]
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
			var row = new KeyValueListRow(
				GameObjectTools.InstantiateAndGetComponent<Text>(TextPrefab),
				GameObjectTools.InstantiateAndGetComponent<Text>(TextPrefab));

			row.KeyText.text = key;
			row.ValueText.text = value;

			row.KeyText.gameObject.SetActive(true);
			row.ValueText.gameObject.SetActive(true);

			row.KeyText.transform.SetParent(KeyContainer);
			row.ValueText.transform.SetParent(ValueContainer);

			KeyValueListRows.Add(row);
		}

		public void RemoveRow(string key)
		{
			for (int i = 0; i < KeyValueListRows.Count; i++)
			{
				if (KeyValueListRows[i].KeyText.text == key)
				{
					KeyValueListRows[i].Destroy();
					i--;
				}
			}
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
	}

}
