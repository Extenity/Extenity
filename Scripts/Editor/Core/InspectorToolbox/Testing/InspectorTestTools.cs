using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// Helpers for tests of inspectors made with the toolbox: build the inspector of an object on a panel that
	/// is never shown, force its lazy content into existence, list the customized inspector types of the
	/// project, and collect the errors logged meanwhile.
	/// </summary>
	public static class InspectorTestTools
	{
		/// <summary>
		/// Builds the inspector of the objects (several objects of one type for multi-object editing) on a hidden
		/// panel and binds it. An object without a UI Toolkit editor gets Unity's default UI Toolkit inspector,
		/// whichever editor would draw it in the Inspector window, so that the property drawers of its fields are
		/// built. Dispose the result.
		/// </summary>
		public static BuiltInspector Build(params Object[] targets)
		{
			if (targets == null || targets.Length == 0)
				throw new ArgumentException("At least one object is needed.", nameof(targets));
			foreach (var target in targets)
			{
				if (target == null)
					throw new ArgumentException("One of the objects is null or destroyed.", nameof(targets));
			}
			return new BuiltInspector(targets);
		}

		/// <summary>
		/// Every concrete editor and property drawer class in the project that is made with the toolbox and
		/// registered with Unity through a CustomEditor or CustomPropertyDrawer attribute.
		/// </summary>
		public static List<Type> GetCustomizedInspectorTypes()
		{
			var types = new List<Type>();
			foreach (var type in TypeCache.GetTypesDerivedFrom<ICustomizedInspector>())
			{
				if (type.IsAbstract || type.IsGenericTypeDefinition)
					continue;
				if (type.IsDefined(typeof(CustomEditor), true) || type.IsDefined(typeof(CustomPropertyDrawer), true))
					types.Add(type);
			}
			types.Sort((a, b) => string.CompareOrdinal(a.FullName, b.FullName));
			return types;
		}

		/// <summary>
		/// The blueprint of a customized editor or drawer class. Building it runs its Configure, which throws
		/// for an unknown field name, a field claimed twice and the other declaration mistakes.
		/// </summary>
		public static InspectorBlueprint GetBlueprint(Type inspectorType)
		{
			if (inspectorType == null)
				throw new ArgumentNullException(nameof(inspectorType));
			if (!typeof(ICustomizedInspector).IsAssignableFrom(inspectorType))
				throw new ArgumentException($"'{inspectorType.FullName}' is not made with the inspector toolbox.", nameof(inspectorType));

			if (typeof(ScriptableObject).IsAssignableFrom(inspectorType))
			{
				var editor = ScriptableObject.CreateInstance(inspectorType);
				try
				{
					return ((ICustomizedInspector)editor).GetBlueprint();
				}
				finally
				{
					Object.DestroyImmediate(editor);
				}
			}
			return ((ICustomizedInspector)Activator.CreateInstance(inspectorType)).GetBlueprint();
		}

		/// <summary>Sets the instantiation count of every blueprint built so far back to zero.</summary>
		public static void ResetInstantiationCounts()
		{
			foreach (var blueprint in InspectorBlueprints.Built)
				blueprint.InstantiationCount = 0;
		}

		/// <summary>Runs what a click on the button runs.</summary>
		public static void Click(Button button)
		{
			if (button == null)
				throw new ArgumentNullException(nameof(button));
			if (!button.enabledInHierarchy)
				throw new InvalidOperationException($"The button '{button.text}' is disabled and cannot be clicked.");
			var invoke = typeof(Clickable).GetMethod("Invoke", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public, null, new[] { typeof(EventBase) }, null)
			             ?? throw new MissingMethodException(nameof(Clickable), "Invoke");
			using (var evt = NavigationSubmitEvent.GetPooled())
			{
				invoke.Invoke(button.clickable, new object[] { evt });
			}
		}

		/// <summary>
		/// Remembers which properties of the objects are expanded and puts exactly that back when disposed. Unity
		/// keeps the expanded state per type and property path for the whole editor session, not per object, so
		/// whatever opens or closes a foldout of a temporary object changes what every other object of that type
		/// shows afterwards, in tests and in the Inspector window. Dispose it while the objects still exist.
		/// <see cref="Build"/> does this on its own for the objects it builds.
		/// </summary>
		public static ExpandedStateKeeper KeepExpandedStates(params Object[] targets)
		{
			if (targets == null || targets.Length == 0)
				throw new ArgumentException("At least one object is needed.", nameof(targets));
			foreach (var target in targets)
			{
				if (target == null)
					throw new ArgumentException("One of the objects is null or destroyed.", nameof(targets));
			}
			return new ExpandedStateKeeper(targets);
		}

		/// <summary>Starts collecting the errors, exceptions and failed assertions that are logged. Dispose it to stop.</summary>
		public static ErrorLogCollector CollectErrorLogs()
		{
			return new ErrorLogCollector();
		}
	}

	/// <summary>
	/// The expanded states of the properties of some objects as they were when this was created. Disposing it
	/// puts them back: what was open is open again, everything else is closed.
	/// </summary>
	public sealed class ExpandedStateKeeper : IDisposable
	{
		private readonly List<(Object Target, HashSet<string> ExpandedPaths)> Kept = new List<(Object, HashSet<string>)>();
		private bool IsDisposed;

		internal ExpandedStateKeeper(Object[] targets)
		{
			foreach (var target in targets)
			{
				var expandedPaths = new HashSet<string>(StringComparer.Ordinal);
				VisitFoldableProperties(target, property =>
				{
					if (property.isExpanded)
						expandedPaths.Add(property.propertyPath);
				});
				Kept.Add((target, expandedPaths));
			}
		}

		/// <summary>
		/// Closes every property of the objects, so that what is built next does not depend on what was left
		/// open earlier (by a person in the Inspector window, or by another test).
		/// </summary>
		public void CollapseAll()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(nameof(ExpandedStateKeeper));
			foreach (var (target, _) in Kept)
			{
				if (target == null)
					continue;
				VisitFoldableProperties(target, property =>
				{
					if (property.isExpanded)
						property.isExpanded = false;
				});
			}
		}

		public void Dispose()
		{
			if (IsDisposed)
				return;
			IsDisposed = true;
			foreach (var (target, expandedPaths) in Kept)
			{
				// The state of a destroyed object cannot be reached any more. Dispose before destroying.
				if (target == null)
					continue;
				VisitFoldableProperties(target, property =>
				{
					var wasExpanded = expandedPaths.Contains(property.propertyPath);
					if (property.isExpanded != wasExpanded)
						property.isExpanded = wasExpanded;
				});
			}
		}

		/// <summary>Every property that has a foldout state, at every depth, whether its parents are open or not.</summary>
		private static void VisitFoldableProperties(Object target, Action<SerializedProperty> visit)
		{
			using (var serializedObject = new SerializedObject(target))
			{
				var iterator = serializedObject.GetIterator();
				var enterChildren = true;
				while (iterator.NextVisible(enterChildren))
				{
					// The characters of a string are children too. They have no state.
					enterChildren = iterator.hasVisibleChildren && iterator.propertyType != SerializedPropertyType.String;
					if (enterChildren)
						visit(iterator);
				}
			}
		}
	}

	/// <summary>Collects the errors, exceptions and failed assertions logged while it is alive.</summary>
	public sealed class ErrorLogCollector : IDisposable
	{
		private readonly List<string> _Errors = new List<string>();
		public IReadOnlyList<string> Errors => _Errors;
		private bool IsDisposed;

		internal ErrorLogCollector()
		{
			Application.logMessageReceived += OnLogMessageReceived;
		}

		private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
		{
			if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
				_Errors.Add($"{type}: {condition}");
		}

		public void Dispose()
		{
			if (IsDisposed)
				return;
			IsDisposed = true;
			Application.logMessageReceived -= OnLogMessageReceived;
		}
	}

	/// <summary>An inspector built by <see cref="InspectorTestTools.Build"/>.</summary>
	public sealed class BuiltInspector : IDisposable
	{
		public HiddenInspectorPanel Panel { get; }
		/// <summary>The element tree of the inspector.</summary>
		public VisualElement Root { get; }
		public UnityEditor.Editor Editor { get; }
		public SerializedObject SerializedObject => Editor.serializedObject;
		/// <summary>The instance of the object's own inspector. Null when the object's editor is not made with the toolbox.</summary>
		public InspectorInstance RootInstance { get; }
		/// <summary>True when Unity's default UI Toolkit inspector was built because the object's editor has no UI Toolkit inspector.</summary>
		public bool UsesDefaultInspector { get; }

		private readonly ExpandedStateKeeper ExpandedStates;
		private bool IsDisposed;

		internal BuiltInspector(Object[] targets)
		{
			// Building can open lists that are declared open by default, and tests open and close what they look
			// at. All of it is put back in Dispose.
			ExpandedStates = new ExpandedStateKeeper(targets);
			Panel = new HiddenInspectorPanel();
			try
			{
				Editor = UnityEditor.Editor.CreateEditor(targets);
				if (Editor == null)
					throw new InvalidOperationException($"Unity created no editor for '{targets[0]}'.");
				using (SelectHandlerCache())
				{
					var gui = Editor.CreateInspectorGUI();
					if (gui == null)
					{
						gui = new VisualElement { name = "default-inspector" };
						InspectorElement.FillDefaultInspector(gui, Editor.serializedObject, Editor);
						UsesDefaultInspector = true;
					}
					Root = gui;
					Panel.Root.Add(Root);
					Root.Bind(Editor.serializedObject);
					Panel.Update();
				}
				RootInstance = (Root as InspectorRootElement)?.Instance;
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		/// <summary>
		/// Whatever binds a property while this inspector is built, updated or opened remembers its drawer in the
		/// cache of the inspector's own editor, as in the Inspector window, and not in Unity's global cache, where
		/// it would outlive the inspector and could be handed to a property of another object later (see
		/// <see cref="PropertyHandlerCacheScope"/>). The editor empties its cache when it is destroyed.
		/// </summary>
		private PropertyHandlerCacheScope SelectHandlerCache()
		{
			return PropertyHandlerCacheScope.SelectCacheOf(Editor);
		}

		/// <summary>Runs one update of the panel: layout, bindings, scheduled items.</summary>
		public void Update()
		{
			using (SelectHandlerCache())
				Panel.Update();
		}

		/// <summary>Every inspector instance in the tree: the object's own and those of nested drawers that are built.</summary>
		public List<InspectorInstance> GetAllInstances()
		{
			var instances = new List<InspectorInstance>();
			Root.Query<InspectorRootElement>().ForEach(element => instances.Add(element.Instance));
			return instances;
		}

		/// <summary>The validation messages of every instance in the tree.</summary>
		public List<ValidationMessage> GetAllMessages()
		{
			var messages = new List<ValidationMessage>();
			foreach (var instance in GetAllInstances())
				messages.AddRange(instance.Messages);
			return messages;
		}

		public void ReevaluateAll()
		{
			// Evaluating can build content (a group opening), which binds it.
			using (SelectHandlerCache())
			{
				foreach (var instance in GetAllInstances())
					instance.Reevaluate();
			}
		}

		public void RefreshAllLiveValues()
		{
			using (SelectHandlerCache())
			{
				foreach (var instance in GetAllInstances())
					instance.RefreshLiveValues();
			}
		}

		/// <summary>
		/// Opens every foldout that follows a property's expanded state (up to <paramref name="maxListElements"/>
		/// elements per list) and builds all other lazy content, repeating until nothing new appears. A toolbox
		/// list has that many rows at most afterwards, built aside (see <see cref="ExtListView.ForceBuildContent"/>).
		/// The expanded states are put back when this is disposed.
		/// <para>
		/// It opens one level per round: what is inside a foldout is opened in the round after the foldout
		/// itself, with an update of the panel in between. Opening everything in one update fails on a panel
		/// without a window: there a list view grows by a row or two per layout pass (in a window it takes its
		/// size in one step), lists inside the rows of lists add their passes, and above ten passes in one
		/// update Unity logs the error "Layout update is struggling to process current layout".
		/// </para>
		/// </summary>
		/// <param name="within">Only what is inside this element is opened and built. Null for the whole inspector.</param>
		/// <param name="opens">
		/// Says for a property path whether its foldout, list or row is opened and built. Null: every one. What
		/// belongs to no property (a group, a live list) is built wherever it appears.
		/// </param>
		public void ExpandAllLazyContent(int maxListElements, VisualElement within = null, Func<string, bool> opens = null)
		{
			if (maxListElements < 0)
				throw new ArgumentOutOfRangeException(nameof(maxListElements));
			using var handlerCache = SelectHandlerCache();
			var scope = within ?? Root;

			var serializedObject = SerializedObject;
			var previousCount = -1;
			for (var round = 0; round < MaxRounds; round++)
			{
				var changedSomething = false;
				var candidates = scope.Query<VisualElement>().Where(element => element is ILazyContent || element is Foldout).ToList();
				foreach (var element in candidates)
				{
					// Removed by what an earlier element of this round built.
					if (element.panel == null)
						continue;
					var propertyPath = GetPropertyPath(element);
					if (propertyPath != null && (IsOutOfReach(propertyPath, maxListElements) || (opens != null && !opens(propertyPath))))
						continue;
					if (element is Foldout && propertyPath != null)
					{
						var property = serializedObject.FindProperty(propertyPath);
						if (property != null && property.hasVisibleChildren && !property.isExpanded)
						{
							property.isExpanded = true;
							changedSomething = true;
						}
					}
					// A foldout of the toolbox takes over the state of its property here and opens.
					if (element is ILazyContent lazyContent && lazyContent.ForceBuildContent(maxListElements))
						changedSomething = true;
				}
				Panel.Update();

				// A foldout of Unity builds its content when it is told to open. It is told only now, after the
				// update: setting the value sends an event at once, and with it every event still waiting from
				// fields bound a moment ago. Handled that early, the event of a slider that shows a value outside
				// its range clamped (0 as 0.1) is taken for an edit and the clamped value is written to the object.
				var openedSomething = false;
				foreach (var element in candidates)
				{
					if (element.panel == null || element is ILazyContent || !(element is Foldout foldout) || foldout.value)
						continue;
					var propertyPath = GetPropertyPath(element);
					if (propertyPath == null || IsOutOfReach(propertyPath, maxListElements) || (opens != null && !opens(propertyPath)))
						continue;
					var property = serializedObject.FindProperty(propertyPath);
					if (property != null && property.hasVisibleChildren)
					{
						foldout.value = true;
						openedSomething = true;
					}
				}
				if (openedSomething)
				{
					changedSomething = true;
					Panel.Update();
				}
				var count = scope.Query<VisualElement>().ToList().Count;
				if (!changedSomething && count == previousCount)
					return;
				previousCount = count;
			}
			throw new InvalidOperationException($"Building the lazy content did not settle after {MaxRounds} rounds.");
		}

		/// <summary>The property a foldout or a list belongs to, or null.</summary>
		private static string GetPropertyPath(VisualElement element)
		{
			if (element is ExtListView list)
				return list.ArrayPropertyPath;
			if (element is ExtTableView table)
				return table.ArrayPropertyPath;
			return element is IBindable bindable && !string.IsNullOrEmpty(bindable.bindingPath) ? bindable.bindingPath : null;
		}

		private const int MaxRounds = 64;
		private const int MaxDepth = 16;
		private const string ArrayElementPrefix = "Array.data[";

		/// <summary>
		/// True for a property inside a list element past the first <paramref name="maxListElements"/>, and
		/// for one nested deeper than any real data (a type that contains itself would never end).
		/// </summary>
		private static bool IsOutOfReach(string propertyPath, int maxListElements)
		{
			var depth = 0;
			var position = 0;
			while (position < propertyPath.Length)
			{
				var end = propertyPath.IndexOf('.', position);
				if (end < 0)
					end = propertyPath.Length;
				if (string.CompareOrdinal(propertyPath, position, ArrayElementPrefix, 0, ArrayElementPrefix.Length) == 0)
				{
					// "Array.data[12]": the dot inside belongs to the segment.
					var open = position + ArrayElementPrefix.Length;
					var close = propertyPath.IndexOf(']', open);
					if (close < 0)
						throw new FormatException($"'{propertyPath}' is not a property path.");
					if (int.Parse(propertyPath.Substring(open, close - open), System.Globalization.CultureInfo.InvariantCulture) >= maxListElements)
						return true;
					end = close + 1;
				}
				depth++;
				position = end + 1;
			}
			return depth > MaxDepth * 2;
		}

		public void Dispose()
		{
			if (IsDisposed)
				return;
			IsDisposed = true;

			if (Editor != null)
			{
				Root?.RemoveFromHierarchy();
				Object.DestroyImmediate(Editor);
			}
			Panel?.Dispose();
			ExpandedStates.Dispose();
		}
	}

}
