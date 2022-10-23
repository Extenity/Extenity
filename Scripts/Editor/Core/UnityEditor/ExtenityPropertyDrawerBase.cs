using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	public abstract class ExtenityPropertyDrawerBase<T> : PropertyDrawer // where T : System.Object
	{
		#region Initialization

		////protected virtual void OnEnableBase() { }
		//protected virtual void OnEnableDerived() { }
		////protected virtual void OnDisableBase() { }
		//protected virtual void OnDisableDerived() { }

		//protected void OnEnable()
		//{
		//	//Me = target as T;
		//	//Configuration = new SerializedObject(target);

		//	//OnEnableBase();
		//	OnEnableDerived();
		//}

		//protected void OnDisable()
		//{
		//	OnDisableDerived();
		//	//OnDisableBase();
		//}

		#endregion

		#region Configuration

		//protected SerializedObject Configuration;
		//protected T Me;

		#endregion

		#region Inspector GUI

		public bool IsDefaultInspectorDrawingEnabled = true;
		public bool IsInspectorDisabledWhenPlaying = false;

		protected virtual void OnBeforeDefaultGUI() { }
		protected virtual void OnAfterDefaultGUI() { }

		public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var disabled = IsInspectorDisabledWhenPlaying;
			if (disabled)
				EditorGUI.BeginDisabledGroup(Application.isPlaying);

			OnBeforeDefaultGUI();

			if (IsDefaultInspectorDrawingEnabled)
			{
				DrawDefaultInspectorTheExtenityWay(position, property, label);
			}

			OnAfterDefaultGUI();

			if (disabled)
				EditorGUI.EndDisabledGroup();
		}

		public sealed override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return GetPropertyHeightTheExtenityWay(property, label);
		}

		public void DrawDefaultInspectorTheExtenityWay(Rect position, SerializedProperty property, GUIContent label)
		{
			// TODO MAINTENANCE: Update that in new Unity versions.
			// Originally copied from PropertyHandler.OnGUI (Unity version 2018.1.1f1)
			// Then modified.

			// Remember state
			Vector2 oldIconSize = EditorGUIUtility.GetIconSize();
			bool wasEnabled = GUI.enabled;
			int origIndent = EditorGUI.indentLevel;

			int relIndent = origIndent - property.depth;

			SerializedProperty prop = property.Copy();
			SerializedProperty endProperty = prop.GetEndProperty();

			position.height = EditorGUITools.GetSinglePropertyHeight(prop, label);

			// First property with custom label
			EditorGUI.indentLevel = prop.depth + relIndent;
			bool childrenAreExpanded = EditorGUITools.DefaultPropertyField(position, prop, label) && EditorGUITools.HasVisibleChildFields(prop);
			position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

			// Loop through all child properties
			while (prop.NextVisible(childrenAreExpanded) && !SerializedProperty.EqualContents(prop, endProperty))
			{
				/*
				// ----- Modification Start -----

				var display = true;

				// See if ConditionalHideInInspector allows the property to be drawn.
				// This is needed only for arrays and lists where ConditionalHideInInspector's property drawer
				// can do nothing about. Unity (for whatever the f*** reason is) won't give us any control over
				// the drawing of header and array size controls of array/list fields. So this is where we tell 
				// Unity to not to do that.
				if (prop.isArray && prop.propertyType != SerializedPropertyType.String)
				{
					var subFieldInfo = prop.GetFieldInfo();

					var conditionalHideInInspectorAttribute = subFieldInfo.GetAttribute<ConditionalHideInInspectorAttribute>(true);
					if (conditionalHideInInspectorAttribute != null)
					{
						var result = ConditionalHideInInspectorAttributeDrawer.DecideIfEnabled(conditionalHideInInspectorAttribute, prop);
						if (result == ConditionalHideResult.Hide)
						{
							display = false;
						}
					}
				}

				if (!display)
					continue;

				// ----- Modification End -----
				*/

				EditorGUI.indentLevel = prop.depth + relIndent;
				position.height = EditorGUI.GetPropertyHeight(prop, null, false);
				EditorGUI.BeginChangeCheck();
				childrenAreExpanded = PropertyHandlerTools.OnGUI(ScriptAttributeUtilityTools.GetHandler(prop), position, prop, null, false) && EditorGUITools.HasVisibleChildFields(prop);
				// Changing child properties (like array size) may invalidate the iterator,
				// so stop now, or we may get errors.
				if (EditorGUI.EndChangeCheck())
					break;

				position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			}

			// Restore state
			GUI.enabled = wasEnabled;
			EditorGUIUtility.SetIconSize(oldIconSize);
			EditorGUI.indentLevel = origIndent;
		}

		private float GetPropertyHeightTheExtenityWay(SerializedProperty property, GUIContent label)
		{
			property = property.Copy();
			SerializedProperty endProperty = property.GetEndProperty();

			// First property with custom label
			var height = EditorGUI.GetPropertyHeight(property, label, false);
			bool childrenAreExpanded = property.isExpanded && EditorGUITools.HasVisibleChildFields(property);

			// Loop through all child properties
			while (property.NextVisible(childrenAreExpanded) && !SerializedProperty.EqualContents(property, endProperty))
			{
				height += EditorGUI.GetPropertyHeight(property, EditorGUIUtilityTools.TempContent(property.displayName), true);
				childrenAreExpanded = false;
				height += EditorGUIUtility.standardVerticalSpacing;
			}

			return height;

			/*
			// Get the base height when not expanded
			var height = base.GetPropertyHeight(property, label);

			// Get children height if expanded
			if (property.isExpanded)
			{
				var propertyEnumerator = property.GetEnumerator();
				while (propertyEnumerator.MoveNext())
					height += EditorGUI.GetPropertyHeight((SerializedProperty)propertyEnumerator.Current, GUIContent.none, true);
			}
			return height;
			*/
		}

		#endregion
	}

}
