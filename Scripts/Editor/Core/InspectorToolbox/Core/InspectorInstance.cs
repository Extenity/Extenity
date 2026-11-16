using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>The root element of an inspector instance. Finding it in a tree leads to the instance.</summary>
	public sealed class InspectorRootElement : VisualElement
	{
		public const string UssClassName = "ext-instance";

		public InspectorInstance Instance { get; }

		internal InspectorRootElement(InspectorInstance instance)
		{
			Instance = instance;
			AddToClassList(UssClassName);
		}
	}

	/// <summary>
	/// One built inspector: the element tree made from a blueprint for one object or one property, and the rules
	/// that keep it up to date. It never keeps a <see cref="SerializedProperty"/>: it stores the path and looks
	/// the property up again whenever it needs it.
	/// </summary>
	public sealed class InspectorInstance
	{
		public InspectorBlueprint Blueprint { get; }
		public InspectorContext Context { get; }
		/// <summary>The element to put into a parent. Bind an ancestor to the serialized object.</summary>
		public VisualElement Root { get; }
		/// <summary>The label the parent asked for. Null: the property's own name. Empty: no header.</summary>
		public string Label { get; }
		/// <summary>True once the fields are built. A nested inspector with a closed foldout has only its header.</summary>
		public bool IsBodyBuilt { get; private set; }

		private readonly List<ValidationMessage> _Messages = new List<ValidationMessage>();
		/// <summary>The validation messages shown after the last evaluation, own fields only.</summary>
		public IReadOnlyList<ValidationMessage> Messages => _Messages;

		private readonly List<Action<EvaluationPass>> Evaluators = new List<Action<EvaluationPass>>();
		private readonly List<Action<EvaluationPass>> LiveRefreshers = new List<Action<EvaluationPass>>();
		private readonly List<(VisualElement Owner, string FieldName, Action Changed)> FieldTrackers = new List<(VisualElement, string, Action)>();
		private readonly HashSet<string> ReportedRuleFailures = new HashSet<string>(StringComparer.Ordinal);
		private readonly List<(VisualElement Element, string SearchText)> SearchItems = new List<(VisualElement, string)>();

		private readonly bool IsConstructed;
		private LazyFoldout HeaderFoldout;
		private BoxGroup HeaderBox;
		private VisualElement ObjectMessages;
		private PropertyField HostField;
		private bool AreTrackersRegistered;
		private bool IsLiveScheduleStarted;
		private SerializedObject[] PerTargetSerializedObjects;

		internal InspectorInstance(InspectorBlueprint blueprint, SerializedObject serializedObject, string propertyPath, string label)
		{
			Blueprint = blueprint;
			Context = new InspectorContext(serializedObject, propertyPath);
			Label = label;

			var root = new InspectorRootElement(this);
			Root = root;
			InspectorStyles.Apply(root);

			if (Context.PropertyPath == null)
			{
				root.AddToClassList(InspectorRootElement.UssClassName + "--object");
				BuildBody(root);
			}
			else
			{
				var property = Context.Property
				               ?? throw new ArgumentException($"'{serializedObject.targetObject}' has no property at '{propertyPath}'.");
				var isHeaderless = label == "" || blueprint.Header == HeaderMode.Inline;
				if (property.propertyType == SerializedPropertyType.ManagedReference && property.managedReferenceValue == null)
				{
					// Nothing to draw. Unity builds the drawer again when the reference gets a value.
					var none = new LiveValueField(label ?? property.displayName);
					none.ShowPlaceholder("None");
					root.Add(none);
					IsBodyBuilt = true;
				}
				else if (isHeaderless)
				{
					BuildBody(root);
				}
				else if (blueprint.Header == HeaderMode.Box)
				{
					HeaderBox = new BoxGroup(label ?? property.displayName);
					root.Add(HeaderBox);
					BuildBody(HeaderBox.contentContainer);
				}
				else
				{
					HeaderFoldout = new LazyFoldout(label ?? property.displayName, BuildBody, property.isExpanded)
					{
						bindingPath = property.propertyPath,
					};
					HeaderFoldout.FollowExpandedStateOf(property);
					HeaderFoldout.AddToClassList("ext-nested");
					root.Add(HeaderFoldout);
				}
			}

			root.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
			root.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
			IsConstructed = true;
			Reevaluate();
		}

		#region Build

		private void BuildBody(VisualElement container)
		{
			if (IsBodyBuilt)
				throw new InvalidOperationException("The body of the inspector is built already.");
			IsBodyBuilt = true;

			var childNames = GetChildNames();
			foreach (var rule in Blueprint.Rules)
			{
				if (!childNames.Contains(rule.FieldName))
				{
					throw new InvalidOperationException(
						$"The inspector '{Blueprint.InspectorType.Name}' has a rule for field '{rule.FieldName}', which is not a visible serialized property of "
						+ $"'{Context.PropertyPath ?? Context.RootObject.GetType().Name}'. A field with HideInInspector has no property to customize: use Hide() in the inspector instead.");
				}
			}

			ObjectMessages = new VisualElement { name = "ext-object-messages" };
			container.Add(ObjectMessages);
			if (Blueprint.ObjectValidators.Count > 0)
				Evaluators.Add(EvaluateObjectValidators);

			// The search field of the sections sits under the Script row, above the first thing it filters.
			var sectionSearch = Blueprint.Sections && Blueprint.SectionsSearchable ? CreateSectionSearchField() : null;
			foreach (var entry in Blueprint.PlanLayout(childNames))
			{
				var isScriptField = entry.IsField && entry.FieldName == InspectorBlueprint.ScriptFieldName;
				if (isScriptField && Blueprint.HideScriptField)
					continue;
				if (sectionSearch != null && !isScriptField)
				{
					container.Add(sectionSearch);
					sectionSearch = null;
				}
				var element = entry.IsField ? BuildField(entry.FieldName) : entry.Node.Build(this);
				if (element == null)
					throw new InvalidOperationException($"The node '{entry.Node?.GetType().Name}' built no element.");
				container.Add(element);
				if (Blueprint.Sections && entry.IsField && !isScriptField)
					SearchItems.Add((element, GetSearchText(entry.FieldName)));
			}
			if (sectionSearch != null)
				container.Add(sectionSearch);

			if (IsConstructed)
			{
				// Built after construction (a foldout opened): nobody else binds or evaluates this content.
				BindLateContent(container);
				Reevaluate();
			}
		}

		private List<string> GetChildNames()
		{
			var names = new List<string>();
			if (Context.PropertyPath == null)
			{
				var iterator = Context.SerializedObject.GetIterator();
				if (iterator.NextVisible(true))
				{
					do
					{
						names.Add(iterator.name);
					}
					while (iterator.NextVisible(false));
				}
			}
			else
			{
				var property = RequireProperty();
				var end = property.GetEndProperty();
				var iterator = property.Copy();
				if (iterator.NextVisible(true))
				{
					while (!SerializedProperty.EqualContents(iterator, end))
					{
						names.Add(iterator.name);
						if (!iterator.NextVisible(false))
							break;
					}
				}
			}
			return names;
		}

		private SerializedProperty RequireProperty()
		{
			return Context.Property
			       ?? throw new InvalidOperationException($"The property '{Context.PropertyPath}' no longer exists on '{Context.RootObject}'.");
		}

		/// <summary>The property of a field of the inspected value, by its serialized name. Throws when there is none.</summary>
		public SerializedProperty FindField(string fieldName)
		{
			var property = Context.PropertyPath == null
				? Context.SerializedObject.FindProperty(fieldName)
				: RequireProperty().FindPropertyRelative(fieldName);
			return property
			       ?? throw new InvalidOperationException($"'{Context.PropertyPath ?? Context.RootObject.GetType().Name}' has no serialized property named '{fieldName}'.");
		}

		/// <summary>
		/// Builds the element of a field with everything its rule declares. Containers call this for the fields
		/// they claimed.
		/// </summary>
		public VisualElement BuildField(string fieldName)
		{
			var property = FindField(fieldName);
			Blueprint.RulesByName.TryGetValue(fieldName, out var rule);

			if (rule == null)
			{
				if (IsSectionField(property))
				{
					var section = CreateSection(property, null);
					FieldNames.TryFind(Context.RootObject.GetType(), fieldName, out var sectionField);
					var decorators = FieldDecorators.Create(sectionField);
					if (decorators == null)
						return section;
					var decorated = new VisualElement { name = "ext-decorated-" + fieldName };
					decorated.Add(decorators);
					decorated.Add(section);
					return decorated;
				}
				var plain = new PropertyField(property);
				if (Context.PropertyPath == null && fieldName == InspectorBlueprint.ScriptFieldName)
					plain.SetEnabled(false);
				return plain;
			}

			var fieldElement = CreateFieldElement(rule, property);
			var slot = new FieldSlot(this, rule, property, fieldElement);
			if (!(fieldElement is PropertyField))
			{
				// A property field draws the Header and Space attributes of its field and shows its tooltip.
				// An element that replaces it must not lose them.
				var decorators = FieldDecorators.Create(rule.Field);
				if (decorators != null)
					slot.Element.Insert(0, decorators);
				FieldDecorators.ApplyTooltip(fieldElement, property.tooltip);
			}
			Evaluators.Add(slot.Evaluate);
			if (rule.ChangeHandlers.Count > 0)
				TrackField(slot.Element, fieldName, () => Mutate(null, false, target =>
				{
					foreach (var handler in rule.ChangeHandlers)
						handler(target);
					return target;
				}));
			return slot.Element;
		}

		private VisualElement CreateFieldElement(FieldRuleData rule, SerializedProperty property)
		{
			var label = rule.HideLabel ? "" : rule.Label;
			if (rule.Factory != null)
			{
				return rule.Factory(this, property, label)
				       ?? throw new InvalidOperationException($"'{rule.FactoryFeature}' built no element for field '{property.propertyPath}'.");
			}
			if (rule.List != null && rule.List.ReplacesDefaultList)
			{
				if (!property.isArray)
					throw new InvalidOperationException($"Field '{property.propertyPath}' has list options but is not an array or a list.");
				return ListElementFactory.Create(this, property, label, rule.List);
			}
			if (rule.Inline)
				return InlineFieldFactory.Create(property, rule.Field.FieldType);
			if (IsSectionField(property))
				return CreateSection(property, rule.Label);
			return new PropertyField(property, label);
		}

		/// <summary>
		/// Binds content that was added after the inspector was bound. Does nothing while the content is not in
		/// a panel yet: whoever attaches and binds the inspector binds it then.
		/// </summary>
		public void BindLateContent(VisualElement content)
		{
			if (content.panel == null)
				return;
			content.Bind(Context.SerializedObject);
			LongLabelFitter.Request(content);
		}

		#endregion

		#region Sections

		private bool IsSectionField(SerializedProperty property)
		{
			if (!Blueprint.Sections || property.propertyType != SerializedPropertyType.Generic || property.isArray)
				return false;
			// A value with a drawer of its own (a vector, a struct drawn as one value) is a field, not a section.
			// A nested inspector is different: it declares the content of the section.
			if (FieldNames.TryFind(Context.RootObject.GetType(), property.name, out var field)
			    && PropertyDrawerLookup.TryGetDrawerType(field.FieldType, out var drawerType)
			    && !typeof(ICustomizedInspector).IsAssignableFrom(drawerType))
				return false;
			return true;
		}

		private VisualElement CreateSection(SerializedProperty property, string title)
		{
			var propertyPath = property.propertyPath;
			FieldNames.TryFind(Context.RootObject.GetType(), property.name, out var field);
			var valueType = field?.FieldType;
			var section = new SectionFoldout(title ?? property.displayName, content =>
			{
				var sectionProperty = Context.SerializedObject.FindProperty(propertyPath)
				                      ?? throw new InvalidOperationException($"The property '{propertyPath}' no longer exists.");
				content.Add(InlineFieldFactory.Create(sectionProperty, valueType));
				BindLateContent(content);
			}, property.isExpanded)
			{
				bindingPath = propertyPath,
			};
			section.FollowExpandedStateOf(property);
			return section;
		}

		private VisualElement CreateSectionSearchField()
		{
			var search = new ToolbarSearchField { name = "ext-section-search" };
			search.AddToClassList("ext-section-search");
			search.RegisterValueChangedCallback(evt => ApplySectionSearch(evt.newValue));
			return search;
		}

		private string GetSearchText(string fieldName)
		{
			var property = FindField(fieldName);
			var text = property.displayName + "\n" + property.name;
			if (property.propertyType == SerializedPropertyType.Generic && !property.isArray)
			{
				var end = property.GetEndProperty();
				var iterator = property.Copy();
				if (iterator.NextVisible(true))
				{
					while (!SerializedProperty.EqualContents(iterator, end))
					{
						text += "\n" + iterator.displayName;
						if (!iterator.NextVisible(false))
							break;
					}
				}
			}
			return text;
		}

		private void ApplySectionSearch(string searchText)
		{
			var isFiltering = !string.IsNullOrWhiteSpace(searchText);
			var trimmed = isFiltering ? searchText.Trim() : null;
			foreach (var (element, text) in SearchItems)
			{
				var isMatch = !isFiltering || text.IndexOf(trimmed, StringComparison.OrdinalIgnoreCase) >= 0;
				element.EnableInClassList("ext-search-hidden", !isMatch);
			}
		}

		#endregion

		#region Evaluation

		/// <summary>Registers something to run on every evaluation: after a tracked change and on <see cref="Reevaluate"/>.</summary>
		public void OnEvaluate(Action<EvaluationPass> evaluator)
		{
			Evaluators.Add(evaluator ?? throw new ArgumentNullException(nameof(evaluator)));
		}

		/// <summary>Registers something to run at the live value interval (100 ms) while the inspector is shown.</summary>
		public void OnRefreshLive(Action<EvaluationPass> refresher)
		{
			LiveRefreshers.Add(refresher ?? throw new ArgumentNullException(nameof(refresher)));
			StartLiveScheduleIfNeeded();
		}

		/// <summary>
		/// Runs <paramref name="changed"/> after the value of a field changed. <paramref name="owner"/> is the
		/// element the tracking lives on: it stops when that element leaves the tree for good.
		/// </summary>
		public void TrackField(VisualElement owner, string fieldName, Action changed)
		{
			if (owner == null)
				throw new ArgumentNullException(nameof(owner));
			if (changed == null)
				throw new ArgumentNullException(nameof(changed));
			if (AreTrackersRegistered)
				owner.TrackPropertyValue(FindField(fieldName), _ => changed());
			else
				FieldTrackers.Add((owner, fieldName, changed));
		}

		/// <summary>
		/// The targets of an evaluation, or null when there is nothing to evaluate: the object was destroyed or
		/// the property no longer exists (a removed list element whose row is not rebuilt yet).
		/// </summary>
		public EvaluationPass CreatePass()
		{
			var serializedObject = Context.SerializedObject;
			if (serializedObject.targetObject == null)
				return null;
			if (!serializedObject.hasModifiedProperties)
				serializedObject.UpdateIfRequiredOrScript();

			object[] targets;
			if (Context.PropertyPath == null)
			{
				var objects = serializedObject.targetObjects;
				var alive = new List<object>(objects.Length);
				foreach (var unityObject in objects)
				{
					if (unityObject != null)
						alive.Add(unityObject);
				}
				targets = alive.ToArray();
			}
			else if (!serializedObject.isEditingMultipleObjects)
			{
				var property = Context.Property;
				if (property == null)
					return null;
				var boxed = Box(property);
				targets = boxed != null ? new[] { boxed } : Array.Empty<object>();
			}
			else
			{
				var boxedTargets = new List<object>();
				foreach (var perTarget in GetPerTargetSerializedObjects())
				{
					if (perTarget.targetObject == null)
						continue;
					perTarget.UpdateIfRequiredOrScript();
					var property = perTarget.FindProperty(Context.PropertyPath);
					var boxed = property != null ? Box(property) : null;
					if (boxed != null)
						boxedTargets.Add(boxed);
				}
				targets = boxedTargets.ToArray();
			}

			foreach (var target in targets)
			{
				if (!Blueprint.TargetType.IsInstanceOfType(target))
				{
					throw new InvalidOperationException(
						$"The inspector '{Blueprint.InspectorType.Name}' is written for '{Blueprint.TargetType.FullName}' but was asked to draw a '{target.GetType().FullName}' "
						+ $"at '{Context.PropertyPath ?? "the object"}'. Check the type its CustomEditor or CustomPropertyDrawer attribute names.");
				}
			}
			return new EvaluationPass(Context, targets);
		}

		private static object Box(SerializedProperty property)
		{
			return property.propertyType == SerializedPropertyType.ManagedReference
				? property.managedReferenceValue
				: property.boxedValue;
		}

		private SerializedObject[] GetPerTargetSerializedObjects()
		{
			if (PerTargetSerializedObjects == null)
			{
				var objects = Context.SerializedObject.targetObjects;
				PerTargetSerializedObjects = new SerializedObject[objects.Length];
				for (var i = 0; i < objects.Length; i++)
					PerTargetSerializedObjects[i] = new SerializedObject(objects[i]);
			}
			return PerTargetSerializedObjects;
		}

		/// <summary>
		/// Evaluates every rule again: visibility, enabled state, validation, titles and labels. Runs by itself
		/// after a tracked change. Call it after changing data in a way the inspector cannot notice.
		/// </summary>
		public void Reevaluate()
		{
			var pass = CreatePass();
			if (pass == null)
				return;

			_Messages.Clear();
			RunGuarded("the header title", () => UpdateHeaderTitle(pass));
			// Evaluating can build content (a group opening), which registers more evaluators: walk by index.
			for (var i = 0; i < Evaluators.Count; i++)
			{
				var evaluator = Evaluators[i];
				RunGuarded("a rule", () => evaluator(pass));
			}
		}

		/// <summary>Reads every live value again. Runs by itself every 100 ms while the inspector is shown.</summary>
		public void RefreshLiveValues()
		{
			if (LiveRefreshers.Count == 0)
				return;
			var pass = CreatePass();
			if (pass == null)
				return;
			for (var i = 0; i < LiveRefreshers.Count; i++)
			{
				var refresher = LiveRefreshers[i];
				RunGuarded("a live value", () => refresher(pass));
			}
		}

		/// <summary>
		/// Runs a part of the evaluation so that its failure neither stops the other parts nor goes unnoticed:
		/// the exception is logged (once per distinct failure) and listed in <see cref="Messages"/>.
		/// </summary>
		private void RunGuarded(string what, Action action)
		{
			try
			{
				action();
			}
			catch (Exception exception)
			{
				var where = $"{Blueprint.InspectorType.Name} at '{Context.PropertyPath ?? Context.RootObject.name}'";
				_Messages.Add(new ValidationMessage(MessageSeverity.Error, $"Evaluating {what} failed: {exception.Message}", Context.PropertyPath, isRuleFailure: true));
				if (ReportedRuleFailures.Add(what + exception.GetType().FullName + exception.Message))
				{
					Log.ErrorWithContext(Context.RootObject, $"Evaluating {what} of the inspector {where} failed. The exception follows.");
					Log.ErrorWithContext(Context.RootObject, exception);
				}
			}
		}

		internal void AddMessage(ValidationMessage message)
		{
			_Messages.Add(message);
		}

		private void UpdateHeaderTitle(EvaluationPass pass)
		{
			if (HeaderFoldout == null && HeaderBox == null)
				return;

			string title = null;
			if (HostField == null)
				HostField = Root.GetFirstAncestorOfType<PropertyField>();
			if (HostField != null && HostField.ClassListContains(InspectorStyles.ExplicitLabelClass))
				title = HostField.label;
			// The title names an element of a list. A field that is not in a list keeps its own name.
			var isListElement = Context.PropertyPath.EndsWith("]", StringComparison.Ordinal);
			if (string.IsNullOrEmpty(title) && Blueprint.Title != null && isListElement && pass.HasSingleTarget)
				title = Blueprint.Title(pass.SingleTarget);
			if (string.IsNullOrEmpty(title))
				title = Label ?? Context.Property?.displayName ?? "";

			if (HeaderFoldout != null)
				HeaderFoldout.text = title;
			else
				HeaderBox.Title = title;
		}

		private void EvaluateObjectValidators(EvaluationPass pass)
		{
			var index = 0;
			foreach (var validator in Blueprint.ObjectValidators)
			{
				var result = pass.Worst(validator);
				if (result.IsOk)
					continue;
				MessageBox box;
				if (index < ObjectMessages.childCount)
				{
					box = (MessageBox)ObjectMessages[index];
				}
				else
				{
					box = new MessageBox();
					ObjectMessages.Add(box);
				}
				box.Severity = result.Severity;
				box.Text = result.Message;
				index++;
				_Messages.Add(new ValidationMessage(result.Severity, result.Message, Context.PropertyPath));
			}
			while (ObjectMessages.childCount > index)
				ObjectMessages.RemoveAt(ObjectMessages.childCount - 1);
		}

		#endregion

		#region Change the targets

		/// <summary>
		/// Runs <paramref name="change"/> on every inspected target and returns what it returns to the data.
		/// For an object the live object is passed, recorded for undo and marked dirty. For a nested value a
		/// copy is passed and the returned value is written back through the serialized object (a managed
		/// reference is changed in place). With <paramref name="recordChange"/> false nothing is recorded or
		/// written back: for actions that only read.
		/// </summary>
		public void Mutate(string undoName, bool recordChange, Func<object, object> change)
		{
			if (change == null)
				throw new ArgumentNullException(nameof(change));
			var serializedObject = Context.SerializedObject;
			if (serializedObject.targetObject == null)
				throw new InvalidOperationException("The inspected object no longer exists.");

			if (Context.PropertyPath == null)
			{
				var objects = serializedObject.targetObjects;
				if (recordChange)
					Undo.RecordObjects(objects, undoName ?? "Inspector Change");
				foreach (var unityObject in objects)
				{
					if (unityObject != null)
						change(unityObject);
				}
				// An action may destroy the object it runs on. Then there is nothing left to update or evaluate.
				if (serializedObject.targetObject == null)
					return;
				if (recordChange)
				{
					foreach (var unityObject in objects)
					{
						if (unityObject == null)
							continue;
						EditorUtility.SetDirty(unityObject);
						if (PrefabUtility.IsPartOfPrefabInstance(unityObject))
							PrefabUtility.RecordPrefabInstancePropertyModifications(unityObject);
					}
				}
				serializedObject.Update();
			}
			else if (!serializedObject.isEditingMultipleObjects)
			{
				MutateProperty(serializedObject, undoName, recordChange, change);
			}
			else
			{
				foreach (var perTarget in GetPerTargetSerializedObjects())
				{
					if (perTarget.targetObject != null)
						MutateProperty(perTarget, undoName, recordChange, change);
				}
				serializedObject.Update();
			}
			Reevaluate();
		}

		private void MutateProperty(SerializedObject serializedObject, string undoName, bool recordChange, Func<object, object> change)
		{
			serializedObject.Update();
			var property = serializedObject.FindProperty(Context.PropertyPath)
			               ?? throw new InvalidOperationException($"The property '{Context.PropertyPath}' no longer exists on '{serializedObject.targetObject}'.");

			if (property.propertyType == SerializedPropertyType.ManagedReference)
			{
				var live = property.managedReferenceValue;
				if (live == null)
					return;
				var unityObject = serializedObject.targetObject;
				if (recordChange)
					Undo.RecordObject(unityObject, undoName ?? "Inspector Change");
				var result = change(live);
				if (recordChange)
				{
					if (!ReferenceEquals(result, live))
					{
						property.managedReferenceValue = result;
						serializedObject.ApplyModifiedProperties();
					}
					EditorUtility.SetDirty(unityObject);
					if (PrefabUtility.IsPartOfPrefabInstance(unityObject))
						PrefabUtility.RecordPrefabInstancePropertyModifications(unityObject);
				}
				serializedObject.Update();
			}
			else
			{
				var boxed = property.boxedValue;
				var result = change(boxed);
				if (recordChange)
				{
					property.boxedValue = result;
					serializedObject.ApplyModifiedProperties();
				}
			}
		}

		#endregion

		#region Panel

		private void OnAttachToPanel(AttachToPanelEvent evt)
		{
			// Tracking is registered a tick later: by then the host has bound the tree, and the trackers join the
			// host's binding instead of each starting their own.
			if (!AreTrackersRegistered)
				Root.schedule.Execute(RegisterTrackers);
			StartLiveScheduleIfNeeded();
			if (Blueprint.PollWhilePlayingIntervalMs > 0 && !IsPollScheduled)
			{
				IsPollScheduled = true;
				Root.schedule.Execute(() =>
				{
					if (EditorApplication.isPlaying)
						Reevaluate();
				}).Every(Blueprint.PollWhilePlayingIntervalMs);
			}
			LongLabelFitter.Request(Root);
		}

		private bool IsPollScheduled;

		private void OnDetachFromPanel(DetachFromPanelEvent evt)
		{
			if (PerTargetSerializedObjects != null)
			{
				foreach (var perTarget in PerTargetSerializedObjects)
					perTarget.Dispose();
				PerTargetSerializedObjects = null;
			}
		}

		private void RegisterTrackers()
		{
			if (AreTrackersRegistered || Root.panel == null)
				return;
			var serializedObject = Context.SerializedObject;
			if (serializedObject.targetObject == null)
				return;
			AreTrackersRegistered = true;

			if (Context.PropertyPath == null || Blueprint.TrackWholeObject)
			{
				Root.TrackSerializedObjectValue(serializedObject, _ => Reevaluate());
			}
			else
			{
				var property = Context.Property;
				if (property != null)
					Root.TrackPropertyValue(property, _ => Reevaluate());
			}

			foreach (var (owner, fieldName, changed) in FieldTrackers)
				owner.TrackPropertyValue(FindField(fieldName), _ => changed());
			FieldTrackers.Clear();

			// What changed between building and now was not tracked yet.
			Reevaluate();
		}

		private void StartLiveScheduleIfNeeded()
		{
			if (IsLiveScheduleStarted || LiveRefreshers.Count == 0 || Root.panel == null)
				return;
			IsLiveScheduleStarted = true;
			Root.schedule.Execute(RefreshLiveValues).Every(LiveRefreshIntervalMs);
		}

		public const int LiveRefreshIntervalMs = 100;

		#endregion

		#region Lazy content

		/// <summary>
		/// Builds everything that is normally built on demand: closed foldouts, sections, the rows of lists (up
		/// to <paramref name="maxListElements"/> per list). Nested inspectors appear only after the new content
		/// is bound, which needs a panel, so a caller that wants everything repeats the call until it returns
		/// false. Returns true when something was built.
		/// </summary>
		public bool ExpandAllLazyContent(int maxListElements)
		{
			var builtAnything = false;
			for (var round = 0; round < MaxLazyRounds; round++)
			{
				var builtThisRound = false;
				var lazyElements = Root.Query<VisualElement>().Where(element => element is ILazyContent).ToList();
				foreach (var element in lazyElements)
				{
					if (((ILazyContent)element).ForceBuildContent(maxListElements))
						builtThisRound = true;
				}
				if (!builtThisRound)
					return builtAnything;
				builtAnything = true;
			}
			throw new InvalidOperationException($"Building the lazy content of '{Blueprint.InspectorType.Name}' did not finish after {MaxLazyRounds} rounds. Some content keeps creating new lazy content.");
		}

		private const int MaxLazyRounds = 64;

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(InspectorInstance));

		#endregion
	}

}
