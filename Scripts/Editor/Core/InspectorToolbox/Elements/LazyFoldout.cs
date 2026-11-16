using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// Something that holds content it has not built yet. Lets a test or a tool force everything into existence.
	/// </summary>
	public interface ILazyContent
	{
		/// <summary>
		/// Builds the content that is not built yet, whatever the visible state is. Returns true when something
		/// was built. <paramref name="maxListElements"/> limits how many elements of a list are built: a list
		/// then has that many rows at most, built aside, and shows none of its own, so that the result is the
		/// same whatever a layout would have shown. Meant for tests and tools, not for a list a person looks at.
		/// </summary>
		bool ForceBuildContent(int maxListElements);
	}

	/// <summary>
	/// A foldout whose content is built the first time it opens. Set <c>bindingPath</c> to a property to make the
	/// open state follow that property's expanded state, as Unity's own foldouts do.
	/// </summary>
	public class LazyFoldout : Foldout, ILazyContent
	{
		public const string LazyUssClassName = "ext-foldout";

		private readonly Action<VisualElement> BuildContent;
		private readonly bool ClearOnCollapse;

		public bool IsContentBuilt { get; private set; }

		/// <param name="buildContent">Fills the given content container. Bind what it adds when the container is attached to a panel.</param>
		/// <param name="clearOnCollapse">Drop the content when the foldout closes and build it again when it reopens.</param>
		public LazyFoldout(string title, Action<VisualElement> buildContent, bool expanded, bool clearOnCollapse = false)
		{
			BuildContent = buildContent ?? throw new ArgumentNullException(nameof(buildContent));
			ClearOnCollapse = clearOnCollapse;
			AddToClassList(LazyUssClassName);
			text = title;
			SetValueWithoutNotify(expanded);
			if (expanded)
				EnsureContentBuilt();
			LastSeenValue = expanded;
			this.RegisterValueChangedCallback(OnValueChanged);
			schedule.Execute(CheckValue).Every(ValueCheckIntervalMs);
		}

		private const int ValueCheckIntervalMs = 100;
		private bool LastSeenValue;

		/// <summary>
		/// Makes the foldout follow the expanded state of a property, in both directions and at any time: it
		/// opens and closes when the state is changed elsewhere (another inspector, a tool), and toggling it
		/// stores the state. Set <c>bindingPath</c> to the same property as well: Unity's binding then marks
		/// the foldout as belonging to the property, but it reads the state only when it binds.
		/// </summary>
		public void FollowExpandedStateOf(SerializedProperty property)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));
			StateObject = property.serializedObject;
			StatePropertyPath = property.propertyPath;
		}

		private SerializedObject StateObject;
		private string StatePropertyPath;

		/// <summary>
		/// The property whose expanded state is followed, or null when none is followed or it no longer exists
		/// (an element of a list that got shorter).
		/// </summary>
		private SerializedProperty FindStateProperty()
		{
			if (StateObject == null || StateObject.targetObject == null)
				return null;
			return StateObject.FindProperty(StatePropertyPath);
		}

		/// <summary>
		/// The expanded state of a property changes without any notification, and so does the value when a
		/// binding sets it. Both are compared with what was last seen a few times a second while the foldout is
		/// in a panel.
		/// </summary>
		private void CheckValue()
		{
			var stateProperty = FindStateProperty();
			if (stateProperty != null && stateProperty.isExpanded != value)
				SetValueWithoutNotify(stateProperty.isExpanded);
			if (value != LastSeenValue)
				OnExpandedStateChanged(value);
		}

		private void OnValueChanged(ChangeEvent<bool> evt)
		{
			if (evt.target != this)
				return;
			var stateProperty = FindStateProperty();
			if (stateProperty != null && stateProperty.isExpanded != evt.newValue)
				stateProperty.isExpanded = evt.newValue;
			OnExpandedStateChanged(evt.newValue);
		}

		private void OnExpandedStateChanged(bool expanded)
		{
			LastSeenValue = expanded;
			if (expanded)
			{
				EnsureContentBuilt();
			}
			else if (ClearOnCollapse && IsContentBuilt)
			{
				contentContainer.Clear();
				IsContentBuilt = false;
			}
		}

		public void EnsureContentBuilt()
		{
			if (IsContentBuilt)
				return;
			IsContentBuilt = true;
			BuildContent(contentContainer);
		}

		public bool ForceBuildContent(int maxListElements)
		{
			// Open now when the followed property says so, not at the next look a moment later: whoever forces
			// content then gets the same tree whatever the timing, and foldouts inside foldouts open one level
			// per call, as their content appears, not all levels in one layout update.
			CheckValue();
			if (IsContentBuilt)
				return false;
			EnsureContentBuilt();
			return true;
		}
	}

	/// <summary>
	/// A full-width section header in the colours of a component title bar. The content is built when the
	/// section opens and dropped when it closes, so a closed section costs one header.
	/// </summary>
	public class SectionFoldout : LazyFoldout
	{
		public const string UssClassName = "ext-section";

		public SectionFoldout(string title, Action<VisualElement> buildContent, bool expanded)
			: base(title, buildContent, expanded, clearOnCollapse: true)
		{
			AddToClassList(UssClassName);
		}
	}

}
