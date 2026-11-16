using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// An object field with a foldout arrow that shows the inspector of the referenced object in place. The
	/// inspector is built when the foldout opens, and is shown without its "Script" row. An object that is
	/// already shown by an enclosing inline editor is not shown again, so references in a circle do not nest
	/// forever.
	/// </summary>
	public class InlineObjectEditor : VisualElement, ILazyContent
	{
		public const string UssClassName = "ext-inline-editor";

		public ObjectField Field { get; }
		private readonly Foldout Foldout;
		private readonly VisualElement Content;
		private UnityEngine.Object ShownObject;
		private bool IsContentBuilt;

		/// <param name="label">Null for the property's own name.</param>
		public InlineObjectEditor(SerializedProperty property, string label, Type objectType, bool expandedByDefault)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));
			if (property.propertyType != SerializedPropertyType.ObjectReference)
				throw new ArgumentException($"An inline editor needs an object reference. '{property.propertyPath}' is a '{property.propertyType}'.", nameof(property));

			AddToClassList(UssClassName);
			InspectorStyles.Apply(this);

			Field = new ObjectField(label ?? property.displayName)
			{
				objectType = objectType ?? typeof(UnityEngine.Object),
				allowSceneObjects = !EditorUtility.IsPersistent(property.serializedObject.targetObject),
				bindingPath = property.propertyPath,
			};
			Field.AddToClassList(ObjectField.alignedFieldUssClassName);
			Field.AddToClassList(UssClassName + "__field");
			Add(Field);

			// The foldout's arrow sits in the margin left of the field's label, like the arrow of any property
			// with children. Its content follows under the field.
			var ownerType = property.serializedObject.targetObject.GetType();
			var stateKey = "Inline:" + property.propertyPath;
			Foldout = new Foldout { value = FoldoutStates.Get(ownerType, stateKey, expandedByDefault) };
			Foldout.AddToClassList(UssClassName + "__foldout");
			Add(Foldout);

			Content = new VisualElement();
			Content.AddToClassList(UssClassName + "__content");
			Foldout.Add(Content);

			ShownObject = property.objectReferenceValue;
			UpdateArrow();
			Field.RegisterValueChangedCallback(evt =>
			{
				ShownObject = evt.newValue;
				IsContentBuilt = false;
				Content.Clear();
				UpdateArrow();
				if (Foldout.value)
					BuildContent();
			});
			Foldout.RegisterValueChangedCallback(evt =>
			{
				if (evt.target != Foldout)
					return;
				FoldoutStates.Set(ownerType, stateKey, evt.newValue);
				if (evt.newValue)
					BuildContent();
			});
			RegisterCallback<AttachToPanelEvent>(_ =>
			{
				// Whether an enclosing editor shows the same object is only known once attached.
				if (Foldout.value)
					BuildContent();
			});
		}

		private void UpdateArrow()
		{
			// Nothing to open without an object.
			EnableInClassList(UssClassName + "--empty", ShownObject == null);
		}

		private void BuildContent()
		{
			if (IsContentBuilt || panel == null)
				return;
			IsContentBuilt = true;
			Content.Clear();
			if (ShownObject == null)
				return;

			for (var ancestor = hierarchy.parent; ancestor != null; ancestor = ancestor.hierarchy.parent)
			{
				if (ancestor is InlineObjectEditor enclosing && enclosing.ShownObject == ShownObject)
				{
					Content.Add(new MessageBox("This object is already shown by an enclosing editor.", MessageSeverity.None));
					return;
				}
			}
			var inspector = new InspectorElement(ShownObject);
			Content.Add(inspector);
			HideScriptFieldOf(inspector);
		}

		/// <summary>
		/// The "Script" row of the shown inspector is hidden: the object field right above already names the
		/// object and its type. An inspector may create its fields after it was built, so this looks again on
		/// layout changes until the row was found.
		/// </summary>
		private static void HideScriptFieldOf(InspectorElement inspector)
		{
			if (TryHideScriptField(inspector))
				return;
			EventCallback<GeometryChangedEvent> onGeometryChanged = null;
			onGeometryChanged = _ =>
			{
				if (TryHideScriptField(inspector))
					inspector.UnregisterCallback(onGeometryChanged);
			};
			inspector.RegisterCallback(onGeometryChanged);
		}

		private static bool TryHideScriptField(InspectorElement inspector)
		{
			var scriptField = inspector.Query<PropertyField>().Where(field => field.bindingPath == ScriptPropertyPath).First();
			if (scriptField == null)
				return false;
			scriptField.style.display = DisplayStyle.None;
			return true;
		}

		private const string ScriptPropertyPath = "m_Script";

		public bool ForceBuildContent(int maxListElements)
		{
			if (IsContentBuilt || panel == null || ShownObject == null)
				return false;
			BuildContent();
			return true;
		}
	}

}
