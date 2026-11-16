using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// Keeps long field labels readable. A label that does not fit the label column wraps onto more lines and
	/// gets its full text as a tooltip. A label that would need more than two lines is stacked above its field,
	/// where it has the full width. Nothing is cut off.
	/// </summary>
	public static class LongLabelFitter
	{
		public const string MeasuredLabelUssClassName = "ext-label--measured";
		public const string WrappedLabelUssClassName = "ext-label--wrapped";
		public const string StackedLabelUssClassName = "ext-label--stacked";
		public const string StackedFieldUssClassName = "ext-field--stacked";
		public const string WrappedFieldUssClassName = "ext-field--wrapped-label";

		private const string BaseFieldLabelClass = "unity-base-field__label";

		private sealed class State
		{
			public bool IsScheduled;
			public float LastWidth = float.NaN;
			public int RetryCount;
		}

		private static readonly ConditionalWeakTable<VisualElement, State> States = new ConditionalWeakTable<VisualElement, State>();

		/// <summary>
		/// Fits the labels under the outermost toolbox root that contains the element, now and whenever that root
		/// changes its size. Cheap to call often.
		/// </summary>
		public static void Request(VisualElement element)
		{
			if (element == null || element.panel == null)
				return;

			var root = element;
			for (var current = element; current != null; current = current.hierarchy.parent)
			{
				if (current.ClassListContains(InspectorStyles.RootClass))
					root = current;
			}

			if (!States.TryGetValue(root, out var state))
			{
				state = new State();
				States.Add(root, state);
				root.RegisterCallback<GeometryChangedEvent, State>(OnRootGeometryChanged, state);
			}
			state.RetryCount = 0;
			Schedule(root, state);
		}

		private static void OnRootGeometryChanged(GeometryChangedEvent evt, State state)
		{
			var root = (VisualElement)evt.currentTarget;
			if (!float.IsNaN(state.LastWidth) && !Approximately(state.LastWidth, evt.newRect.width))
				Reset(root);
			state.LastWidth = evt.newRect.width;
			Schedule(root, state);
		}

		private static void Schedule(VisualElement root, State state)
		{
			if (state.IsScheduled)
				return;
			state.IsScheduled = true;
			// Two ways to the same pass, whichever comes first. The scheduler of the panel is the regular one, but
			// in an editor window it can let half a second go by before it runs a new item, and that long a
			// label would stay cut off and the fields below would jump afterwards. The delayed call of the
			// editor runs on its next update. It never runs for a panel that is updated by hand (tests).
			root.schedule.Execute(() => RunScheduledPass(root, state));
			EditorApplication.delayCall += () => RunScheduledPass(root, state);
		}

		private static void RunScheduledPass(VisualElement root, State state)
		{
			if (!state.IsScheduled)
				return; // The other way was faster.
			state.IsScheduled = false;
			var waitingForLayout = FitLabels(root);
			if (waitingForLayout == 0)
			{
				state.RetryCount = 0;
			}
			else if (state.RetryCount < MaxRetryCount && root.panel != null)
			{
				// Fields that were bound a moment ago have labels without a layout yet, and nothing announces
				// when they get one: look again on the next tick. A label of a hidden field never gets a
				// layout, hence the limit.
				state.RetryCount++;
				Schedule(root, state);
			}
		}

		private const int MaxRetryCount = 30;

		private static void Reset(VisualElement root)
		{
			root.Query<Label>(className: MeasuredLabelUssClassName).ForEach(label =>
			{
				label.RemoveFromClassList(MeasuredLabelUssClassName);
				label.RemoveFromClassList(WrappedLabelUssClassName);
				label.RemoveFromClassList(StackedLabelUssClassName);
				if (label.parent != null)
				{
					label.parent.RemoveFromClassList(WrappedFieldUssClassName);
					if (label.parent.ClassListContains(StackedFieldUssClassName))
					{
						label.parent.RemoveFromClassList(StackedFieldUssClassName);
						FieldLabelTools.RestoreLabelAlignment(label.parent);
					}
				}
			});
		}

		/// <summary>Measures every label under the root that was not measured yet and has a layout.</summary>
		public static void Fit(VisualElement root)
		{
			FitLabels(root);
		}

		/// <summary>Returns how many labels could not be measured because they have no layout yet.</summary>
		private static int FitLabels(VisualElement root)
		{
			if (root.panel == null)
				return 0;
			var waitingForLayout = 0;
			root.Query<Label>(className: BaseFieldLabelClass).ForEach(label =>
			{
				if (!FitLabel(label))
					waitingForLayout++;
			});
			return waitingForLayout;
		}

		/// <summary>Returns false when the label has a text but no layout yet, so that it must be looked at again.</summary>
		private static bool FitLabel(Label label)
		{
			if (label.ClassListContains(MeasuredLabelUssClassName))
				return true;
			if (string.IsNullOrEmpty(label.text))
				return true; // Nothing to fit.
			var available = label.contentRect.width;
			if (float.IsNaN(available))
				return false; // Not laid out yet. Measured on a later pass.
			if (available <= 0f)
			{
				// Either not laid out yet, or the field starts right of the inspector's label column (deep inside
				// lists), where Unity gives the label no width at all. In that case it goes above its field.
				var fieldWidth = label.parent != null ? label.parent.contentRect.width : float.NaN;
				if (float.IsNaN(fieldWidth) || fieldWidth <= 0f)
					return false;
				available = 0f;
			}

			label.AddToClassList(MeasuredLabelUssClassName);
			var needed = label.MeasureTextSize(label.text, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined).x;
			if (needed <= available + 0.5f)
				return true;

			if (string.IsNullOrEmpty(label.tooltip))
				label.tooltip = label.text;
			// A column narrower than a short word would break the text letter by letter: no wrapping there.
			if (available >= MinimumWidthForWrapping && needed <= available * 2f - WrapSafetyMargin)
			{
				label.AddToClassList(WrappedLabelUssClassName);
				label.parent?.AddToClassList(WrappedFieldUssClassName);
			}
			else
			{
				label.AddToClassList(StackedLabelUssClassName);
				if (label.parent != null)
				{
					label.parent.AddToClassList(StackedFieldUssClassName);
					// The label takes the full width above the field: Unity must stop sizing it to the label column,
					// now and when it aligns the field later (a field that was not aligned yet when it was measured).
					FieldLabelTools.ReleaseLabelAlignment(label.parent);
					label.parent.UnregisterCallback<GeometryChangedEvent>(KeepStackedFieldReleased);
					label.parent.RegisterCallback<GeometryChangedEvent>(KeepStackedFieldReleased);
				}
			}
			return true;
		}

		private static void KeepStackedFieldReleased(GeometryChangedEvent evt)
		{
			var field = (VisualElement)evt.currentTarget;
			if (!field.ClassListContains(StackedFieldUssClassName))
				return;
			FieldLabelTools.ReleaseLabelAlignment(field);
			foreach (var child in field.Children())
			{
				if (child is Label label && label.ClassListContains(BaseFieldLabelClass))
				{
					// A width Unity set in the same layout change, before the alignment was released.
					if (label.style.width.keyword != StyleKeyword.Null)
						label.style.width = StyleKeyword.Null;
					if (label.style.minWidth.keyword != StyleKeyword.Null)
						label.style.minWidth = StyleKeyword.Null;
					break;
				}
			}
		}

		// Words do not break at the exact column width, so two lines hold a little less than twice the width.
		private const float WrapSafetyMargin = 24f;
		private const float MinimumWidthForWrapping = 70f;

		private static bool Approximately(float a, float b)
		{
			return a > b - 0.5f && a < b + 0.5f;
		}
	}

}
