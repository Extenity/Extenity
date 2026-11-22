using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Extenity.InspectorToolbox.Editor;
using Extenity.Testing;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ExtenityTests.InspectorToolbox
{

	public class Test_InspectorToolbox : ExtenityTestBase
	{
		#region Setup

		private readonly List<Object> CreatedObjects = new List<Object>();
		private readonly List<IDisposable> Disposables = new List<IDisposable>();

		protected override void OnDeinitialize()
		{
			foreach (var disposable in Disposables)
				disposable.Dispose();
			Disposables.Clear();
			foreach (var created in CreatedObjects)
			{
				if (created != null)
					Object.DestroyImmediate(created);
			}
			CreatedObjects.Clear();
		}

		private T Create<T>() where T : ScriptableObject
		{
			var instance = ScriptableObject.CreateInstance<T>();
			CreatedObjects.Add(instance);
			return instance;
		}

		private BuiltInspector Build(params Object[] targets)
		{
			var built = InspectorTestTools.Build(targets);
			Disposables.Add(built);
			return built;
		}

		private static VisualElement Slot(BuiltInspector built, string fieldName)
		{
			return built.Root.Q(name: "ext-field-" + fieldName)
			       ?? throw new InvalidOperationException($"The inspector has no field slot for '{fieldName}'.");
		}

		private static bool IsDisplayed(VisualElement element)
		{
			return element.style.display != DisplayStyle.None;
		}

		/// <summary>Lets the bindings notice a change. Unity polls them at most every 100 ms.</summary>
		private static void Pump(BuiltInspector built)
		{
			for (var i = 0; i < 3; i++)
			{
				Thread.Sleep(110);
				built.Update();
			}
		}

		/// <summary>
		/// Unity keeps the expanded state of a property per type, not per object, and across test runs. A test
		/// that depends on it sets it before building the inspector.
		/// </summary>
		private static void SetExpanded(Object target, string propertyPath, bool expanded)
		{
			using (var serializedObject = new SerializedObject(target))
			{
				var property = serializedObject.FindProperty(propertyPath)
				               ?? throw new InvalidOperationException($"No property '{propertyPath}'.");
				property.isExpanded = expanded;
			}
		}

		private static List<string> MessagesOf(BuiltInspector built)
		{
			return built.GetAllMessages().Select(message => message.Message).ToList();
		}

		#endregion

		#region Declaration mistakes throw

		[Test]
		public void Blueprint_UnknownFieldName_Throws()
		{
			var exception = Assert.Throws<ArgumentException>(() => InspectorTestTools.GetBlueprint(typeof(UnknownFieldNameInspector)));
			StringAssert.Contains("DoesNotExist", exception.Message);
		}

		[Test]
		public void Blueprint_FieldClaimedTwice_Throws()
		{
			var exception = Assert.Throws<InvalidOperationException>(() => InspectorTestTools.GetBlueprint(typeof(FieldClaimedTwiceInspector)));
			StringAssert.Contains("Id", exception.Message);
		}

		[Test]
		public void Blueprint_ActionOnStruct_Throws()
		{
			var exception = Assert.Throws<InvalidOperationException>(() => InspectorTestTools.GetBlueprint(typeof(ActionOnStructInspector)));
			StringAssert.Contains("ref", exception.Message);
		}

		[Test]
		public void Blueprint_UnsupportedButtonParameter_Throws()
		{
			Assert.Throws<NotSupportedException>(() => InspectorTestTools.GetBlueprint(typeof(UnsupportedButtonParameterInspector)));
		}

		[Test]
		public void Blueprint_NonSerializedField_Throws()
		{
			var exception = Assert.Throws<ArgumentException>(() => InspectorTestTools.GetBlueprint(typeof(NonSerializedFieldInspector)));
			StringAssert.Contains("LiveCounter", exception.Message);
		}

		[Test]
		public void Blueprint_TwoFeaturesDrawingOneField_Throws()
		{
			Assert.Throws<InvalidOperationException>(() => InspectorTestTools.GetBlueprint(typeof(TwoDrawersForOneFieldInspector)));
		}

		[Test]
		public void Blueprint_PlacementInsideGroup_Throws()
		{
			Assert.Throws<InvalidOperationException>(() => InspectorTestTools.GetBlueprint(typeof(PlacedInsideGroupInspector)));
		}

		[Test]
		public void Blueprint_SectionsOnNestedType_Throws()
		{
			Assert.Throws<InvalidOperationException>(() => InspectorTestTools.GetBlueprint(typeof(SectionsOnNestedInspector)));
		}

		[Test]
		public void Blueprint_ListsReferencedFieldNames()
		{
			var blueprint = InspectorTestTools.GetBlueprint(typeof(FixtureNestedInspector));
			CollectionAssert.AreEquivalent(new[] { "OnlyForDaily" }, blueprint.ReferencedFieldNames);
			Assert.AreEqual(typeof(FixtureNested), blueprint.TargetType);
		}

		[Test]
		public void CustomizedInspectorTypes_ListRegisteredOnesOnly()
		{
			var types = InspectorTestTools.GetCustomizedInspectorTypes();
			CollectionAssert.Contains(types, typeof(FixtureObjectInspector));
			CollectionAssert.Contains(types, typeof(FixtureNestedInspector));
			CollectionAssert.Contains(types, typeof(FixtureShapeInspector));
			// Not registered with Unity through an attribute.
			CollectionAssert.DoesNotContain(types, typeof(UnknownFieldNameInspector));
			foreach (var type in types)
			{
				if (type.Namespace == typeof(FixtureObjectInspector).Namespace)
					Assert.DoesNotThrow(() => InspectorTestTools.GetBlueprint(type), type.Name);
			}
		}

		#endregion

		#region Fields

		[Test]
		public void EveryVisibleSerializedField_IsDrawn_UnmentionedOnesInDeclarationOrder()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			Assert.IsNotNull(built.RootInstance, "The fixture's editor was not used.");

			// Every visible property has an element bound to it, except the one the inspector hides on purpose.
			var boundPaths = new List<string>();
			built.Root.Query<VisualElement>().ForEach(element =>
			{
				if (element is IBindable bindable && !string.IsNullOrEmpty(bindable.bindingPath))
					boundPaths.Add(bindable.bindingPath);
			});
			var iterator = built.SerializedObject.GetIterator();
			iterator.NextVisible(true);
			do
			{
				if (iterator.name == nameof(FixtureObject.Hidden))
					continue;
				var path = iterator.propertyPath;
				Assert.IsTrue(boundPaths.Any(bound => bound == path || bound.StartsWith(path + ".", StringComparison.Ordinal)), $"Nothing is bound to '{path}'.");
			}
			while (iterator.NextVisible(false));

			// The plain fields keep their declaration order, and the field moved to the top comes before them.
			var order = boundPaths.Where(path => !path.Contains(".")).ToList();
			Assert.Less(order.IndexOf(nameof(FixtureObject.MovedToTop)), order.IndexOf(nameof(FixtureObject.First)));
			Assert.Less(order.IndexOf(nameof(FixtureObject.First)), order.IndexOf(nameof(FixtureObject.ShowDetails)));
			Assert.Less(order.IndexOf(nameof(FixtureObject.ShowDetails)), order.IndexOf(nameof(FixtureObject.Unmentioned)));
			Assert.Less(order.IndexOf(nameof(FixtureObject.Unmentioned)), order.IndexOf(nameof(FixtureObject.Last)));
			// The group sits where its first field is declared: after ToggleOnLeft, before Last.
			Assert.Less(order.IndexOf(nameof(FixtureObject.ToggleOnLeft)), order.IndexOf(nameof(FixtureObject.GroupedA)));
			Assert.Less(order.IndexOf(nameof(FixtureObject.GroupedB)), order.IndexOf(nameof(FixtureObject.Last)));
		}

		[Test]
		public void VisibleWhen_FlipsDisplay_AfterValueChangeAndReevaluate()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			var slot = Slot(built, nameof(FixtureObject.Details));
			Assert.IsFalse(IsDisplayed(slot));

			target.ShowDetails = true;
			built.RootInstance.Reevaluate();
			Assert.IsTrue(IsDisplayed(slot));

			target.ShowDetails = false;
			built.RootInstance.Reevaluate();
			Assert.IsFalse(IsDisplayed(slot));
		}

		[Test]
		public void VisibleWhen_FollowsAChange_WithoutBeingAsked()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			var slot = Slot(built, nameof(FixtureObject.Details));
			Assert.IsFalse(IsDisplayed(slot));

			// Through the inspector's own serialized object, as an edit in the inspector does.
			built.SerializedObject.FindProperty(nameof(FixtureObject.ShowDetails)).boolValue = true;
			built.SerializedObject.ApplyModifiedProperties();
			Pump(built);
			Assert.IsTrue(IsDisplayed(slot));

			// Directly on the object, as code or an undo does.
			target.ShowDetails = false;
			Pump(built);
			Assert.IsFalse(IsDisplayed(slot));
		}

		[Test]
		public void Hide_NeverShowsTheField()
		{
			var built = Build(Create<FixtureObject>());
			Assert.IsFalse(IsDisplayed(Slot(built, nameof(FixtureObject.Hidden))));
		}

		[Test]
		public void EnabledWhen_And_ReadOnly_DisableTheField()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			var gated = Slot(built, nameof(FixtureObject.Gated)).Q<PropertyField>();
			var locked = Slot(built, nameof(FixtureObject.Locked)).Q<PropertyField>();
			Assert.IsFalse(gated.enabledSelf);
			Assert.IsFalse(locked.enabledSelf);

			target.ShowDetails = true;
			built.RootInstance.Reevaluate();
			Assert.IsTrue(gated.enabledSelf);
			Assert.IsFalse(locked.enabledSelf);
		}

		[Test]
		public void Label_Static_Dynamic_AndPrivateField()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			Assert.AreEqual("Custom Label", Slot(built, nameof(FixtureObject.Renamed)).Q<PropertyField>().Q<Label>().text);
			Assert.AreEqual("Private", Slot(built, "PrivateSerialized").Q<PropertyField>().Q<Label>().text);

			var dynamic = Slot(built, nameof(FixtureObject.DynamicallyLabelled)).Q<PropertyField>();
			Assert.AreEqual("Dynamic 0", dynamic.Q<Label>().text);
			target.First = 7;
			built.RootInstance.Reevaluate();
			Assert.AreEqual("Dynamic 7", dynamic.Q<Label>().text);
		}

		[Test]
		public void Validator_MessageAppearsAndClears()
		{
			var target = Create<FixtureObject>();
			target.Required = "present";
			var built = Build(target);
			CollectionAssert.IsEmpty(MessagesOf(built));
			var slot = Slot(built, nameof(FixtureObject.Limited));
			Assert.IsNull(slot.Q<MessageBox>());

			target.Limited = 11;
			built.RootInstance.Reevaluate();
			CollectionAssert.AreEqual(new[] { "Limited must be at most 10." }, MessagesOf(built));
			var box = slot.Q<MessageBox>();
			Assert.IsNotNull(box, "The message is not under its field.");
			Assert.AreEqual(MessageSeverity.Error, box.Severity);
			Assert.IsTrue(slot.ClassListContains("ext-field--invalid"));

			target.Limited = 10;
			built.RootInstance.Reevaluate();
			CollectionAssert.IsEmpty(MessagesOf(built));
			Assert.IsNull(slot.Q<MessageBox>());
			Assert.IsFalse(slot.ClassListContains("ext-field--invalid"));
		}

		[Test]
		public void Required_And_ObjectValidator_ReportMessages()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			CollectionAssert.AreEqual(new[] { "Required is required." }, MessagesOf(built));

			target.Required = "x";
			target.Details = -1;
			built.RootInstance.Reevaluate();
			CollectionAssert.AreEqual(new[] { "Details must not be negative." }, MessagesOf(built));
			Assert.AreEqual("Details must not be negative.", built.Root.Q("ext-object-messages").Q<MessageBox>().Text);
		}

		[Test]
		public void OnChanged_RunsAfterTheValueChanged()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			Pump(built);
			Assert.AreEqual(0, target.ObservedChangeCount);

			built.SerializedObject.FindProperty(nameof(FixtureObject.Observed)).floatValue = 3f;
			built.SerializedObject.ApplyModifiedProperties();
			Pump(built);
			Assert.AreEqual(1, target.ObservedChangeCount);

			// Another field changing is not this field changing.
			built.SerializedObject.FindProperty(nameof(FixtureObject.First)).intValue = 3;
			built.SerializedObject.ApplyModifiedProperties();
			Pump(built);
			Assert.AreEqual(1, target.ObservedChangeCount);
		}

		[Test]
		public void CustomFields_AreBoundThroughBindingPath()
		{
			var target = Create<FixtureObject>();
			target.PickedNames.Add("A");
			var built = Build(target);
			built.ExpandAllLazyContent(3);

			var expected = new Dictionary<string, string>
			{
				{ nameof(FixtureObject.Choice), nameof(FixtureObject.Choice) },
				{ nameof(FixtureObject.OptionalChoice), nameof(FixtureObject.OptionalChoice) },
				{ nameof(FixtureObject.WrappedChoice), nameof(FixtureObject.WrappedChoice) + ".Value" },
				{ nameof(FixtureObject.NumberChoice), nameof(FixtureObject.NumberChoice) },
				{ nameof(FixtureObject.ToggleOnLeft), nameof(FixtureObject.ToggleOnLeft) },
				{ nameof(FixtureObject.Folder), nameof(FixtureObject.Folder) },
				{ nameof(FixtureObject.Icon), nameof(FixtureObject.Icon) },
				{ nameof(FixtureObject.Linked), nameof(FixtureObject.Linked) },
			};
			foreach (var pair in expected)
			{
				var bound = new List<string>();
				Slot(built, pair.Key).Query<VisualElement>().ForEach(element =>
				{
					if (element is IBindable bindable && !string.IsNullOrEmpty(bindable.bindingPath))
						bound.Add(bindable.bindingPath);
				});
				CollectionAssert.Contains(bound, pair.Value, $"The custom field of '{pair.Key}' is not bound to its property.");
			}

			// No custom field of the toolbox is left without a binding path.
			foreach (var className in new[] { "ext-dropdown", "ext-toggle-left", "ext-folder-picker", "ext-preview__field", "ext-inline-editor__field" })
			{
				var fields = built.Root.Query<VisualElement>(className: className).ToList();
				Assert.IsNotEmpty(fields, className);
				foreach (var field in fields)
					Assert.IsFalse(string.IsNullOrEmpty(((IBindable)field).bindingPath), $"A '{className}' field has no binding path.");
			}
		}

		#endregion

		#region Dropdown

		[Test]
		public void Dropdown_WriteChangesTheValue_AndUndoRestoresIt()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			var dropdown = Slot(built, nameof(FixtureObject.Choice)).Q<SearchableDropdownField<string>>();
			Assert.AreEqual("A", dropdown.value);
			Assert.AreEqual("Choice A", dropdown.DisplayText);
			Assert.IsFalse(dropdown.IsValueUnknown);

			Undo.IncrementCurrentGroup();
			dropdown.value = "B";
			Assert.AreEqual("B", target.Choice);
			Assert.AreEqual("Choice B", dropdown.DisplayText);

			Undo.PerformUndo();
			Assert.AreEqual("A", target.Choice);
			Pump(built);
			Assert.AreEqual("A", dropdown.value);
			Assert.AreEqual("Choice A", dropdown.DisplayText);
		}

		[Test]
		public void Dropdown_UnknownValue_IsMarkedAndKept()
		{
			var target = Create<FixtureObject>();
			target.Choice = "Z";
			var built = Build(target);
			var dropdown = Slot(built, nameof(FixtureObject.Choice)).Q<SearchableDropdownField<string>>();
			Assert.IsTrue(dropdown.IsValueUnknown);
			StringAssert.Contains("Z", dropdown.DisplayText);
			Pump(built);
			Assert.AreEqual("Z", target.Choice, "The field rewrote a value it does not know.");
		}

		[Test]
		public void Dropdown_EmptyChoice_And_Sorting()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			var dropdown = Slot(built, nameof(FixtureObject.OptionalChoice)).Q<SearchableDropdownField<string>>();
			Assert.AreEqual("(None)", dropdown.DisplayText);
			Assert.IsFalse(dropdown.IsValueUnknown);
			CollectionAssert.AreEqual(new[] { "(None)", "Choice A", "Choice B", "Choice C" }, dropdown.GetChoices().Select(choice => choice.Label));
		}

		[Test]
		public void Dropdown_OnWrapperStruct_And_Number_EditTheRawValue()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);

			var wrapped = Slot(built, nameof(FixtureObject.WrappedChoice)).Q<SearchableDropdownField<string>>();
			Assert.AreEqual("One", wrapped.DisplayText);
			Assert.AreEqual("Wrapped Choice", wrapped.label);
			wrapped.value = "two";
			Assert.AreEqual("two", target.WrappedChoice.Value);

			var number = Slot(built, nameof(FixtureObject.NumberChoice)).Q<SearchableDropdownField<int>>();
			Assert.AreEqual("First", number.DisplayText);
			number.value = 2;
			Assert.AreEqual(2, target.NumberChoice);
		}

		#endregion

		#region Buttons

		[Test]
		public void Button_OnObject_ChangesIt_AndUndoRestores()
		{
			var target = Create<FixtureObject>();
			target.Details = 42;
			var built = Build(target);
			var button = built.Root.Query<Button>().Where(candidate => candidate.text == "Reset Details").First();

			Undo.IncrementCurrentGroup();
			InspectorTestTools.Click(button);
			Assert.AreEqual(0, target.Details);
			Assert.IsTrue(EditorUtility.IsDirty(target));

			Undo.PerformUndo();
			Assert.AreEqual(42, target.Details);
		}

		[Test]
		public void Button_WithParameter_PassesTheFieldValue()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			var button = built.Root.Query<Button>().Where(candidate => candidate.text == "Add To Details").First();
			var form = button.GetFirstAncestorOfType<BoxGroup>();
			var amount = form.Q<IntegerField>();
			Assert.AreEqual("Amount", amount.label, "The parameter field is not labelled with the parameter's name.");
			Assert.AreEqual(5, amount.value, "The default value was not used.");
			var textsNamingTheAction = form.Query<TextElement>().Where(element => element.text == "Add To Details").ToList();
			Assert.AreEqual(1, textsNamingTheAction.Count, "The form shows the label of its button more than once.");

			InspectorTestTools.Click(button);
			Assert.AreEqual(5, target.Details);
			amount.value = 20;
			InspectorTestTools.Click(button);
			Assert.AreEqual(25, target.Details);
		}

		[Test]
		public void Button_OnNestedStruct_WritesBack()
		{
			var target = Create<FixtureObject>();
			target.Struct.Name = "before";
			var built = Build(target);
			built.ExpandAllLazyContent(3);
			var structField = built.Root.Query<PropertyField>().Where(field => field.bindingPath == nameof(FixtureObject.Struct)).First();
			var button = structField.Query<Button>().Where(candidate => candidate.text == "Set Name").First();
			Assert.IsNotNull(button, "The nested inspector of the struct was not built.");

			Undo.IncrementCurrentGroup();
			InspectorTestTools.Click(button);
			Assert.AreEqual("set by button", target.Struct.Name);

			Undo.PerformUndo();
			Assert.AreEqual("before", target.Struct.Name);
		}

		[Test]
		public void Button_OnNestedClass_WritesBack()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			built.ExpandAllLazyContent(3);
			var nestedField = built.Root.Query<PropertyField>().Where(field => field.bindingPath == nameof(FixtureObject.Nested)).First();
			var button = nestedField.Query<Button>().Where(candidate => candidate.text == "Stamp Version").First();
			InspectorTestTools.Click(button);
			Assert.AreEqual("stamped", target.Nested.Version);
		}

		#endregion

		#region Nested inspectors

		[Test]
		public void NestedInspector_UsesItsTitle_AndBuildsItsBodyWhenOpened()
		{
			var target = Create<FixtureObject>();
			target.Nested.Id = "the id";
			SetExpanded(target, nameof(FixtureObject.Nested), false);
			var built = Build(target);
			var nestedField = built.Root.Query<PropertyField>().Where(field => field.bindingPath == nameof(FixtureObject.Nested)).First();
			var instance = nestedField.Q<InspectorRootElement>().Instance;
			var foldout = nestedField.Q<LazyFoldout>();
			// A field that is not in a list keeps its own name: the title is for list elements.
			Assert.AreEqual("Nested", foldout.text);
			Assert.IsFalse(foldout.value);
			Assert.IsFalse(instance.IsBodyBuilt, "The body of a closed nested inspector was built.");

			// Opening it through the property's expanded state, as the Inspector window and capture tools do.
			built.SerializedObject.FindProperty(nameof(FixtureObject.Nested)).isExpanded = true;
			Pump(built);
			Assert.IsTrue(foldout.value);
			Assert.IsTrue(instance.IsBodyBuilt);
			Assert.IsNotNull(nestedField.Q(name: "ext-field-OnlyForDaily"));

			// Toggling the foldout stores the state in the property.
			foldout.value = false;
			Assert.IsFalse(built.SerializedObject.FindProperty(nameof(FixtureObject.Nested)).isExpanded, "Closing the foldout was not stored in the property.");
			foldout.value = true;
			Assert.IsTrue(built.SerializedObject.FindProperty(nameof(FixtureObject.Nested)).isExpanded, "Opening the foldout was not stored in the property.");

			built.SerializedObject.FindProperty(nameof(FixtureObject.Nested)).isExpanded = false;
		}

		[Test]
		public void NestedInspector_RuleFollowsItsOwnProperty()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			built.ExpandAllLazyContent(3);
			var nestedField = built.Root.Query<PropertyField>().Where(field => field.bindingPath == nameof(FixtureObject.Nested)).First();
			var slot = nestedField.Q(name: "ext-field-OnlyForDaily");
			Assert.IsFalse(IsDisplayed(slot));

			target.Nested.Kind = FixtureKind.Daily;
			Pump(built);
			Assert.IsTrue(IsDisplayed(slot));
		}

		[Test]
		public void NestedInspector_IsFoundInsideUnitysDefaultInspectorAndLists()
		{
			var target = Create<FixturePlainObject>();
			target.NestedList.Add(new FixtureNested { Id = "first" });
			target.NestedList.Add(new FixtureNested { Id = "second" });
			target.Shape = new FixtureSquare { Name = "a square" };
			target.Shapes.Add(new FixtureCircle { Name = "a circle" });

			var built = Build(target);
			Assert.IsNull(built.RootInstance);
			built.ExpandAllLazyContent(3);

			var titles = built.Root.Query<LazyFoldout>().ToList().Select(foldout => foldout.text).ToList();
			// List elements are named by the title of their inspector, single fields by their own name.
			CollectionAssert.Contains(titles, "Nested");
			CollectionAssert.Contains(titles, "first");
			CollectionAssert.Contains(titles, "second");
			CollectionAssert.Contains(titles, "Shape");
			CollectionAssert.Contains(titles, "a circle");

			// The title of a list element follows the data.
			target.NestedList[0].Id = "renamed";
			Pump(built);
			CollectionAssert.Contains(built.Root.Query<LazyFoldout>().ToList().Select(foldout => foldout.text).ToList(), "renamed");
			var instanceCounts = built.GetAllInstances().GroupBy(instance => instance.Blueprint.InspectorType)
			                          .ToDictionary(group => group.Key, group => group.Count());
			Assert.GreaterOrEqual(instanceCounts.GetValueOrDefault(typeof(FixtureNestedInspector)), 3);
			Assert.GreaterOrEqual(instanceCounts.GetValueOrDefault(typeof(FixtureShapeInspector)), 2);
			Assert.GreaterOrEqual(instanceCounts.GetValueOrDefault(typeof(FixtureStructInspector)), 1);

			// The rule of the base type's inspector applies to the derived instance, and the derived type's own
			// field is still drawn.
			var squareField = built.Root.Query<PropertyField>().Where(field => field.bindingPath == nameof(FixturePlainObject.Shape)).First();
			Assert.AreEqual("Size (m)", squareField.Q(name: "ext-field-Size").Q<PropertyField>().Q<Label>().text);
			Assert.IsNotNull(squareField.Query<PropertyField>().Where(field => field.bindingPath == "Shape.Side").First());
		}

		[Test]
		public void Inline_And_FlattenedStruct_DrawWithoutAFoldout()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);

			var inlined = Slot(built, nameof(FixtureObject.Inlined));
			Assert.IsNull(inlined.Q<Foldout>(), "An inline field has a foldout.");
			Assert.IsNotNull(inlined.Q(name: "ext-field-Extra"), "The inline field's own inspector was not used.");

			var flattened = built.Root.Query<PropertyField>().Where(field => field.bindingPath == nameof(FixtureObject.Flattened)).First();
			Assert.IsNull(flattened.Q<Foldout>());
			var inner = flattened.Query<PropertyField>().Where(field => field.bindingPath == "Flattened.Value").First();
			Assert.IsNotNull(inner);
			Assert.AreEqual("Flattened", inner.Q<Label>().text);
		}

		[Test]
		public void IMGUIFallback_MeasuresPlainChildren()
		{
			var target = Create<FixtureObject>();
			var serializedObject = new SerializedObject(target);
			var property = serializedObject.FindProperty(nameof(FixtureObject.Nested));
			var drawer = new FixtureNestedInspector();
			var line = EditorGUIUtility.singleLineHeight;

			// Without an editor Unity would remember the drawers of the children in its global cache, which outlives the test.
			using (PropertyHandlerCacheScope.SelectNew())
			{
				property.isExpanded = false;
				Assert.AreEqual(line, drawer.GetPropertyHeight(property, new GUIContent("Nested")), 0.01f);
				property.isExpanded = true;
				Assert.Greater(drawer.GetPropertyHeight(property, new GUIContent("Nested")), line * 4f);
				Assert.Greater(drawer.GetPropertyHeight(property, GUIContent.none), line * 3f);
				property.isExpanded = false;
			}
		}

		#endregion

		#region Lists

		private static ExtListView ListOf(BuiltInspector built, string path)
		{
			return built.Root.Query<ExtListView>().Where(list => list.ArrayPropertyPath == path).First()
			       ?? throw new InvalidOperationException($"No list element for '{path}'.");
		}

		[Test]
		public void List_FollowsTheExpandedStateOfItsProperty()
		{
			var target = Create<FixtureObject>();
			target.Names.Add("one");
			SetExpanded(target, nameof(FixtureObject.Names), false);
			var built = Build(target);
			var property = built.SerializedObject.FindProperty(nameof(FixtureObject.Names));
			var list = ListOf(built, nameof(FixtureObject.Names));
			var header = list.Q<LazyFoldout>();
			Assert.IsFalse(header.value);

			property.isExpanded = true;
			Pump(built);
			Assert.IsTrue(header.value);
			Assert.IsNotNull(list.Q<ListView>(), "Opening the list did not build its content.");

			header.value = false;
			Assert.IsFalse(built.SerializedObject.FindProperty(nameof(FixtureObject.Names)).isExpanded, "Closing the list was not stored in the property.");
		}

		[Test]
		public void List_Search_FiltersByLabel()
		{
			var target = Create<FixtureObject>();
			target.Names.AddRange(new[] { "apple", "banana", "apricot", "cherry" });
			var built = Build(target);
			built.ExpandAllLazyContent(2);
			var list = ListOf(built, nameof(FixtureObject.Names));
			CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, list.VisibleIndices);

			list.SearchText = "AP";
			Assert.IsTrue(list.IsFiltering);
			CollectionAssert.AreEqual(new[] { 0, 2 }, list.VisibleIndices);
			Assert.IsFalse(list.Q<ListView>().reorderable, "Reordering must be off while the list is filtered.");

			list.SearchText = "";
			CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, list.VisibleIndices);
			Assert.IsTrue(list.Q<ListView>().reorderable);
		}

		[Test]
		public void List_ElementLabel_Validation_AndChanges()
		{
			var target = Create<FixtureObject>();
			target.LabelledList.Add(new FixtureNested { Id = "a" });
			target.LabelledList.Add(new FixtureNested { Id = "b" });
			target.Required = "x";
			var built = Build(target);
			built.ExpandAllLazyContent(3);
			var list = ListOf(built, nameof(FixtureObject.LabelledList));
			Assert.AreEqual("Item a", list.GetElementLabel(0));
			Assert.AreEqual("Item b", list.GetElementLabel(1));

			// The label chosen by the list wins over the title of the element's own inspector.
			var rowTitles = list.Query<LazyFoldout>().ToList().Select(foldout => foldout.text).ToList();
			CollectionAssert.Contains(rowTitles, "Item a");
			CollectionAssert.DoesNotContain(rowTitles, "a");

			CollectionAssert.IsEmpty(MessagesOf(built));
			list.AddElement();
			Assert.AreEqual(3, target.LabelledList.Count);
			Assert.AreEqual("b", target.LabelledList[2].Id, "A new element copies the last one, as in Unity's lists.");
			built.RootInstance.Reevaluate();
			CollectionAssert.AreEqual(new[] { "Ids must be unique." }, MessagesOf(built));
			Assert.IsNotNull(list.ValidationMessageContainer.Q<MessageBox>(), "The message of a list belongs under its header.");

			list.MoveElement(0, 2);
			CollectionAssert.AreEqual(new[] { "b", "b", "a" }, target.LabelledList.Select(element => element.Id));
			list.RemoveElement(0);
			CollectionAssert.AreEqual(new[] { "b", "a" }, target.LabelledList.Select(element => element.Id));
			built.RootInstance.Reevaluate();
			CollectionAssert.IsEmpty(MessagesOf(built));
		}

		[Test]
		public void List_ElementLabel_ThatChangesWithAnEdit_KeepsTheFieldsOfTheRow()
		{
			var target = Create<FixtureObject>();
			target.LabelledList.Add(new FixtureNested { Id = "a" });
			var built = Build(target);
			built.ExpandAllLazyContent(3);
			Pump(built);
			var list = ListOf(built, nameof(FixtureObject.LabelledList));
			var idPath = nameof(FixtureObject.LabelledList) + ".Array.data[0]." + nameof(FixtureNested.Id);
			BindableElement FindIdField() => list.Query<BindableElement>().Where(element => element.bindingPath == idPath).First();
			var idField = FindIdField();
			Assert.IsNotNull(idField, "The row has no field for the id.");

			// The id is typed in: the label of the row is made of it.
			built.SerializedObject.Update();
			built.SerializedObject.FindProperty(idPath).stringValue = "ab";
			built.SerializedObject.ApplyModifiedProperties();
			Pump(built);

			Assert.AreEqual("Item ab", list.GetElementLabel(0));
			CollectionAssert.Contains(list.Query<LazyFoldout>().ToList().Select(foldout => foldout.text).ToList(), "Item ab", "The row did not take its new label.");
			Assert.AreSame(idField, FindIdField(), "The new label of the row replaced the field that was being edited: it would lose the keyboard focus with every key.");
			Assert.IsNotNull(idField.panel);
		}

		[Test]
		public void List_ManagedReferences_AddAndChangeType_KeepSharedFields()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			built.ExpandAllLazyContent(3);
			var list = ListOf(built, nameof(FixtureObject.Shapes));

			var types = list.GetManagedReferenceTypes();
			CollectionAssert.AreEquivalent(new[] { typeof(FixtureCircle), typeof(FixtureSquare) }, types.Select(entry => entry.Type));
			CollectionAssert.Contains(types.Select(entry => entry.MenuPath), "Shapes/Fixture Circle");

			list.AddManagedReference(typeof(FixtureCircle));
			Assert.AreEqual(1, target.Shapes.Count);
			Assert.IsInstanceOf<FixtureCircle>(target.Shapes[0]);

			target.Shapes[0].Name = "kept";
			target.Shapes[0].Size = 7f;
			list.ChangeManagedReferenceType(0, typeof(FixtureSquare));
			Assert.IsInstanceOf<FixtureSquare>(target.Shapes[0]);
			Assert.AreEqual("kept", target.Shapes[0].Name);
			Assert.AreEqual(7f, target.Shapes[0].Size);
			Assert.AreEqual(3f, ((FixtureSquare)target.Shapes[0]).Side);

			list.ChangeManagedReferenceType(0, null);
			Assert.IsNull(target.Shapes[0]);
			Assert.Throws<ArgumentException>(() => list.AddManagedReference(typeof(FixtureNested)));
			Assert.Throws<InvalidOperationException>(() => list.AddElement());
		}

		[Test]
		public void List_CustomAdd_AppendsWhatTheFunctionReturns()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			built.ExpandAllLazyContent(3);
			var list = ListOf(built, nameof(FixtureObject.Generated));
			list.AddElement();
			list.AddElement();
			CollectionAssert.AreEqual(new[] { "generated 0", "generated 1" }, target.Generated.Select(element => element.Id));
		}

		[Test]
		public void List_CollapsedByDefault_ClosesItOncePerObject()
		{
			var target = Create<FixtureObject>();
			SetExpanded(target, nameof(FixtureObject.InitiallyClosed), true);
			var built = Build(target);
			Assert.IsFalse(built.SerializedObject.FindProperty(nameof(FixtureObject.InitiallyClosed)).isExpanded);
			Assert.IsFalse(ListOf(built, nameof(FixtureObject.InitiallyClosed)).Q<LazyFoldout>().value);

			// The user's choice stays when the inspector is built again for the same object.
			SetExpanded(target, nameof(FixtureObject.InitiallyClosed), true);
			var rebuilt = Build(target);
			Assert.IsTrue(ListOf(rebuilt, nameof(FixtureObject.InitiallyClosed)).Q<LazyFoldout>().value);
			SetExpanded(target, nameof(FixtureObject.InitiallyClosed), false);
		}

		[Test]
		public void List_ExpandedByDefault_OpensItOncePerObject()
		{
			var target = Create<FixtureObject>();
			SetExpanded(target, nameof(FixtureObject.InitiallyOpen), false);
			var built = Build(target);
			Assert.IsTrue(built.SerializedObject.FindProperty(nameof(FixtureObject.InitiallyOpen)).isExpanded);
			Assert.IsTrue(ListOf(built, nameof(FixtureObject.InitiallyOpen)).Q<LazyFoldout>().value);

			// The user's choice stays when the inspector is built again for the same object.
			SetExpanded(target, nameof(FixtureObject.InitiallyOpen), false);
			var rebuilt = Build(target);
			Assert.IsFalse(ListOf(rebuilt, nameof(FixtureObject.InitiallyOpen)).Q<LazyFoldout>().value);
		}

		[Test]
		public void List_CollapsedAndExpandedByDefault_Throws()
		{
			var options = new ListOptions(typeof(int)) { CollapsedByDefault = true, ExpandedByDefault = true };
			var target = Create<FixtureObject>();
			var serializedObject = new SerializedObject(target);
			Assert.Throws<ArgumentException>(() => new ExtListView(serializedObject.FindProperty(nameof(FixtureObject.InitiallyOpen)), null, options));
		}

		[Test]
		public void List_AsTable_HasAColumnPerChosenField()
		{
			var target = Create<FixtureObject>();
			target.Table.Add(new FixtureStruct { Name = "row", Extra = 2f });
			var built = Build(target);
			built.ExpandAllLazyContent(3);
			var table = built.Root.Q<ExtTableView>();
			Assert.IsNotNull(table);
			var view = table.Q<MultiColumnListView>();
			Assert.IsNotNull(view, "Opening the table did not build its content.");
			CollectionAssert.AreEqual(new[] { "Name", "Extra" }, view.columns.Select(column => column.title));
			Assert.AreEqual(nameof(FixtureObject.Table), view.bindingPath);
		}

		[Test]
		public void List_ElementDropdown_DrawsEachElementAsADropdown()
		{
			var target = Create<FixtureObject>();
			target.PickedNames.Add("B");
			var built = Build(target);
			built.ExpandAllLazyContent(3);
			var list = ListOf(built, nameof(FixtureObject.PickedNames));
			var dropdown = list.Q<SearchableDropdownField<string>>();
			Assert.IsNotNull(dropdown);
			Assert.AreEqual("PickedNames.Array.data[0]", dropdown.bindingPath);
			Assert.AreEqual("Choice B", dropdown.DisplayText);
		}

		#endregion

		#region Numbers on one line, declared table columns, empty lists

		[Test]
		public void NumbersOnOneLine_DrawsAFieldPerMember_BoundToTheMember()
		{
			var target = Create<FixtureNumbersObject>();
			target.Position = new Vector3(1f, 2f, 3f);
			target.Size = new FixtureSize { Width = 0.5f, Count = 4 };
			var built = Build(target);

			var position = Slot(built, nameof(FixtureNumbersObject.Position)).Q<NumberLineField>();
			Assert.IsNotNull(position, "The vector is not drawn as numbers on one line.");
			Assert.AreEqual("Position", position.label);
			Assert.AreEqual(3, position.NumberFields.Count);
			CollectionAssert.AreEqual(new[] { "Position.x", "Position.y", "Position.z" }, position.NumberFields.Select(field => ((IBindable)field).bindingPath));
			CollectionAssert.AreEqual(new[] { "X", "Y", "Z" }, position.NumberFields.Select(field => ((FloatField)field).label));
			Pump(built);
			Assert.AreEqual(2f, ((FloatField)position.NumberFields[1]).value);

			Undo.IncrementCurrentGroup();
			((FloatField)position.NumberFields[1]).value = 7.5f;
			Pump(built);
			Assert.AreEqual(new Vector3(1f, 7.5f, 3f), target.Position, "Editing a number did not change its member.");
			Undo.PerformUndo();
			Assert.AreEqual(new Vector3(1f, 2f, 3f), target.Position, "Undo did not restore the member.");

			// A struct of our own: a float and an integer member, under their own names.
			var size = Slot(built, nameof(FixtureNumbersObject.Size)).Q<NumberLineField>();
			Assert.AreEqual("Size Of It", size.label);
			Assert.IsInstanceOf<FloatField>(size.NumberFields[0]);
			Assert.IsInstanceOf<IntegerField>(size.NumberFields[1]);
			Assert.AreEqual("Width", ((FloatField)size.NumberFields[0]).label);
			Assert.AreEqual(4, ((IntegerField)size.NumberFields[1]).value);

			// A struct with a member that is no number cannot be drawn this way.
			using (var serializedObject = new SerializedObject(Create<FixturePlainObject>()))
			{
				var notNumbers = serializedObject.FindProperty(nameof(FixturePlainObject.Struct));
				Assert.IsFalse(NumberLineField.CanDraw(notNumbers));
				Assert.Throws<ArgumentException>(() => new NumberLineField(notNumbers));
			}
			AssertExpectNoLogs();
		}

		[Test]
		public void List_ElementNumbersOnOneLine_DrawsEachElementAsNumbers()
		{
			var target = Create<FixtureNumbersObject>();
			target.Points.Add(new Vector3(1f, 2f, 3f));
			target.Points.Add(new Vector3(4f, 5f, 6f));
			var built = Build(target);
			built.ExpandAllLazyContent(3);
			Pump(built);

			var list = ListOf(built, nameof(FixtureNumbersObject.Points));
			var second = list.Query<NumberLineField>().ToList().FirstOrDefault(field => ((IBindable)field.NumberFields[0]).bindingPath == "Points.Array.data[1].x");
			Assert.IsNotNull(second, "The second element is not drawn as numbers on one line.");
			Assert.IsTrue(string.IsNullOrEmpty(second.label), "A numbered element has a label of its own.");
			Assert.AreEqual(6f, ((FloatField)second.NumberFields[2]).value);
			Assert.IsNull(list.Q<VisualElement>(className: ExtListView.RowUssClassName + "--foldout"), "A row of numbers is treated as a foldout row.");
			AssertExpectNoLogs();
		}

		[Test]
		public void List_AsTable_WithDeclaredColumns_UsesTheirTitlesWidthsAndCells()
		{
			var target = Create<FixtureNumbersObject>();
			target.Rows.Add(new FixtureNumbersRow { Name = "first", Offset = new Vector3(1f, 2f, 3f) });
			SetExpanded(target, nameof(FixtureNumbersObject.Rows), true);
			var built = Build(target);
			built.ExpandAllLazyContent(3);
			Pump(built);

			var view = built.Root.Q<ExtTableView>().Q<MultiColumnListView>();
			Assert.IsNotNull(view, "Opening the table did not build its content.");
			CollectionAssert.AreEqual(new[] { "Name", "Offset" }, view.columns.Select(column => column.name));
			CollectionAssert.AreEqual(new[] { "Row", "Offset (X Y Z)" }, view.columns.Select(column => column.title));
			Assert.AreEqual(view.columns[0].width.value * 2f, view.columns[1].width.value, 0.001f, "The column with twice the share is not twice as wide.");
			Assert.AreEqual(90f, view.columns[1].minWidth.value);

			var numbers = view.Q<NumberLineField>();
			Assert.IsNotNull(numbers, "The cell of the numbers column is not drawn as numbers on one line.");
			Assert.AreEqual("Rows.Array.data[0].Offset.y", ((IBindable)numbers.NumberFields[1]).bindingPath);
			Assert.IsTrue(string.IsNullOrEmpty(((FloatField)numbers.NumberFields[1]).label), "A table cell repeats the member names.");
			Assert.AreEqual(2f, ((FloatField)numbers.NumberFields[1]).value);

			Assert.Throws<ArgumentException>(() => new TableBuilder<FixtureNumbersRow>().Column(x => x.Name.Length), "A column for something that is no serialized field was accepted.");
			AssertExpectNoLogs();
		}

		[Test]
		public void List_WithoutElements_CarriesTheEmptyClass_UntilItGetsOne()
		{
			var target = Create<FixtureNumbersObject>();
			SetExpanded(target, nameof(FixtureNumbersObject.Numbers), true);
			var built = Build(target);
			Pump(built);
			var list = ListOf(built, nameof(FixtureNumbersObject.Numbers));
			Assert.IsTrue(list.ClassListContains(ExtListView.EmptyUssClassName), "An empty list is not marked as empty.");

			list.AddElement();
			Pump(built);
			Assert.AreEqual(1, target.Numbers.Count);
			Assert.IsFalse(list.ClassListContains(ExtListView.EmptyUssClassName), "A list with an element is still marked as empty.");

			list.RemoveElement(0);
			Pump(built);
			Assert.IsTrue(list.ClassListContains(ExtListView.EmptyUssClassName), "The list is not marked as empty again after its last element was removed.");
			AssertExpectNoLogs();
		}

		[Test]
		public void List_ShowIndex_MarksItsRows_SoThatTheBodyOfAFoldoutElementStartsUnderTheIndex()
		{
			var target = Create<FixtureNumbersObject>();
			target.Structs.Add(new FixtureStruct { Name = "a" });
			var built = Build(target);
			built.ExpandAllLazyContent(3);
			Pump(built);
			var row = ListOf(built, nameof(FixtureNumbersObject.Structs)).Q<VisualElement>(className: ExtListView.RowUssClassName);
			Assert.IsNotNull(row);
			Assert.IsTrue(row.ClassListContains(ExtListView.IndexedRowUssClassName), "A row that shows its index is not marked.");
			Assert.IsTrue(row.ClassListContains(ExtListView.RowUssClassName + "--foldout"));

			// A list without ShowIndex keeps the body under the title.
			var plain = Create<FixtureObject>();
			plain.NestedList.Add(new FixtureNested());
			var builtPlain = Build(plain);
			builtPlain.ExpandAllLazyContent(3);
			Pump(builtPlain);
			foreach (var plainRow in builtPlain.Root.Query<VisualElement>(className: ExtListView.RowUssClassName).ToList())
			{
				var owner = plainRow.GetFirstAncestorOfType<ExtListView>();
				Assert.AreEqual(owner.Options.ShowIndex, plainRow.ClassListContains(ExtListView.IndexedRowUssClassName), $"A row of '{owner.ArrayPropertyPath}' is marked against its list's ShowIndex option.");
			}
			AssertExpectNoLogs();
		}

		[Test]
		public void Button_ThatDestroysItsTarget_Runs_AndHasItsSpaceBefore()
		{
			var target = Create<FixtureNumbersObject>();
			var built = Build(target);
			var button = built.Root.Query<Button>().Where(candidate => candidate.text == FixtureNumbersObjectInspector.DestroyButtonLabel).First();
			Assert.IsNotNull(button);
			Assert.AreEqual(10f, button.style.marginTop.value.value, "The button has no space before it.");

			InspectorTestTools.Click(button);
			Assert.IsTrue(target == null, "The button did not run its action.");
			AssertExpectNoLogs();
		}

		#endregion

		#region Groups, messages, live values

		[Test]
		public void Group_HoldsItsFields_AndItsMessageFollowsTheRule()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			var group = built.Root.Q<BoxGroup>(name: "ext-group-Grouped");
			Assert.IsNotNull(group);
			Assert.IsNotNull(group.Q(name: "ext-field-GroupedA"));
			Assert.IsNotNull(group.Q(name: "ext-field-GroupedB"));
			var message = group.Q<MessageBox>();
			Assert.IsFalse(IsDisplayed(message));

			target.GroupedA = 1;
			built.RootInstance.Reevaluate();
			Assert.IsTrue(IsDisplayed(message));
		}

		[Test]
		public void Group_OnAnyChanged_RunsForEachOfItsFields()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			Pump(built);
			built.SerializedObject.FindProperty(nameof(FixtureObject.GroupedB)).intValue = 3;
			built.SerializedObject.ApplyModifiedProperties();
			Pump(built);
			Assert.AreEqual(1, target.GroupChangeCount);
		}

		[Test]
		public void Placement_Tabs_CustomAndImguiNodes()
		{
			var target = Create<FixtureObject>();
			target.name = "placed";
			var built = Build(target);

			var order = new List<string>();
			built.Root.Query<PropertyField>().ForEach(field =>
			{
				if (!field.bindingPath.Contains("."))
					order.Add(field.bindingPath);
			});
			Assert.AreEqual(order.IndexOf(nameof(FixtureObject.First)) - 1, order.IndexOf(nameof(FixtureObject.PlacedBeforeFirst)));
			Assert.AreEqual(order.IndexOf(nameof(FixtureObject.First)) + 1, order.IndexOf(nameof(FixtureObject.PlacedAfterFirst)));

			// What is placed at the top comes after the Script field and before the fields that stay in place:
			// first the fields placed there, then the nodes, each in the order they were declared.
			var custom = built.Root.Q<Label>(name: "custom-node");
			Assert.AreEqual("custom for placed", custom.text);
			Assert.AreEqual(built.Root.IndexOf(Slot(built, nameof(FixtureObject.MovedToTop))) + 1, built.Root.IndexOf(custom));
			Assert.Less(built.Root.IndexOf(custom), built.Root.IndexOf(Slot(built, nameof(FixtureObject.PlacedBeforeFirst))));

			var tabs = built.Root.Q<TabView>();
			Assert.IsNotNull(tabs);
			CollectionAssert.AreEqual(new[] { "Tab A", "Tab B" }, tabs.Query<Tab>().ToList().Select(tab => tab.label));
			Assert.IsNotNull(tabs.Q(name: "ext-field-TabbedA"));
			Assert.IsNotNull(tabs.Q(name: "ext-field-TabbedB"));

			Assert.IsNotNull(built.Root.Q<IMGUIContainer>(className: "ext-imgui"));
		}

		[Test]
		public void InlineObjectEditor_ShowsTheInspectorOfTheReferencedObject()
		{
			var target = Create<FixtureObject>();
			var linked = Create<FixtureSectionsObject>();
			target.Linked = linked;
			var built = Build(target);
			var inline = built.Root.Q<InlineObjectEditor>();
			Assert.AreSame(linked, inline.Field.value);
			built.ExpandAllLazyContent(3);
			Assert.IsNotNull(inline.Q<InspectorElement>(), "The inspector of the referenced object was not built.");

			// The object field above the inspector names the object and its type: no "Script" row below it,
			// while the object's own inspector keeps the row.
			var owner = Create<FixtureObject>();
			var shownInPlace = Create<FixtureObject>();
			owner.Linked = shownInPlace;
			var builtOwner = Build(owner);
			builtOwner.ExpandAllLazyContent(3);
			builtOwner.Update();
			var scriptFields = builtOwner.Root.Query<PropertyField>().Where(field => field.bindingPath == "m_Script").ToList();
			Assert.AreEqual(2, scriptFields.Count, "Expected the script row of the object and the one of the object shown in place.");
			var inlineInspector = builtOwner.Root.Q<InlineObjectEditor>().Q<InspectorElement>();
			foreach (var scriptField in scriptFields)
			{
				var isInsideInlineInspector = inlineInspector.Contains(scriptField);
				Assert.AreEqual(!isInsideInlineInspector, IsDisplayed(scriptField), isInsideInlineInspector ? "The script row of the inline inspector is shown." : "The script row of the object itself is hidden.");
			}

			// An object that refers to itself is not nested forever.
			var selfReferring = Create<FixtureObject>();
			selfReferring.Linked = selfReferring;
			var builtSelf = Build(selfReferring);
			builtSelf.ExpandAllLazyContent(3);
			Assert.AreEqual(1, builtSelf.Root.Query<InspectorElement>().ToList().Count, "The inner editor showed the same object again.");
		}

		[Test]
		public void Info_WithComputedText_HidesWhenTheTextIsEmpty()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			var message = built.Root.Query<MessageBox>().Where(box => box.Severity == MessageSeverity.Warning).First();
			Assert.IsFalse(IsDisplayed(message));

			target.Details = 101;
			built.RootInstance.Reevaluate();
			Assert.IsTrue(IsDisplayed(message));
			Assert.AreEqual("Details is large.", message.Text);
		}

		[Test]
		public void LiveValue_ChangesAfterRefresh()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			var live = built.Root.Query<LiveValueField>().Where(field => field.label == "Live Counter").First();
			Assert.AreEqual("0", live.value);

			target.LiveCounter = 12;
			Assert.AreEqual("0", live.value);
			built.RootInstance.RefreshLiveValues();
			Assert.AreEqual("12", live.value);

			// And by itself, at the live interval.
			target.LiveCounter = 13;
			Pump(built);
			Assert.AreEqual("13", live.value);
		}

		[Test]
		public void LiveField_ShowsAndSetsTheValue()
		{
			var target = Create<FixtureObject>();
			target.LiveEditable = 1.5f;
			var built = Build(target);
			var field = built.Root.Query<FloatField>().Where(candidate => candidate.label == "Live Editable").First();
			Assert.AreEqual(1.5f, field.value);

			field.value = 4f;
			Assert.AreEqual(4f, target.LiveEditable);
		}

		[Test]
		public void LiveField_EnabledWhen_DisablesItWithATooltip_AndStillShowsTheValue()
		{
			var target = Create<FixtureObject>();
			target.LiveEditable = 1.5f;
			var built = Build(target);
			var field = built.Root.Query<FloatField>().Where(candidate => candidate.label == "Live Gated").First();
			Assert.IsFalse(field.enabledSelf, "The live field is enabled although its rule does not hold.");
			Assert.AreEqual("Needs details.", field.tooltip);
			Assert.AreEqual(1.5f, field.value, "A disabled live field does not show the value.");

			// A disabled field keeps following the value.
			target.LiveEditable = 2.5f;
			built.RootInstance.RefreshLiveValues();
			Assert.AreEqual(2.5f, field.value);

			// The rule is read at the live interval: no data change is needed.
			target.ShowDetails = true;
			built.RootInstance.RefreshLiveValues();
			Assert.IsTrue(field.enabledSelf);
			Assert.AreEqual("", field.tooltip);
			field.value = 4f;
			Assert.AreEqual(4f, target.LiveEditable);

			// A read-only live value has nothing to enable.
			Assert.Throws<InvalidOperationException>(() => new LiveNode<FixtureObject, int> { Label = "Read Only", Getter = _ => 0 }.EnabledWhen((t, context) => true));
		}

		[Test]
		public void LiveValue_Tint_ColoursTheValue_AndNullGoesBackToTheDefault_AndTheRangeIsABar()
		{
			var target = Create<FixtureObject>();
			target.LiveCounter = 2;
			var built = Build(target);
			var live = built.Root.Query<LiveValueField>().Where(field => field.label == "Live Tinted").First();
			Assert.IsNull(live.Tint);
			Assert.IsTrue(live.IsBarShown);
			Assert.AreEqual(0f, live.BarMin);
			Assert.AreEqual(10f, live.BarMax);
			Assert.AreEqual(0.2f, live.BarFraction, 0.0001f);
			Assert.AreEqual("2", live.Text);

			target.LiveCounter = 8;
			built.RootInstance.RefreshLiveValues();
			Assert.AreEqual(Color.green, live.Tint);
			Assert.AreEqual(0.8f, live.BarFraction, 0.0001f);

			target.LiveCounter = 1;
			built.RootInstance.RefreshLiveValues();
			Assert.IsNull(live.Tint);

			// A value without a range has no bar.
			Assert.IsFalse(built.Root.Query<LiveValueField>().Where(field => field.label == "Live Counter").First().IsBarShown);
		}

		[Test]
		public void Button_VisibleWhen_ShowsTheButtonOnlyWhileItsRuleHolds()
		{
			var target = Create<FixtureObject>();
			var built = Build(target);
			var button = built.Root.Query<Button>().Where(candidate => candidate.text == "Shown With Details").First();
			Assert.IsFalse(IsDisplayed(button));

			target.ShowDetails = true;
			built.RootInstance.Reevaluate();
			Assert.IsTrue(IsDisplayed(button));

			target.ShowDetails = false;
			built.RootInstance.Reevaluate();
			Assert.IsFalse(IsDisplayed(button));
		}

		#endregion

		#region Several objects

		[Test]
		public void MultiObject_FieldIsShownWhenAnyTargetWantsIt()
		{
			var first = Create<FixtureObject>();
			var second = Create<FixtureObject>();
			var built = Build(first, second);
			Assert.IsTrue(built.SerializedObject.isEditingMultipleObjects);
			var slot = Slot(built, nameof(FixtureObject.Details));
			Assert.IsFalse(IsDisplayed(slot));

			second.ShowDetails = true;
			built.RootInstance.Reevaluate();
			Assert.IsTrue(IsDisplayed(slot), "Data that one of the objects has was hidden.");

			// Enabled needs every target to allow it.
			Assert.IsFalse(Slot(built, nameof(FixtureObject.Gated)).Q<PropertyField>().enabledSelf);
			first.ShowDetails = true;
			built.RootInstance.Reevaluate();
			Assert.IsTrue(Slot(built, nameof(FixtureObject.Gated)).Q<PropertyField>().enabledSelf);
		}

		[Test]
		public void MultiObject_NestedInspector_EvaluatesEveryTarget_AndItsButtonRunsOnAll()
		{
			var first = Create<FixtureObject>();
			var second = Create<FixtureObject>();
			second.Nested.Kind = FixtureKind.Daily;
			var built = Build(first, second);
			built.ExpandAllLazyContent(3);
			var nestedField = built.Root.Query<PropertyField>().Where(field => field.bindingPath == nameof(FixtureObject.Nested)).First();
			var slot = nestedField.Q(name: "ext-field-OnlyForDaily");
			Assert.IsNotNull(slot);
			Assert.IsTrue(IsDisplayed(slot), "A field one of the objects has was hidden in the nested inspector.");

			InspectorTestTools.Click(nestedField.Query<Button>().Where(candidate => candidate.text == "Stamp Version").First());
			Assert.AreEqual("stamped", first.Nested.Version);
			Assert.AreEqual("stamped", second.Nested.Version);
			Assert.AreEqual(FixtureKind.Fixed, first.Nested.Kind, "Writing back to one object carried data of the other.");
			Assert.AreEqual(FixtureKind.Daily, second.Nested.Kind);
		}

		[Test]
		public void MultiObject_ValidationShowsTheWorst_ButtonsRunOnAll_LiveValuesNeedOne()
		{
			var first = Create<FixtureObject>();
			var second = Create<FixtureObject>();
			first.Required = "x";
			second.Required = "x";
			second.Limited = 11;
			second.Details = 9;
			first.Details = 8;
			var built = Build(first, second);
			CollectionAssert.AreEqual(new[] { "Limited must be at most 10." }, MessagesOf(built));

			InspectorTestTools.Click(built.Root.Query<Button>().Where(candidate => candidate.text == "Reset Details").First());
			Assert.AreEqual(0, first.Details);
			Assert.AreEqual(0, second.Details);

			var live = built.Root.Query<LiveValueField>().Where(field => field.label == "Live Counter").First();
			StringAssert.Contains("multiple objects", live.value);
		}

		#endregion

		#region Sections and lazy content

		[Test]
		public void Sections_BuildWhenOpened_DropWhenClosed_AndFilter()
		{
			var target = Create<FixtureSectionsObject>();
			SetExpanded(target, nameof(FixtureSectionsObject.First), false);
			SetExpanded(target, nameof(FixtureSectionsObject.Second), false);
			var built = Build(target);
			var sections = built.Root.Query<SectionFoldout>().ToList();
			CollectionAssert.AreEquivalent(new[] { "First", "Second" }, sections.Select(section => section.text));
			var first = sections.First(section => section.text == "First");
			Assert.IsFalse(first.IsContentBuilt);
			Assert.IsNull(built.Root.Query<PropertyField>().Where(field => field.bindingPath == "m_Script").First(), "The Script field was not hidden.");

			built.SerializedObject.FindProperty(nameof(FixtureSectionsObject.First)).isExpanded = true;
			Pump(built);
			Assert.IsTrue(first.IsContentBuilt);
			// The section is the header: the nested inspector inside has none of its own.
			Assert.IsNull(first.contentContainer.Q<Foldout>());
			Assert.IsNotNull(first.contentContainer.Q(name: "ext-field-OnlyForDaily"));

			built.SerializedObject.FindProperty(nameof(FixtureSectionsObject.First)).isExpanded = false;
			Pump(built);
			Assert.IsFalse(first.IsContentBuilt);
			Assert.AreEqual(0, first.contentContainer.childCount);

			var search = built.Root.Q<ToolbarSearchField>(name: "ext-section-search");
			search.value = "second";
			Assert.IsTrue(first.ClassListContains("ext-search-hidden"));
			Assert.IsFalse(sections.First(section => section.text == "Second").ClassListContains("ext-search-hidden"));
			// A section is also found by the name of a field inside it.
			search.value = "only for daily";
			Assert.IsFalse(first.ClassListContains("ext-search-hidden"));
			search.value = "";
			Assert.IsFalse(first.ClassListContains("ext-search-hidden"));
		}

		[Test]
		public void Sections_AValueWithADrawerOfItsOwn_IsAField()
		{
			var target = Create<FixtureSectionsObject>();
			var built = Build(target);
			Assert.IsNull(built.Root.Query<SectionFoldout>().Where(section => section.bindingPath == nameof(FixtureSectionsObject.WrappedId)).First(),
				"A struct that is drawn as one value by its own drawer became a section.");
			Assert.IsNotNull(built.Root.Query<PropertyField>().Where(field => field.bindingPath == nameof(FixtureSectionsObject.WrappedId)).First());
		}

		[Test]
		public void FieldNotDrawnByAPropertyField_KeepsItsHeaderSpaceAndTooltip()
		{
			var target = Create<FixtureSectionsObject>();
			var built = Build(target);
			var slot = built.Root.Q(name: "ext-field-" + nameof(FixtureSectionsObject.DecoratedNames));
			Assert.IsNotNull(slot);
			var decorators = slot.Q(className: FieldDecorators.UssClassName);
			Assert.IsNotNull(decorators, "The Header and Space attributes of a list drawn by the toolbox are lost.");
			Assert.AreEqual(0, slot.IndexOf(decorators), "The decorators are not above the field.");
			Assert.AreEqual(2, decorators.childCount);
			Assert.IsTrue(decorators[0].ClassListContains(FieldDecorators.SpaceUssClassName), "The space is not drawn first, as declared.");
			Assert.AreEqual(7f, decorators[0].style.height.value.value);
			Assert.AreEqual("Decorated", decorators[1].Q<Label>().text);

			var list = slot.Q<ExtListView>();
			Assert.AreEqual("The decorated names.", list.Q<Toggle>(className: Foldout.toggleUssClassName).tooltip, "The tooltip of the field is not on the header of its list.");

			// A field without decorators gets none.
			Assert.IsNull(built.Root.Q(name: "ext-field-" + nameof(FixtureSectionsObject.WrappedIds)).Q(className: FieldDecorators.UssClassName));
		}

		[Test]
		public void List_ShowIndex_AnElementDrawnAsOneValueByItsOwnDrawer_HasNoElementLabel()
		{
			var target = Create<FixtureSectionsObject>();
			target.WrappedIds.Add(default);
			var built = Build(target);
			built.ExpandAllLazyContent(3);
			var list = ListOf(built, nameof(FixtureSectionsObject.WrappedIds));
			var rowField = list.Query<PropertyField>().Where(field => field.bindingPath == "WrappedIds.Array.data[0]").First();
			Assert.IsNotNull(rowField);
			Assert.AreEqual("", rowField.label, "The row shows 'Element 0' next to its index.");
		}

		[Test]
		public void ExpandAllLazyContent_BuildsEverything_WithoutErrors_AndRestoresExpandedStates()
		{
			var target = Create<FixtureObject>();
			target.Required = "x";
			target.NestedList.Add(new FixtureNested { Id = "in list" });
			target.LabelledList.Add(new FixtureNested { Id = "labelled" });
			target.Names.Add("name");
			target.PickedNames.Add("A");
			target.Shapes.Add(new FixtureCircle());
			target.Shapes.Add(null);
			target.Linked = Create<FixturePlainObject>();
			SetExpanded(target, nameof(FixtureObject.Nested), false);

			using (var errors = InspectorTestTools.CollectErrorLogs())
			{
				var built = InspectorTestTools.Build(target);
				try
				{
					built.ExpandAllLazyContent(3);
					built.ReevaluateAll();
					built.RefreshAllLiveValues();
					Assert.Greater(built.GetAllInstances().Count, 5);
					Assert.IsTrue(built.GetAllInstances().All(instance => instance.IsBodyBuilt), "A nested inspector was left unbuilt.");
					CollectionAssert.IsEmpty(built.GetAllMessages().Select(message => message.Message));
				}
				finally
				{
					built.Dispose();
				}
				CollectionAssert.IsEmpty(errors.Errors);
			}
			Assert.IsFalse(new SerializedObject(target).FindProperty(nameof(FixtureObject.Nested)).isExpanded, "The expanded state was not put back.");
		}

		[Test]
		public void BuiltInspector_RemembersItsDrawersInTheCacheOfItsEditor_AndLeavesNothingInUnitysGlobalCache()
		{
			var target = Create<FixtureObject>();
			target.Required = "x";
			target.NestedList.Add(new FixtureNested { Id = "in list" });
			target.LabelledList.Add(new FixtureNested { Id = "labelled" });
			target.Names.Add("apple");
			target.Names.Add("pear");
			target.PickedNames.Add("A");
			target.Shapes.Add(new FixtureCircle());
			target.Table.Add(default);
			target.Linked = Create<FixturePlainObject>();
			SetExpanded(target, nameof(FixtureObject.Nested), false);

			var rememberedBefore = CountDrawersInUnitysGlobalCache();
			var selectedBefore = PropertyHandlerCacheScope.Selected;

			var built = Build(target);
			built.ExpandAllLazyContent(3);
			Pump(built);
			built.ReevaluateAll();
			built.RefreshAllLiveValues();
			Assert.Greater(built.GetAllInstances().Count, 5, "The test needs nested inspectors: Unity finds their drawers through its cache.");
			// What a test or a person does between the updates: the rows of a list are bound again.
			var list = ListOf(built, nameof(FixtureObject.Names));
			list.SearchText = "apple";
			list.SearchText = "";
			Pump(built);

			// What is remembered there outlives the inspector and its object, and can be handed to a property
			// of another object later: Unity finds an entry by the instance id xor a hash of the property path.
			Assert.AreEqual(rememberedBefore, CountDrawersInUnitysGlobalCache(), "Building the inspector left drawers in Unity's global property handler cache.");
			Assert.AreSame(selectedBefore, PropertyHandlerCacheScope.Selected, "The property handler cache that was selected before was not selected again.");
		}

		private static int CountDrawersInUnitysGlobalCache()
		{
			const BindingFlags Any = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			var utilityType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ScriptAttributeUtility", true);
			var cacheField = utilityType.GetField("s_GlobalCache", Any)
			                 ?? throw new MissingFieldException("UnityEditor.ScriptAttributeUtility", "s_GlobalCache");
			var cache = cacheField.GetValue(null);
			var handlersField = cache.GetType().GetField("m_PropertyHandlers", Any)
			                    ?? throw new MissingFieldException("UnityEditor.PropertyHandlerCache", "m_PropertyHandlers");
			return ((System.Collections.ICollection)handlersField.GetValue(cache)).Count;
		}

		[Test]
		public void ExpandAllLazyContent_ReachesTheElementsOfAUnityDefaultList_ThatWasClosed()
		{
			// An object without a custom editor: Unity's own foldouts and list, with nested inspectors inside.
			var target = Create<Demo.InspectorToolboxPlainDemo>();
			SetExpanded(target, nameof(Demo.InspectorToolboxPlainDemo.Items), false);
			SetExpanded(target, nameof(Demo.InspectorToolboxPlainDemo.Items) + ".Array.data[0]", false);
			SetExpanded(target, nameof(Demo.InspectorToolboxPlainDemo.Item), false);

			var built = Build(target);
			Assert.IsNull(built.RootInstance, "The fixture must be an object that is not drawn by a toolbox editor.");
			built.ExpandAllLazyContent(3);

			var paths = built.GetAllInstances().Select(instance => instance.Context.PropertyPath).ToList();
			CollectionAssert.Contains(paths, "Item");
			CollectionAssert.Contains(paths, "Items.Array.data[0]", "The elements of the closed list were not built.");
			CollectionAssert.Contains(paths, "Items.Array.data[1]");
			Assert.IsTrue(built.GetAllInstances().All(instance => instance.IsBodyBuilt), "A nested inspector was left unbuilt.");

			built.Dispose();
			using (var serializedObject = new SerializedObject(target))
			{
				Assert.IsFalse(serializedObject.FindProperty("Items").isExpanded, "The expanded state of the list was not put back.");
				Assert.IsFalse(serializedObject.FindProperty("Items.Array.data[0]").isExpanded, "The expanded state of an element was not put back.");
				Assert.IsFalse(serializedObject.FindProperty("Item").isExpanded, "The expanded state of a field was not put back.");
			}
		}

		[Test]
		public void Build_PutsBackEveryExpandedState_AlsoWhatAListThatIsOpenByDefaultOpened_AndWhatWasOpenedMeanwhile()
		{
			// The expanded state belongs to the type and outlives the object: what a build leaves open stays
			// open for every later object of the type, in tests and in the Inspector window.
			var target = Create<FixtureObject>();
			SetExpanded(target, nameof(FixtureObject.InitiallyOpen), false);
			SetExpanded(target, nameof(FixtureObject.Names), false);
			SetExpanded(target, nameof(FixtureObject.LabelledList), true);
			try
			{
				var built = InspectorTestTools.Build(target);
				Assert.IsTrue(built.SerializedObject.FindProperty(nameof(FixtureObject.InitiallyOpen)).isExpanded, "The list that is open by default was not opened.");
				SetExpanded(target, nameof(FixtureObject.Names), true);
				SetExpanded(target, nameof(FixtureObject.LabelledList), false);
				built.Dispose();

				using (var serializedObject = new SerializedObject(target))
				{
					Assert.IsFalse(serializedObject.FindProperty(nameof(FixtureObject.InitiallyOpen)).isExpanded, "What the list opened by default was not closed again.");
					Assert.IsFalse(serializedObject.FindProperty(nameof(FixtureObject.Names)).isExpanded, "What was opened after the build was not closed again.");
					Assert.IsTrue(serializedObject.FindProperty(nameof(FixtureObject.LabelledList)).isExpanded, "What was open before the build was not opened again.");
				}
			}
			finally
			{
				SetExpanded(target, nameof(FixtureObject.LabelledList), false);
			}
		}

		[Test]
		public void KeepExpandedStates_CollapseAllClosesEverything_AndDisposingPutsBackWhatWasOpen()
		{
			var target = Create<FixtureObject>();
			target.LabelledList.Add(new FixtureNested { Id = "a" });
			target.LabelledList.Add(new FixtureNested { Id = "b" });
			const string List = nameof(FixtureObject.LabelledList);
			const string SecondRow = List + ".Array.data[1]";
			SetExpanded(target, List, true);
			SetExpanded(target, SecondRow, true);
			SetExpanded(target, List + ".Array.data[0]", false);
			SetExpanded(target, nameof(FixtureObject.Nested), false);
			try
			{
				var kept = InspectorTestTools.KeepExpandedStates(target);
				kept.CollapseAll();
				using (var serializedObject = new SerializedObject(target))
				{
					Assert.IsFalse(serializedObject.FindProperty(List).isExpanded);
					Assert.IsFalse(serializedObject.FindProperty(SecondRow).isExpanded, "An element inside a list was not closed.");
				}
				SetExpanded(target, nameof(FixtureObject.Nested), true);
				kept.Dispose();

				using (var serializedObject = new SerializedObject(target))
				{
					Assert.IsTrue(serializedObject.FindProperty(List).isExpanded);
					Assert.IsTrue(serializedObject.FindProperty(SecondRow).isExpanded);
					Assert.IsFalse(serializedObject.FindProperty(List + ".Array.data[0]").isExpanded);
					Assert.IsFalse(serializedObject.FindProperty(nameof(FixtureObject.Nested)).isExpanded, "What was opened meanwhile was not closed again.");
				}
				Assert.Throws<ObjectDisposedException>(() => kept.CollapseAll());
			}
			finally
			{
				SetExpanded(target, List, false);
				SetExpanded(target, SecondRow, false);
			}
		}

		[Test]
		public void ExpandAllLazyContent_BuildsTheSame_WhateverWasOpenBefore_AndOnlyTheFirstRowsOfALongList()
		{
			const string List = nameof(FixtureObject.LabelledList);
			List<string> BuildAndListInstances(bool openBefore, out int elementCount)
			{
				var target = Create<FixtureObject>();
				target.Required = "x";
				for (var i = 0; i < 12; i++)
					target.LabelledList.Add(new FixtureNested { Id = "element " + i });
				SetExpanded(target, List, openBefore);
				for (var i = 0; i < 12; i++)
					SetExpanded(target, $"{List}.Array.data[{i}]", openBefore);
				SetExpanded(target, nameof(FixtureObject.Nested), openBefore);

				var built = InspectorTestTools.Build(target);
				try
				{
					built.ExpandAllLazyContent(3);
					// Nothing may appear later either: a foldout that opens a moment after the call, a list view that fills up.
					Pump(built);
					elementCount = built.Root.Query<VisualElement>().ToList().Count;
					Assert.IsTrue(built.GetAllInstances().All(instance => instance.IsBodyBuilt), "A nested inspector was left unbuilt.");
					return built.GetAllInstances().Select(instance => instance.Context.PropertyPath).OrderBy(path => path, StringComparer.Ordinal).ToList();
				}
				finally
				{
					built.Dispose();
					SetExpanded(target, List, false);
					for (var i = 0; i < 12; i++)
						SetExpanded(target, $"{List}.Array.data[{i}]", false);
					SetExpanded(target, nameof(FixtureObject.Nested), false);
				}
			}

			var fromClosed = BuildAndListInstances(false, out var countFromClosed);
			var fromOpen = BuildAndListInstances(true, out var countFromOpen);
			CollectionAssert.AreEqual(fromClosed, fromOpen, "What is built depends on what was open before.");
			Assert.AreEqual(countFromClosed, countFromOpen, "The element tree depends on what was open before.");
			CollectionAssert.Contains(fromClosed, List + ".Array.data[2]");
			CollectionAssert.DoesNotContain(fromClosed, List + ".Array.data[3]", "A row past the limit was built.");
		}

		[Test]
		public void ExpandAllLazyContent_WithAFilter_OpensOnlyTheNamedProperties()
		{
			const string List = nameof(FixtureObject.LabelledList);
			var target = Create<FixtureObject>();
			target.Required = "x";
			target.LabelledList.Add(new FixtureNested { Id = "a" });
			target.LabelledList.Add(new FixtureNested { Id = "b" });
			SetExpanded(target, List, false);
			SetExpanded(target, List + ".Array.data[0]", false);
			SetExpanded(target, List + ".Array.data[1]", false);
			SetExpanded(target, nameof(FixtureObject.Nested), false);

			var built = Build(target);
			var opened = new HashSet<string> { List, List + ".Array.data[1]" };
			built.ExpandAllLazyContent(3, null, opened.Contains);

			var builtPaths = built.GetAllInstances().Where(instance => instance.IsBodyBuilt && instance != built.RootInstance).Select(instance => instance.Context.PropertyPath).ToList();
			CollectionAssert.Contains(builtPaths, List + ".Array.data[1]");
			CollectionAssert.DoesNotContain(builtPaths, List + ".Array.data[0]", "A row that was not named was opened.");
			CollectionAssert.DoesNotContain(builtPaths, nameof(FixtureObject.Nested), "A field that was not named was opened.");
			Assert.IsTrue(built.SerializedObject.FindProperty(List + ".Array.data[1]").isExpanded);
			Assert.IsFalse(built.SerializedObject.FindProperty(List + ".Array.data[0]").isExpanded);
		}

		[Test]
		public void List_WhoseContentWasForced_KeepsItsRowsUpToDate_WhenTheListChangesOrIsSearched()
		{
			var target = Create<FixtureObject>();
			target.Required = "x";
			target.Names.Add("apple");
			target.Names.Add("pear");
			var built = Build(target);
			var list = built.Root.Query<ExtListView>().Where(candidate => candidate.ArrayPropertyPath == nameof(FixtureObject.Names)).First();
			Assert.IsNotNull(list);
			list.ForceBuildContent(3);
			built.Update();
			int RowCount() => list.Query<VisualElement>(className: ExtListView.RowUssClassName).ToList().Count;
			Assert.AreEqual(2, RowCount());

			list.AddElement();
			list.AddElement();
			built.Update();
			Assert.AreEqual(4, built.SerializedObject.FindProperty(nameof(FixtureObject.Names)).arraySize);
			Assert.AreEqual(3, RowCount(), "The rows are limited to the number that was asked for.");

			list.SearchText = "apple";
			built.Update();
			CollectionAssert.AreEqual(new[] { 0 }, list.VisibleIndices);
			Assert.AreEqual(1, RowCount(), "The rows did not follow the search.");

			list.SearchText = "";
			list.RemoveElement(0);
			built.Update();
			Assert.AreEqual(3, RowCount());
			Assert.IsTrue(list.ForceBuildContent(5), "Asking for more rows built nothing.");
			Assert.IsFalse(list.ForceBuildContent(5), "Asking for the same rows again built them again.");
		}

		[Test]
		public void FailingRule_IsLoggedOnce_AndListed_AndDoesNotStopOtherRules()
		{
			var target = Create<FixtureObject>();
			target.Required = "x";
			target.LabelledList = null;
			var built = Build(target);
			// The list validator of the fixture reads the list, which is null on the live object now.
			built.RootInstance.Reevaluate();
			built.RootInstance.Reevaluate();
			Assert.IsTrue(MessagesOf(built).Any(message => message.StartsWith("Evaluating a rule failed", StringComparison.Ordinal)));
			Assert.IsTrue(built.GetAllMessages().Any(message => message.IsRuleFailure), "The failure is not marked as a failure of a rule.");
			Assert.IsTrue(built.GetAllMessages().Where(message => !message.Message.StartsWith("Evaluating ", StringComparison.Ordinal)).All(message => !message.IsRuleFailure), "A validation message is marked as a failure of a rule.");

			var errorLogs = Logs.Where(log => log.Type == LogType.Error || log.Type == LogType.Exception).ToList();
			Assert.AreEqual(2, errorLogs.Count, "The failure must be logged once: one line that says where, and the exception.");
			Logs.Clear();

			// Rules after the failing one still ran.
			target.ShowDetails = true;
			built.RootInstance.Reevaluate();
			Assert.IsTrue(IsDisplayed(Slot(built, nameof(FixtureObject.Details))));
			Logs.Clear();
		}

		#endregion

		#region Demo and documentation examples

		[TestCase(typeof(Demo.InspectorToolboxDemo))]
		[TestCase(typeof(Demo.InspectorToolboxListDemo))]
		[TestCase(typeof(Demo.InspectorToolboxSectionsDemo))]
		[TestCase(typeof(Demo.InspectorToolboxPlainDemo))]
		[TestCase(typeof(Cookbook.CookbookComponent))]
		public void DemoObject_BuildsCompletely_WithoutErrors(Type objectType)
		{
			var target = ScriptableObject.CreateInstance(objectType);
			CreatedObjects.Add(target);
			using (var errors = InspectorTestTools.CollectErrorLogs())
			{
				var built = Build(target);
				built.ExpandAllLazyContent(3);
				built.ReevaluateAll();
				built.RefreshAllLiveValues();
				Assert.Greater(built.Root.Query<VisualElement>().ToList().Count, 20);
				CollectionAssert.IsEmpty(errors.Errors);
			}
		}

		[Test]
		public void EveryRegisteredCustomizedInspector_BuildsItsBlueprint()
		{
			foreach (var type in InspectorTestTools.GetCustomizedInspectorTypes())
			{
				if (type.Namespace == null || !type.Namespace.StartsWith("ExtenityTests.", StringComparison.Ordinal))
					continue;
				InspectorBlueprint blueprint = null;
				Assert.DoesNotThrow(() => blueprint = InspectorTestTools.GetBlueprint(type), type.FullName);
				Assert.IsNotNull(blueprint);
			}
		}

		#endregion

		#region Styles

		[Test]
		public void StyleSheets_LoadByGuid_ForBothSkins()
		{
			Assert.IsNotNull(InspectorStyles.Base);
			Assert.IsNotNull(InspectorStyles.Dark);
			Assert.IsNotNull(InspectorStyles.Light);
			Assert.AreSame(EditorGUIUtility.isProSkin ? InspectorStyles.Dark : InspectorStyles.Light, InspectorStyles.Skin);

			var element = new VisualElement();
			InspectorStyles.Apply(element);
			InspectorStyles.Apply(element);
			Assert.IsTrue(element.ClassListContains(InspectorStyles.RootClass));
			Assert.AreEqual(2, element.styleSheets.count);
		}

		[Test]
		public void StyleSheets_EveryVariableInUse_IsDefinedForBothSkins()
		{
			var baseText = ReadStyleSheet(InspectorStyles.BaseStyleSheetGuid);
			var darkText = ReadStyleSheet(InspectorStyles.DarkStyleSheetGuid);
			var lightText = ReadStyleSheet(InspectorStyles.LightStyleSheetGuid);

			var used = new HashSet<string>(Regex.Matches(baseText, @"var\(\s*(--ext-[a-z0-9-]+)").Cast<Match>().Select(match => match.Groups[1].Value));
			// Read from code by the accent group.
			used.Add("--ext-accent-bg-alpha");
			used.Add("--ext-accent-header-alpha");
			Assert.Greater(used.Count, 20);

			var definition = new Regex(@"(--ext-[a-z0-9-]+)\s*:");
			var definedInBase = new HashSet<string>(definition.Matches(baseText).Cast<Match>().Select(match => match.Groups[1].Value));
			var definedInDark = new HashSet<string>(definition.Matches(darkText).Cast<Match>().Select(match => match.Groups[1].Value));
			var definedInLight = new HashSet<string>(definition.Matches(lightText).Cast<Match>().Select(match => match.Groups[1].Value));

			foreach (var variable in used)
			{
				if (definedInBase.Contains(variable))
					continue;
				Assert.IsTrue(definedInDark.Contains(variable), $"'{variable}' is used but not defined for the dark skin.");
				Assert.IsTrue(definedInLight.Contains(variable), $"'{variable}' is used but not defined for the light skin.");
			}
			CollectionAssert.AreEquivalent(definedInDark, definedInLight, "The two skin sheets must define the same variables.");
			// The skin sheets hold variables only.
			Assert.AreEqual(1, Regex.Matches(darkText, @"\{").Count);
			Assert.AreEqual(1, Regex.Matches(lightText, @"\{").Count);
		}

		private static string ReadStyleSheet(string guid)
		{
			var path = AssetDatabase.GUIDToAssetPath(guid);
			Assert.IsFalse(string.IsNullOrEmpty(path), $"No asset with GUID '{guid}'.");
			return File.ReadAllText(Path.GetFullPath(path));
		}

		#endregion
	}

}
