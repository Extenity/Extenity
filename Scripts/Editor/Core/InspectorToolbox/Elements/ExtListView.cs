using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// An element that offers a place for the validation messages of the field it draws, when under the whole
	/// element is not the right place.
	/// </summary>
	public interface IValidationMessageHost
	{
		VisualElement ValidationMessageContainer { get; }
	}

	/// <summary>
	/// A list for an array or list property: a foldout header that follows the property's expanded state, the
	/// element count, optional search, index labels and element labels, drag reordering and the add and remove
	/// buttons. With <see cref="ListOptions.ManagedReferences"/> it edits a SerializeReference list: the add
	/// button opens a type picker and every row can change its type.
	/// <para>
	/// The rows are drawn with property fields, so each element is drawn by the drawer of its type, and undo,
	/// prefab override marks and the context menu of each element work as everywhere else.
	/// </para>
	/// </summary>
	public class ExtListView : VisualElement, ILazyContent, IValidationMessageHost
	{
		public const string UssClassName = "ext-list";
		public const string RowUssClassName = UssClassName + "__row";
		public const string IndexUssClassName = UssClassName + "__index";
		/// <summary>On a row that shows its index. The body of a foldout element then starts under the index.</summary>
		public const string IndexedRowUssClassName = RowUssClassName + "--indexed";
		public const string CappedUssClassName = UssClassName + "--capped";
		/// <summary>On a list without elements.</summary>
		public const string EmptyUssClassName = UssClassName + "--empty";
		public const string OverriddenUssClassName = UssClassName + "--overridden";

		/// <summary>Above this many elements the element area is limited in height and scrolls.</summary>
		public const int ElementCountThatCapsHeight = 50;

		public SerializedObject SerializedObject { get; }
		public string ArrayPropertyPath { get; }
		public ListOptions Options { get; }
		public VisualElement ValidationMessageContainer { get; }

		private readonly InspectorInstance Owner;
		private readonly LazyFoldout Header;
		private readonly Label CountLabel;
		private readonly List<int> Indices = new List<int>();
		private ListView List;
		private ToolbarSearchField SearchField;
		private Label SearchCounter;
		private VisualElement ProbeContainer;
		private int ProbeRowLimit;
		private static readonly List<int> NoIndices = new List<int>();
		private string _SearchText = "";
		private int LastSignature;
		private bool IsTracking;

		private static readonly HashSet<(int, string)> ListsSeenThisSession = new HashSet<(int, string)>();

		/// <summary>
		/// Opens or closes a list that declares a default state, the first time it is shown for its object in an
		/// editor session. Shared by the list and the table form of a list.
		/// </summary>
		internal static void ApplyDefaultExpandedState(SerializedProperty arrayProperty, ListOptions options)
		{
			if (options.CollapsedByDefault && options.ExpandedByDefault)
				throw new ArgumentException($"The list '{arrayProperty.propertyPath}' is declared both collapsed and expanded by default.", nameof(options));
			if ((options.CollapsedByDefault || options.ExpandedByDefault) && ListsSeenThisSession.Add((arrayProperty.serializedObject.targetObject.GetInstanceID(), arrayProperty.propertyPath)))
				arrayProperty.isExpanded = options.ExpandedByDefault;
		}

		/// <param name="label">Null for the property's own name.</param>
		/// <param name="owner">The inspector the list belongs to. Needed by the options that read the owning value: a custom add function and element dropdowns.</param>
		public ExtListView(SerializedProperty arrayProperty, string label = null, ListOptions options = null, InspectorInstance owner = null)
		{
			if (arrayProperty == null)
				throw new ArgumentNullException(nameof(arrayProperty));
			if (!arrayProperty.isArray || arrayProperty.propertyType == SerializedPropertyType.String)
				throw new ArgumentException($"'{arrayProperty.propertyPath}' is not an array or a list.", nameof(arrayProperty));

			SerializedObject = arrayProperty.serializedObject;
			ArrayPropertyPath = arrayProperty.propertyPath;
			Options = options ?? new ListOptions(typeof(object));
			Owner = owner;
			if (Owner == null && (Options.CustomAdd != null || Options.ElementDropdownItems != null))
				throw new ArgumentException("A custom add function and element dropdowns read the owning value, so the list needs the inspector it belongs to.", nameof(owner));

			AddToClassList(UssClassName);
			InspectorStyles.Apply(this);

			ApplyDefaultExpandedState(arrayProperty, Options);

			CountLabel = new Label { pickingMode = PickingMode.Ignore };
			CountLabel.AddToClassList(UssClassName + "__count");
			Header = new LazyFoldout(label ?? arrayProperty.displayName, BuildContent, arrayProperty.isExpanded)
			{
				bindingPath = ArrayPropertyPath,
			};
			Header.FollowExpandedStateOf(arrayProperty);
			Header.AddToClassList(UssClassName + "__header");
			Add(Header);

			var toggle = Header.Q<Toggle>(className: Foldout.toggleUssClassName)
			             ?? throw new InvalidOperationException("The foldout has no toggle to put the element count on.");
			toggle.Add(CountLabel);
			toggle.AddManipulator(new ContextualMenuManipulator(BuildHeaderMenu));

			// Between the header and the content, so the messages show while the list is closed too.
			ValidationMessageContainer = new VisualElement();
			ValidationMessageContainer.AddToClassList(UssClassName + "__messages");
			Header.hierarchy.Insert(1, ValidationMessageContainer);

			LastSignature = ComputeSignature(arrayProperty);
			UpdateHeader(arrayProperty);
			RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
		}

		#region Content

		private void BuildContent(VisualElement content)
		{
			if (Options.Searchable)
			{
				var searchRow = new VisualElement();
				searchRow.AddToClassList(UssClassName + "__search");
				SearchField = new ToolbarSearchField();
				SearchField.RegisterValueChangedCallback(evt => SearchText = evt.newValue);
				searchRow.Add(SearchField);
				SearchCounter = new Label();
				SearchCounter.AddToClassList(UssClassName + "__search-counter");
				searchRow.Add(SearchCounter);
				content.Add(searchRow);
			}

			List = new ListView
			{
				itemsSource = Indices,
				makeItem = MakeRow,
				bindItem = BindRow,
				unbindItem = UnbindRow,
				showFoldoutHeader = false,
				showBoundCollectionSize = false,
				showAddRemoveFooter = true,
				showBorder = true,
				reorderable = true,
				reorderMode = ListViewReorderMode.Animated,
				virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
				showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
				selectionType = SelectionType.Single,
			};
			List.AddToClassList(UssClassName + "__view");
			List.onRemove = _ => RemoveSelectedOrLastElement();
			if (Options.ManagedReferences)
				List.overridingAddButtonBehavior = (_, button) => ShowTypePicker(button.worldBound, false, AddManagedReference);
			else
				List.onAdd = _ => AddElement();
			List.itemIndexChanged += OnItemIndexChanged;
			content.Add(List);

			Refresh();
		}

		private SerializedProperty FindArray()
		{
			return SerializedObject.FindProperty(ArrayPropertyPath);
		}

		private SerializedProperty RequireArray()
		{
			return FindArray()
			       ?? throw new InvalidOperationException($"The list property '{ArrayPropertyPath}' no longer exists on '{SerializedObject.targetObject}'.");
		}

		/// <summary>Reads the list again: which elements are shown, the rows, the count.</summary>
		public void Refresh()
		{
			var array = FindArray();
			if (array == null)
				return; // The list itself was removed (an element of an outer list). The outer list rebuilds.

			LastSignature = ComputeSignature(array);
			UpdateHeader(array);
			if (List == null)
				return;

			Indices.Clear();
			var size = array.arraySize;
			for (var i = 0; i < size; i++)
			{
				if (!IsFiltering || MatchesSearch(array.GetArrayElementAtIndex(i), i))
					Indices.Add(i);
			}

			List.reorderable = !IsFiltering;
			List.allowAdd = !IsFiltering;
			List.allowRemove = !IsFiltering;
			if (Options.MaxVisibleHeight > 0f)
				List.style.maxHeight = Options.MaxVisibleHeight;
			else
				List.EnableInClassList(CappedUssClassName, Indices.Count > ElementCountThatCapsHeight);
			// An empty list is its header ("0 items") and the add button: no box that says it is empty.
			EnableInClassList(EmptyUssClassName, size == 0);
			if (SearchCounter != null)
				SearchCounter.text = IsFiltering ? $"{Indices.Count} / {size}" : "";
			List.ClearSelection();
			if (ProbeContainer != null)
				BuildProbeRows(array);
			else
				List.Rebuild();
		}

		private void UpdateHeader(SerializedProperty array)
		{
			var size = array.arraySize;
			CountLabel.text = size == 1 ? "1 item" : size + " items";
			EnableInClassList(OverriddenUssClassName, array.prefabOverride);
		}

		private void BuildHeaderMenu(ContextualMenuPopulateEvent evt)
		{
			var array = FindArray();
			if (array == null)
				return;
			evt.menu.AppendAction("Copy Property Path", _ => EditorGUIUtility.systemCopyBuffer = ArrayPropertyPath);
			if (array.prefabOverride)
			{
				evt.menu.AppendSeparator();
				evt.menu.AppendAction("Revert", _ =>
				{
					var property = RequireArray();
					PrefabUtility.RevertPropertyOverride(property, InteractionMode.UserAction);
					SerializedObject.Update();
					Refresh();
				});
			}
			evt.menu.AppendSeparator();
			evt.menu.AppendAction("Clear List", _ =>
			{
				var property = RequireArray();
				property.ClearArray();
				SerializedObject.ApplyModifiedProperties();
				Refresh();
			}, array.arraySize > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
		}

		#endregion

		#region Rows

		private sealed class Row : VisualElement
		{
			public readonly Label Index;
			public readonly VisualElement FieldHost;
			public readonly Button TypeButton;
			public int ArrayIndex = -1;

			public Row(bool showIndex, bool hasTypeButton)
			{
				AddToClassList(RowUssClassName);
				EnableInClassList(IndexedRowUssClassName, showIndex);
				Index = new Label();
				Index.AddToClassList(IndexUssClassName);
				Index.style.display = showIndex ? DisplayStyle.Flex : DisplayStyle.None;
				Add(Index);
				FieldHost = new VisualElement();
				FieldHost.AddToClassList(UssClassName + "__field");
				Add(FieldHost);
				if (hasTypeButton)
				{
					TypeButton = new Button();
					TypeButton.AddToClassList(UssClassName + "__type");
					var icon = new Image { image = EditorGUIUtility.IconContent("_Menu").image, pickingMode = PickingMode.Ignore };
					icon.AddToClassList(UssClassName + "__type-icon");
					TypeButton.Add(icon);
					Add(TypeButton);
					AddToClassList(RowUssClassName + "--typed");
				}
			}
		}

		private VisualElement MakeRow()
		{
			var row = new Row(Options.ShowIndex, Options.ManagedReferences);
			if (row.TypeButton != null)
				row.TypeButton.clicked += () => ShowTypePicker(row.TypeButton.worldBound, true, type => ChangeManagedReferenceType(row.ArrayIndex, type));
			return row;
		}

		private void BindRow(VisualElement element, int rowIndex)
		{
			BindRowToArrayIndex((Row)element, Indices[rowIndex]);
		}

		private void BindRowToArrayIndex(Row row, int arrayIndex)
		{
			var elementProperty = RequireArray().GetArrayElementAtIndex(arrayIndex);
			row.ArrayIndex = arrayIndex;
			row.Index.text = arrayIndex.ToString();
			row.FieldHost.Clear();
			var field = CreateElementField(elementProperty);
			row.FieldHost.Add(field);
			if (row.TypeButton != null)
				row.TypeButton.tooltip = $"Change the type of this element (now: {DescribeManagedReferenceType(elementProperty)}). Fields with the same name keep their values.";
			var hasFoldout = (elementProperty.propertyType == SerializedPropertyType.Generic && !elementProperty.isArray)
			                 || (elementProperty.propertyType == SerializedPropertyType.ManagedReference && elementProperty.managedReferenceValue != null);
			row.EnableInClassList(RowUssClassName + "--foldout", hasFoldout && Options.ElementDropdownItems == null && Options.ElementFactory == null && !IsElementDrawnByAnotherDrawer);
			if (row.panel != null)
			{
				// A list without an owner (default) selects nothing and binds as before.
				using (Owner?.SelectHandlerCache() ?? default)
					field.Bind(SerializedObject);
			}
		}

		private void UnbindRow(VisualElement element, int rowIndex)
		{
			var row = (Row)element;
			row.ArrayIndex = -1;
			row.FieldHost.Clear();
		}

		private VisualElement CreateElementField(SerializedProperty elementProperty)
		{
			if (Options.ElementDropdownItems != null)
			{
				return DropdownFieldFactory.Create(elementProperty, "", () =>
				{
					var pass = Owner.CreatePass();
					return pass != null && pass.Targets.Length > 0
						? Options.ElementDropdownItems(pass.Targets[0], pass.Context)
						: Array.Empty<DropdownItem<object>>();
				}, Options.ElementDropdownOptions);
			}

			if (Options.ElementFactory != null)
			{
				return Options.ElementFactory(elementProperty, GetRowLabel(elementProperty))
				       ?? throw new InvalidOperationException($"The element factory of the list '{ArrayPropertyPath}' built no element.");
			}

			if (elementProperty.propertyType == SerializedPropertyType.ManagedReference && elementProperty.managedReferenceValue == null)
			{
				var none = new Label("None");
				none.AddToClassList(UssClassName + "__none");
				return none;
			}

			var field = new PropertyField(elementProperty, GetRowLabel(elementProperty));
			if (Options.ElementLabel != null)
				field.AddToClassList(InspectorStyles.ExplicitLabelClass);
			return field;
		}

		/// <summary>The label a row asks its element to show. Null: the element decides (its own title, or Unity's default).</summary>
		private string GetRowLabel(SerializedProperty elementProperty)
		{
			if (Options.ElementLabel != null)
			{
				var boxed = Box(elementProperty);
				var label = boxed != null ? Options.ElementLabel(boxed) : null;
				return string.IsNullOrEmpty(label) ? "(no label)" : label;
			}
			if (elementProperty.propertyType == SerializedPropertyType.ManagedReference)
				return DescribeManagedReferenceType(elementProperty);
			// A plain value needs no "Element 3" next to its index. Neither does a value that a drawer of its own
			// draws as one field (a vector, a struct drawn as its single value, an asset reference).
			if (Options.ShowIndex && (elementProperty.propertyType != SerializedPropertyType.Generic || elementProperty.isArray || IsElementDrawnByAnotherDrawer || Options.ElementFactory != null))
				return "";
			return null;
		}

		private bool? _IsElementDrawnByAnotherDrawer;

		/// <summary>True when the elements have a property drawer that is not a nested inspector of the toolbox.</summary>
		private bool IsElementDrawnByAnotherDrawer
		{
			get
			{
				_IsElementDrawnByAnotherDrawer ??= PropertyDrawerLookup.TryGetDrawerType(Options.ElementType, out var drawerType)
				                                   && !typeof(ICustomizedInspector).IsAssignableFrom(drawerType);
				return _IsElementDrawnByAnotherDrawer.Value;
			}
		}

		/// <summary>The label of an element as the list shows or searches it.</summary>
		public string GetElementLabel(int arrayIndex)
		{
			var array = RequireArray();
			if (arrayIndex < 0 || arrayIndex >= array.arraySize)
				throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			return GetSearchLabel(array.GetArrayElementAtIndex(arrayIndex), arrayIndex);
		}

		private string GetSearchLabel(SerializedProperty elementProperty, int arrayIndex)
		{
			if (Options.ElementLabel != null)
				return GetRowLabel(elementProperty);

			switch (elementProperty.propertyType)
			{
				case SerializedPropertyType.String:
					return elementProperty.stringValue;
				case SerializedPropertyType.ObjectReference:
					return elementProperty.objectReferenceValue != null ? elementProperty.objectReferenceValue.name : "";
				case SerializedPropertyType.Generic:
				case SerializedPropertyType.ManagedReference:
				{
					var boxed = Box(elementProperty);
					if (boxed == null)
						return "None";
					if (InspectorBlueprints.TryGetForValueType(boxed.GetType(), out var blueprint) && blueprint.Title != null)
					{
						var title = blueprint.Title(boxed);
						if (!string.IsNullOrEmpty(title))
							return title;
					}
					if (elementProperty.propertyType == SerializedPropertyType.ManagedReference)
						return ObjectNames.NicifyVariableName(boxed.GetType().Name);
					// Unity's own convention: the first string of the element names it.
					var end = elementProperty.GetEndProperty();
					var child = elementProperty.Copy();
					if (child.NextVisible(true) && !SerializedProperty.EqualContents(child, end) && child.propertyType == SerializedPropertyType.String)
						return child.stringValue;
					return "Element " + arrayIndex;
				}
				default:
					return "Element " + arrayIndex;
			}
		}

		private void UpdateRowLabels()
		{
			if (List == null || Options.ElementLabel == null)
				return;
			var array = FindArray();
			if (array == null)
				return;
			// The rows of the list view, or the rows built aside when content was forced.
			var rowParent = ProbeContainer ?? (VisualElement)List;
			rowParent.Query<VisualElement>(className: RowUssClassName).ForEach(element =>
			{
				var row = (Row)element;
				// The query also finds the rows of lists inside the elements. Those belong to their own list.
				if (row.GetFirstAncestorOfType<ExtListView>() != this)
					return;
				if (row.ArrayIndex < 0 || row.ArrayIndex >= array.arraySize || row.FieldHost.childCount == 0)
					return;
				if (!(row.FieldHost[0] is PropertyField field))
					return;
				var label = GetRowLabel(array.GetArrayElementAtIndex(row.ArrayIndex));
				if (field.label == label)
					return;
				FieldLabelTools.SetLabel(field, label);
				// An element drawn by a nested inspector takes its title from the label of its row.
				field.Q<InspectorRootElement>()?.Instance.Reevaluate();
			});
		}

		private static object Box(SerializedProperty property)
		{
			return property.propertyType == SerializedPropertyType.ManagedReference
				? property.managedReferenceValue
				: property.boxedValue;
		}

		#endregion

		#region Search

		public bool IsFiltering => !string.IsNullOrWhiteSpace(_SearchText);

		/// <summary>The search text. Setting it filters the rows. While filtering, reordering, adding and removing are off.</summary>
		public string SearchText
		{
			get => _SearchText;
			set
			{
				value ??= "";
				if (_SearchText == value)
					return;
				_SearchText = value;
				SearchField?.SetValueWithoutNotify(value);
				Refresh();
			}
		}

		/// <summary>The array indices of the elements that are shown, in order.</summary>
		public IReadOnlyList<int> VisibleIndices => Indices;

		private bool MatchesSearch(SerializedProperty elementProperty, int arrayIndex)
		{
			var text = _SearchText.Trim();
			if (Options.SearchMatch != null)
			{
				var boxed = Box(elementProperty);
				return boxed != null && Options.SearchMatch(boxed, text);
			}
			var label = GetSearchLabel(elementProperty, arrayIndex);
			return label != null && label.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		#endregion

		#region Change the list

		/// <summary>Adds an element at the end: the custom add function's result when there is one, otherwise a copy of the last element as Unity does.</summary>
		public void AddElement()
		{
			if (Options.ManagedReferences)
				throw new InvalidOperationException($"'{ArrayPropertyPath}' is a managed reference list. Add an element with {nameof(AddManagedReference)} and a type.");

			object created = null;
			if (Options.CustomAdd != null)
			{
				var pass = Owner.CreatePass();
				if (pass == null || pass.Targets.Length == 0)
					throw new InvalidOperationException($"The value that owns the list '{ArrayPropertyPath}' no longer exists.");
				created = Options.CustomAdd(pass.Targets[0]);
			}

			SerializedObject.Update();
			var array = RequireArray();
			var index = array.arraySize;
			array.arraySize = index + 1;
			if (Options.CustomAdd != null)
			{
				var element = array.GetArrayElementAtIndex(index);
				if (element.propertyType == SerializedPropertyType.ManagedReference)
					element.managedReferenceValue = created;
				else
					element.boxedValue = created;
			}
			SerializedObject.ApplyModifiedProperties();
			Refresh();
		}

		public void RemoveElement(int arrayIndex)
		{
			SerializedObject.Update();
			var array = RequireArray();
			if (arrayIndex < 0 || arrayIndex >= array.arraySize)
				throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			array.DeleteArrayElementAtIndex(arrayIndex);
			SerializedObject.ApplyModifiedProperties();
			Refresh();
		}

		public void MoveElement(int fromArrayIndex, int toArrayIndex)
		{
			SerializedObject.Update();
			var array = RequireArray();
			if (fromArrayIndex < 0 || fromArrayIndex >= array.arraySize)
				throw new ArgumentOutOfRangeException(nameof(fromArrayIndex));
			if (toArrayIndex < 0 || toArrayIndex >= array.arraySize)
				throw new ArgumentOutOfRangeException(nameof(toArrayIndex));
			array.MoveArrayElement(fromArrayIndex, toArrayIndex);
			SerializedObject.ApplyModifiedProperties();
			Refresh();
		}

		private void RemoveSelectedOrLastElement()
		{
			var array = RequireArray();
			if (array.arraySize == 0)
				return;
			var selectedRow = List.selectedIndex;
			RemoveElement(selectedRow >= 0 && selectedRow < Indices.Count ? Indices[selectedRow] : array.arraySize - 1);
		}

		private void OnItemIndexChanged(int fromRow, int toRow)
		{
			// The list view has already moved the entry inside Indices. Rows equal array indices here, because
			// reordering is off while filtering.
			MoveElement(fromRow, toRow);
		}

		#endregion

		#region Managed references

		/// <summary>The concrete types an element of a managed reference list can have, with their menu paths, sorted by path.</summary>
		public List<(Type Type, string MenuPath)> GetManagedReferenceTypes()
		{
			var types = new List<(Type, string)>();
			void Consider(Type type)
			{
				if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition || typeof(UnityEngine.Object).IsAssignableFrom(type))
					return;
				if (!type.IsSerializable)
					return;
				var path = Options.ManagedReferenceMenuPath?.Invoke(type);
				types.Add((type, string.IsNullOrEmpty(path) ? ObjectNames.NicifyVariableName(type.Name) : path));
			}
			Consider(Options.ElementType);
			foreach (var type in TypeCache.GetTypesDerivedFrom(Options.ElementType))
				Consider(type);
			types.Sort((a, b) => string.Compare(a.Item2, b.Item2, StringComparison.OrdinalIgnoreCase));
			return types;
		}

		private void ShowTypePicker(Rect anchor, bool offerNone, Action<Type> onPick)
		{
			var items = new List<DropdownItem<object>>();
			if (offerNone)
				items.Add(new DropdownItem<object>("None", null));
			foreach (var (type, menuPath) in GetManagedReferenceTypes())
			{
				var separator = menuPath.LastIndexOf('/');
				items.Add(separator < 0
					? new DropdownItem<object>(menuPath, type)
					: new DropdownItem<object>(menuPath.Substring(separator + 1), type, menuPath.Substring(0, separator)));
			}
			SearchablePicker.Show(anchor, items, picked => onPick((Type)picked), ObjectNames.NicifyVariableName(Options.ElementType.Name));
		}

		/// <summary>Adds a new instance of the type at the end of a managed reference list.</summary>
		public void AddManagedReference(Type type)
		{
			var instance = CreateManagedReference(type);
			SerializedObject.Update();
			var array = RequireArray();
			var index = array.arraySize;
			array.arraySize = index + 1;
			array.GetArrayElementAtIndex(index).managedReferenceValue = instance;
			SerializedObject.ApplyModifiedProperties();
			Refresh();
		}

		/// <summary>
		/// Replaces an element with a new instance of another type. Fields that exist under the same name in both
		/// types keep their values. A null type clears the element.
		/// </summary>
		public void ChangeManagedReferenceType(int arrayIndex, Type type)
		{
			SerializedObject.Update();
			var array = RequireArray();
			if (arrayIndex < 0 || arrayIndex >= array.arraySize)
				throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			var element = array.GetArrayElementAtIndex(arrayIndex);
			var old = element.managedReferenceValue;
			if (type == (old?.GetType()))
				return;

			object replacement = null;
			if (type != null)
			{
				replacement = CreateManagedReference(type);
				if (old != null)
					JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(old), replacement);
			}
			element.managedReferenceValue = replacement;
			SerializedObject.ApplyModifiedProperties();
			Refresh();
		}

		private object CreateManagedReference(Type type)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (!Options.ElementType.IsAssignableFrom(type))
				throw new ArgumentException($"'{type.FullName}' is not a '{Options.ElementType.FullName}', so it cannot be an element of '{ArrayPropertyPath}'.");
			try
			{
				return Activator.CreateInstance(type, true);
			}
			catch (MissingMethodException exception)
			{
				throw new InvalidOperationException($"'{type.FullName}' needs a constructor without parameters to be created from the inspector.", exception);
			}
		}

		private static string DescribeManagedReferenceType(SerializedProperty elementProperty)
		{
			if (elementProperty.propertyType != SerializedPropertyType.ManagedReference)
				return "";
			var value = elementProperty.managedReferenceValue;
			return value != null ? ObjectNames.NicifyVariableName(value.GetType().Name) : "None";
		}

		#endregion

		#region Tracking

		private void OnAttachToPanel(AttachToPanelEvent evt)
		{
			if (IsTracking)
				return;
			// A tick later, when the host has bound the tree: see InspectorInstance.
			schedule.Execute(() =>
			{
				if (IsTracking || panel == null)
					return;
				var array = FindArray();
				if (array == null)
					return;
				IsTracking = true;
				this.TrackPropertyValue(array, _ => OnArrayChanged());
				OnArrayChanged();
			});
		}

		private void OnArrayChanged()
		{
			var array = FindArray();
			if (array == null)
				return;
			if (ComputeSignature(array) != LastSignature)
				Refresh();
			else
				UpdateRowLabels();
			UpdateHeader(array);
		}

		/// <summary>Changes when elements are added, removed or, for managed references, change their type.</summary>
		private int ComputeSignature(SerializedProperty array)
		{
			var size = array.arraySize;
			var signature = size;
			if (Options.ManagedReferences)
			{
				for (var i = 0; i < size; i++)
				{
					unchecked
					{
						signature = signature * 31 + array.GetArrayElementAtIndex(i).managedReferenceFullTypename.GetHashCode();
					}
				}
			}
			return signature;
		}

		#endregion

		#region Lazy content

		/// <summary>
		/// From the first call on the list view shows no rows of its own: the first
		/// <paramref name="maxListElements"/> elements that are shown (all of them, or those the search finds)
		/// get a row each, built aside and bound, and are kept up to date when the list changes. Which rows a
		/// list view creates depends on the layout and on timing, and on a panel without a window it grows by
		/// about a row per layout pass, which is more than Unity allows in one update for a list of twenty.
		/// Searching, adding, removing and moving still work on the list.
		/// </summary>
		public bool ForceBuildContent(int maxListElements)
		{
			var built = Header.ForceBuildContent(maxListElements);
			if (ProbeContainer != null && maxListElements <= ProbeRowLimit)
				return built;

			var array = FindArray();
			if (array == null)
				return built;
			if (ProbeContainer == null)
			{
				ProbeContainer = new VisualElement { name = "ext-list-probe-rows" };
				ProbeContainer.style.display = DisplayStyle.None;
				hierarchy.Add(ProbeContainer);
				List.itemsSource = NoIndices;
				List.Rebuild();
				List.style.display = DisplayStyle.None;
			}
			ProbeRowLimit = maxListElements;
			BuildProbeRows(array);
			return true;
		}

		private void BuildProbeRows(SerializedProperty array)
		{
			ProbeContainer.Clear();
			var count = Mathf.Min(ProbeRowLimit, Indices.Count);
			for (var i = 0; i < count; i++)
			{
				if (Indices[i] >= array.arraySize)
					break;
				var row = (Row)MakeRow();
				ProbeContainer.Add(row);
				BindRowToArrayIndex(row, Indices[i]);
			}
		}

		#endregion
	}

	/// <summary>Creates the element for a list field that has list options.</summary>
	public static class ListElementFactory
	{
		public static VisualElement Create(InspectorInstance instance, SerializedProperty property, string label, ListOptions options)
		{
			// With several objects selected their lists can differ in length. Unity's own list knows how to show that.
			if (property.serializedObject.isEditingMultipleObjects)
				return new PropertyField(property, label);
			if (options.Table)
				return new ExtTableView(property, label, options, instance);
			return new ExtListView(property, label, options, instance);
		}
	}

}
