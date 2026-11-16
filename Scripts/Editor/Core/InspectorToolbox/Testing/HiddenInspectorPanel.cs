using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// An editor UI Toolkit panel that belongs to no window and is never shown. Elements added to <see cref="Root"/>
	/// are attached to a real panel, so property fields resolve their drawers, bindings work and scheduled items
	/// can be run by calling <see cref="Update"/>. Meant for tests and tools that must build an inspector without
	/// opening one. Dispose it when done.
	/// </summary>
	public sealed class HiddenInspectorPanel : IDisposable
	{
		/// <summary>The element to add content to. It is 480 px wide, like a narrow Inspector window.</summary>
		public VisualElement Root { get; }

		private readonly HiddenPanelOwner Owner;
		private readonly IPanel Panel;
		private bool IsDisposed;

		public HiddenInspectorPanel(float width = 480f)
		{
			Owner = ScriptableObject.CreateInstance<HiddenPanelOwner>();
			Owner.hideFlags = HideFlags.HideAndDontSave;
			Panel = (IPanel)FindOrCreateMethod.Invoke(null, new object[] { Owner })
			        ?? throw new InvalidOperationException("Unity created no editor panel for the hidden panel owner.");

			Root = new VisualElement { name = "hidden-inspector-panel-root" };
			Root.style.position = Position.Absolute;
			Root.style.left = 0f;
			Root.style.top = 0f;
			Root.style.width = width;
			Panel.visualTree.Add(Root);
		}

		/// <summary>
		/// Runs what Unity runs for a visible panel once per editor update: styles and layout, bindings (tracked
		/// property values included) and scheduled items. Bindings are polled by Unity at most every 100 ms,
		/// so wait that long between two calls when a binding must notice a change. Unlike an editor update,
		/// it binds every element that waits to be bound (see <see cref="BindAllWaitingElements"/>).
		/// </summary>
		public void Update()
		{
			ThrowIfDisposed();
			Update(Panel);
		}

		/// <summary>
		/// Runs the same steps for the panel an element is shown in, for example the panel of an editor window
		/// that a test opened: a test runs inside one editor update, in which Unity updates no window.
		/// </summary>
		public static void UpdatePanelOf(VisualElement element)
		{
			if (element == null)
				throw new ArgumentNullException(nameof(element));
			var panel = element.panel
			            ?? throw new InvalidOperationException("The element is not shown in a panel, so there is no panel to update.");
			Update(panel);
		}

		private static void Update(IPanel panel)
		{
			UpdateStylesAndLayout(panel);
			BindAllWaitingElements(panel);
			TickSchedulingUpdatersMethod.Invoke(panel, null);
			UpdateScheduledEventsMethod.Invoke(SchedulerProperty.GetValue(panel), null);
			UpdateStylesAndLayout(panel);
		}

		/// <summary>
		/// Unity binds the elements that wait to be bound for at most 100 ms per editor update and leaves the
		/// rest for the next updates. A window gets an update every frame, so the rest follows a moment later.
		/// Here the next update is the next call, and how much of a large inspector is bound by then would
		/// depend on how fast the machine is at that moment: a caller that waits for the tree to stop changing
		/// would wait a different number of calls each time, or stop while fields are still unbound. So the
		/// update of bindings is repeated until nothing waits.
		/// </summary>
		private static void BindAllWaitingElements(IPanel panel)
		{
			var waiting = ElementsToBindField.GetValue(GetUpdaterMethod.Invoke(panel, new[] { BindingsPhase })) as HashSet<VisualElement>
			              ?? throw new InvalidOperationException("Unity's list of the elements that wait to be bound was not found.");
			for (var pass = 0; pass < MaxBindingPasses; pass++)
			{
				UpdateBindingsMethod.Invoke(panel, null);
				if (waiting.Count == 0)
					return;
			}
			throw new InvalidOperationException($"{waiting.Count} elements still wait to be bound after {MaxBindingPasses} binding updates. Binding them keeps asking for more binding.");
		}

		/// <summary>Far above what is needed: building every foldout of an inspector with about 34,000 elements needed at most 82 in one call.</summary>
		private const int MaxBindingPasses = 10000;

		/// <summary>
		/// Resolves styles and lays the tree out. The panel's own layout validation is not used because it asks
		/// for the pixel scale of the window the panel is shown in, and this panel has none.
		/// </summary>
		public void UpdateStylesAndLayout()
		{
			ThrowIfDisposed();
			UpdateStylesAndLayout(Panel);
		}

		private static void UpdateStylesAndLayout(IPanel panel)
		{
			var updater = VisualTreeUpdaterProperty.GetValue(panel);
			UpdateVisualTreePhaseMethod.Invoke(updater, new[] { StylesPhase });
			UpdateVisualTreePhaseMethod.Invoke(updater, new[] { LayoutPhase });
		}

		public void Dispose()
		{
			if (IsDisposed)
				return;
			IsDisposed = true;
			Root.RemoveFromHierarchy();
			((IDisposable)Panel).Dispose();
			Object.DestroyImmediate(Owner);
		}

		private void ThrowIfDisposed()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(nameof(HiddenInspectorPanel));
		}

		#region Unity internals

		private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		private static readonly Type EditorPanelType =
			typeof(UnityEditor.UIElements.PropertyField).Assembly.GetType("UnityEditor.UIElements.EditorPanel", false)
			?? typeof(EditorWindow).Assembly.GetType("UnityEditor.UIElements.EditorPanel", false)
			?? FindTypeInLoadedAssemblies("UnityEditor.UIElements.EditorPanel");

		private static readonly Type BasePanelType =
			typeof(VisualElement).Assembly.GetType("UnityEngine.UIElements.BaseVisualElementPanel", true);

		private static readonly Type PanelType =
			typeof(VisualElement).Assembly.GetType("UnityEngine.UIElements.Panel", true);

		private static readonly MethodInfo FindOrCreateMethod =
			EditorPanelType.GetMethod("FindOrCreate", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(ScriptableObject) }, null)
			?? throw new MissingMethodException("UnityEditor.UIElements.EditorPanel", "FindOrCreate");

		private static readonly PropertyInfo VisualTreeUpdaterProperty =
			PanelType.GetProperty("visualTreeUpdater", AnyInstance)
			?? throw new MissingMemberException("UnityEngine.UIElements.Panel", "visualTreeUpdater");

		private static readonly Type UpdatePhaseType =
			typeof(VisualElement).Assembly.GetType("UnityEngine.UIElements.VisualTreeUpdatePhase", true);

		private static readonly MethodInfo UpdateVisualTreePhaseMethod =
			VisualTreeUpdaterProperty.PropertyType.GetMethod("UpdateVisualTreePhase", AnyInstance, null, new[] { UpdatePhaseType }, null)
			?? throw new MissingMethodException("UnityEngine.UIElements.VisualTreeUpdater", "UpdateVisualTreePhase");

		private static readonly object StylesPhase = Enum.Parse(UpdatePhaseType, "Styles");
		private static readonly object LayoutPhase = Enum.Parse(UpdatePhaseType, "Layout");
		private static readonly MethodInfo UpdateBindingsMethod = RequireMethod(BasePanelType, "UpdateBindings");

		private static readonly object BindingsPhase = Enum.Parse(UpdatePhaseType, "Bindings");

		private static readonly MethodInfo GetUpdaterMethod =
			BasePanelType.GetMethod("GetUpdater", AnyInstance, null, new[] { UpdatePhaseType }, null)
			?? throw new MissingMethodException("UnityEngine.UIElements.BaseVisualElementPanel", "GetUpdater");

		private static readonly FieldInfo ElementsToBindField =
			typeof(VisualElement).Assembly.GetType("UnityEngine.UIElements.VisualTreeBindingsUpdater", true).GetField("m_ElementsToBind", AnyInstance)
			?? throw new MissingFieldException("UnityEngine.UIElements.VisualTreeBindingsUpdater", "m_ElementsToBind");
		private static readonly MethodInfo TickSchedulingUpdatersMethod = RequireMethod(BasePanelType, "TickSchedulingUpdaters");

		private static readonly PropertyInfo SchedulerProperty =
			BasePanelType.GetProperty("scheduler", AnyInstance)
			?? throw new MissingMemberException("UnityEngine.UIElements.BaseVisualElementPanel", "scheduler");

		private static readonly MethodInfo UpdateScheduledEventsMethod =
			RequireMethod(SchedulerProperty.PropertyType, "UpdateScheduledEvents");

		private static MethodInfo RequireMethod(Type type, string name)
		{
			return type.GetMethod(name, AnyInstance, null, Type.EmptyTypes, null)
			       ?? throw new MissingMethodException(type.FullName, name);
		}

		private static Type FindTypeInLoadedAssemblies(string fullName)
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var type = assembly.GetType(fullName, false);
				if (type != null)
					return type;
			}
			throw new TypeLoadException($"Type '{fullName}' was not found in any loaded assembly.");
		}

		#endregion
	}

}
