using Extenity.UnityEditorToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.UIToolbox.Editor
{

	[CustomEditor(typeof(ClickArea))]
	public class ClickAreaInspector : ExtenityEditorBase<ClickArea>
	{
		private SerializedProperty RaycastTargetProperty;
		private SerializedProperty ShapeProperty;

		protected override void OnEnableDerived()
		{
			IsDefaultInspectorDrawingEnabled = false;
			ShapeProperty = GetProperty("Shape");
			RaycastTargetProperty = GetProperty("m_RaycastTarget");
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			EditorGUILayout.PropertyField(ShapeProperty);
			EditorGUILayout.PropertyField(RaycastTargetProperty);
		}

		#region Editor right Click Menu

#if UNITY_EDITOR

		[MenuItem(ExtenityMenu.WidgetsContext + "Click Area")]
		private static void AddClickArea(MenuCommand menuCommand)
		{
			var go = new GameObject(nameof(ClickArea));
			go.AddComponent<ClickArea>();
			Undo.RegisterCreatedObjectUndo(go, $"Create {nameof(ClickArea)}");

			UIEditorUtilities.PlaceUIElementRoot(go, menuCommand);

			var defaultOffset = new Vector2(30, 30);
			var rectTransform = go.GetComponent<RectTransform>();
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.offsetMin = -defaultOffset;
			rectTransform.offsetMax = defaultOffset;

			// Disable RaycastTarget of parent object
			var parentGO = menuCommand.context as GameObject;
			if (parentGO != null)
			{
				var monoBehaviours = parentGO.GetComponents<MonoBehaviour>();
				foreach (var monoBehaviour in monoBehaviours)
				{
					if (monoBehaviour == null) // There might be missing scripts.
						continue;
					var serializedObject = new SerializedObject(monoBehaviour);
					var raycastTargetProperty = serializedObject.FindProperty("m_RaycastTarget");
					if (raycastTargetProperty != null && raycastTargetProperty.propertyType == SerializedPropertyType.Boolean)
					{
						if (raycastTargetProperty.boolValue)
						{
							Undo.RecordObject(monoBehaviour, "Raycast target disabled");
							raycastTargetProperty.boolValue = false;
							serializedObject.ApplyModifiedProperties();
						}
					}
				}
			}
		}

#endif

		#endregion
	}

}
