using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>An element made by the caller, for what no building block covers.</summary>
	public sealed class CustomNode<T> : InspectorNode<T, CustomNode<T>>
	{
		public Func<InspectorInstance, VisualElement> Factory;

		public override VisualElement Build(InspectorInstance instance)
		{
			return Factory(instance) ?? throw new InvalidOperationException("The factory of a custom node returned no element.");
		}
	}

	/// <summary>A block drawn with IMGUI inside the inspector. A stopgap for code that is not ported yet.</summary>
	public sealed class ImguiNode<T> : InspectorNode<T, ImguiNode<T>>
	{
		public Action<object> OnGUI;
		public bool RepaintWhilePlaying;

		public override VisualElement Build(InspectorInstance instance)
		{
			var container = new IMGUIContainer(() =>
			{
				var pass = instance.CreatePass();
				if (pass == null)
					return;
				foreach (var target in pass.Targets)
					OnGUI(target);
			});
			container.AddToClassList("ext-imgui");
			if (RepaintWhilePlaying)
			{
				container.schedule.Execute(() =>
				{
					if (EditorApplication.isPlaying)
						container.MarkDirtyRepaint();
				}).Every(InspectorInstance.LiveRefreshIntervalMs);
			}
			return container;
		}
	}

	public static class CustomFeature
	{
		/// <summary>Adds an element made by the factory. The context gives the serialized object and the property.</summary>
		public static CustomNode<T> Custom<T>(this IInspectorNodeContainer<T> container, Func<InspectorContext, VisualElement> factory)
		{
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));
			return container.Add(new CustomNode<T> { Factory = instance => factory(instance.Context) });
		}

		/// <summary>
		/// Adds an element made by the factory, which gets the inspector instance: to register evaluators and
		/// live refreshers, or to change the target with undo.
		/// </summary>
		public static CustomNode<T> CustomWithInstance<T>(this IInspectorNodeContainer<T> container, Func<InspectorInstance, VisualElement> factory)
		{
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));
			return container.Add(new CustomNode<T> { Factory = factory });
		}

		/// <summary>
		/// Draws a block with IMGUI. In the inspector of an object the action gets the object. In a nested
		/// inspector it gets a copy made from the serialized data.
		/// </summary>
		public static ImguiNode<T> IMGUI<T>(this IInspectorNodeContainer<T> container, Action<T> onGUI, bool repaintWhilePlaying = false)
		{
			if (onGUI == null)
				throw new ArgumentNullException(nameof(onGUI));
			return container.Add(new ImguiNode<T> { OnGUI = target => onGUI((T)target), RepaintWhilePlaying = repaintWhilePlaying });
		}
	}

}
