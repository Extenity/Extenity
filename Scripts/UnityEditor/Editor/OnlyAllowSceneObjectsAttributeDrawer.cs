using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox
{

	[CustomPropertyDrawer(typeof(OnlyAllowSceneObjectsAttribute))]
	public class OnlyAllowSceneObjectsAttributeDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.PropertyField(position, property, label);

			if (property.propertyType.ToString() == "ObjectReference")
			{
				var obj = property.objectReferenceValue;
				if (obj != null)
				{
					var prefabType = PrefabUtility.GetPrefabType(obj);

					if (prefabType != PrefabType.None &&
						prefabType != PrefabType.PrefabInstance &&
						prefabType != PrefabType.ModelPrefabInstance &&
						prefabType != PrefabType.DisconnectedPrefabInstance &&
						prefabType != PrefabType.DisconnectedModelPrefabInstance)
					{
						Debug.LogError("Field '" + property.displayName + "' only allows scene objects. Does not allow prefabs or model prefabs.");
						property.objectReferenceValue = null;
					}
				}
			}
			else
			{
				Debug.LogError("OnlyAllowSceneObjects attribute is only meaningful for fields of type Object.");
			}
		}
	}

}
