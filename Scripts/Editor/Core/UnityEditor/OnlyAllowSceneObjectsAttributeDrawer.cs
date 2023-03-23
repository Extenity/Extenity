using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
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
#if UNITY_2018_3_OR_NEWER
					throw new System.NotImplementedException();
#else
					var prefabType = PrefabUtility.GetPrefabType(obj);

					if (prefabType != PrefabType.None &&
						prefabType != PrefabType.PrefabInstance &&
						prefabType != PrefabType.ModelPrefabInstance &&
						prefabType != PrefabType.DisconnectedPrefabInstance &&
						prefabType != PrefabType.DisconnectedModelPrefabInstance)
					{
						Log.Error("Field '" + property.displayName + "' only allows scene objects. Does not allow prefabs or model prefabs.");
						property.objectReferenceValue = null;
					}
#endif
				}
			}
			else
			{
				Log.Error("OnlyAllowSceneObjects attribute is only meaningful for fields of type Object.");
			}
		}

		#region Log

		private static readonly Logger Log = new(nameof(OnlyAllowSceneObjectsAttributeDrawer));

		#endregion
	}

}
