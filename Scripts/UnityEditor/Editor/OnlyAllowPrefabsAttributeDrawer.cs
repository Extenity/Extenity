using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	[CustomPropertyDrawer(typeof(OnlyAllowPrefabsAttribute))]
	public class OnlyAllowPrefabsAttributeDrawer : PropertyDrawer
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
					OnlyAllowPrefabsAttribute thisAttribute = (OnlyAllowPrefabsAttribute)attribute;

					var denied = thisAttribute.allowImportedModels
						? prefabType != PrefabType.Prefab && prefabType != PrefabType.ModelPrefab
						: prefabType != PrefabType.Prefab;

					if (denied)
					{
						Debug.LogError("Field '" + property.displayName + "' only allows prefabs.");
						property.objectReferenceValue = null;
					}
				}
			}
			else
			{
				Debug.LogError("OnlyAllowPrefabs attribute is only meaningful for fields of type Object.");
			}
		}
	}

}
