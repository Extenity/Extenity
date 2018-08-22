using System.Collections.Generic;
using Extenity.UnityEditorToolbox;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Extenity.UIToolbox.Editor
{

	[CanEditMultipleObjects]
	[CustomEditor(typeof(PowerUpButton))]
	public class PowerUpButtonInspector : ButtonEditor
	{
		private readonly List<SerializedProperty> DrawingProperties = new List<SerializedProperty>(5);

		private void Awake()
		{
			// Because all Unity internal fields start with "m_",
			// we can get our fields just by excluding them.
			// Hacky but neat solution.
			serializedObject.GatherSerializedPropertiesNotStartingWithM(DrawingProperties);
		}

		public override void OnInspectorGUI()
		{
			for (int i = 0; i < DrawingProperties.Count; i++)
			{
				EditorGUILayout.PropertyField(DrawingProperties[i], true);
			}

			serializedObject.ApplyModifiedProperties();

			GUILayout.Space(20f);

			base.OnInspectorGUI();
		}
	}

}
