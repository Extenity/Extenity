using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// A value that is not serialized, read from the target several times a second: a property, a computed
	/// value, runtime state. With a setter it can be edited.
	/// </summary>
	public sealed class LiveNode<T, V> : InspectorNode<T, LiveNode<T, V>>
	{
		public const string MultipleObjectsText = "— (multiple objects)";
		public const string PlayModeOnlyText = "Play Mode only";

		public string Label;
		public Func<object, V> Getter;
		public Action<object, V> Setter;
		public float? RangeMin;
		public float? RangeMax;
		public Func<object, Color?> TintGetter;
		public bool IsPlayModeOnly;
		public bool IsMultiline;
		public readonly List<Func<object, InspectorContext, bool>> EnabledRules = new List<Func<object, InspectorContext, bool>>();
		public Func<object, InspectorContext, string> DisabledTooltip;

		/// <summary>
		/// An editable live value can be edited only while the predicate holds. Otherwise it still shows the
		/// current value, disabled, with what <paramref name="disabledTooltip"/> returns as its tooltip.
		/// </summary>
		public LiveNode<T, V> EnabledWhen(Func<T, InspectorContext, bool> predicate, Func<T, InspectorContext, string> disabledTooltip = null)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			if (Setter == null)
				throw new InvalidOperationException($"The live value '{Label}' is read-only: only a live field can be enabled and disabled.");
			EnabledRules.Add((target, context) => predicate((T)target, context));
			if (disabledTooltip != null)
				DisabledTooltip = (target, context) => disabledTooltip((T)target, context);
			return this;
		}

		/// <summary>Read-only: draws the value as a bar within the range. Editable: edits it with a slider.</summary>
		public LiveNode<T, V> Range(float min, float max)
		{
			if (!(max > min))
				throw new ArgumentException("The range needs a maximum above its minimum.");
			if (typeof(V) != typeof(float) && typeof(V) != typeof(int) && typeof(V) != typeof(double))
				throw new InvalidOperationException($"A range needs a number. The live value '{Label}' is a '{typeof(V).Name}'.");
			RangeMin = min;
			RangeMax = max;
			return this;
		}

		/// <summary>Colours a read-only value.</summary>
		public LiveNode<T, V> Tint(Func<T, Color> tint)
		{
			if (tint == null)
				throw new ArgumentNullException(nameof(tint));
			TintGetter = target => tint((T)target);
			return this;
		}

		/// <summary>Colours a read-only value while the function returns a colour; null shows it in the default colours.</summary>
		public LiveNode<T, V> Tint(Func<T, Color?> tint)
		{
			if (tint == null)
				throw new ArgumentNullException(nameof(tint));
			TintGetter = target => tint((T)target);
			return this;
		}

		/// <summary>Outside Play Mode the value is not read and a muted note is shown.</summary>
		public LiveNode<T, V> PlayModeOnly()
		{
			IsPlayModeOnly = true;
			return this;
		}

		/// <summary>Lets a read-only text take as many lines as it needs.</summary>
		public LiveNode<T, V> Multiline()
		{
			IsMultiline = true;
			return this;
		}

		public override VisualElement Build(InspectorInstance instance)
		{
			return Setter == null ? BuildReadOnly(instance) : BuildEditable(instance);
		}

		private VisualElement BuildReadOnly(InspectorInstance instance)
		{
			var field = new LiveValueField(Label) { IsMultiline = IsMultiline };
			void Refresh(EvaluationPass pass)
			{
				if (IsPlayModeOnly && !pass.Context.IsPlaying)
				{
					field.ShowPlaceholder(PlayModeOnlyText);
					return;
				}
				if (!pass.HasSingleTarget)
				{
					field.ShowPlaceholder(MultipleObjectsText);
					return;
				}
				V value;
				try
				{
					value = Getter(pass.SingleTarget);
					field.SetTint(TintGetter?.Invoke(pass.SingleTarget));
				}
				catch (Exception exception)
				{
					// Shown in place, and rethrown so that the instance logs it.
					field.ShowError("Error: " + exception.Message);
					throw;
				}
				field.ShowValue(LiveValueField.Format(value));
				if (RangeMin.HasValue)
					field.ShowBar(Convert.ToSingle(value), RangeMin.Value, RangeMax.Value);
			}
			instance.OnEvaluate(Refresh);
			instance.OnRefreshLive(Refresh);
			return field;
		}

		private VisualElement BuildEditable(InspectorInstance instance)
		{
			var input = LiveInput<V>.Create(Label, RangeMin, RangeMax);
			input.Changed = value =>
			{
				// What a live field edits is not serialized, so there is nothing to record for an object. A nested
				// target is a copy, which has to be written back to have any effect.
				instance.Mutate(Label, instance.Context.PropertyPath != null, target =>
				{
					Setter(target, value);
					return target;
				});
			};
			void Refresh(EvaluationPass pass)
			{
				var canRead = pass.HasSingleTarget && (!IsPlayModeOnly || pass.Context.IsPlaying);
				var isAllowed = true;
				if (canRead)
				{
					foreach (var rule in EnabledRules)
					{
						if (!rule(pass.SingleTarget, pass.Context))
						{
							isAllowed = false;
							break;
						}
					}
				}
				var canEdit = canRead && isAllowed;
				input.Element.SetEnabled(canEdit);
				input.Element.tooltip = IsPlayModeOnly && !pass.Context.IsPlaying ? "Available in Play Mode." : !isAllowed ? DisabledTooltip?.Invoke(pass.SingleTarget, pass.Context) ?? "" : "";
				// A disabled field still shows the current value; a field that is being edited keeps what is typed.
				if (canRead && !(canEdit && input.IsBeingEdited))
					input.Show(Getter(pass.SingleTarget));
			}
			instance.OnEvaluate(Refresh);
			instance.OnRefreshLive(Refresh);
			return input.Element;
		}
	}

	/// <summary>The input element of an editable live value.</summary>
	internal sealed class LiveInput<V>
	{
		public VisualElement Element;
		public Action<V> Show;
		public Action<V> Changed;
		public bool IsBeingEdited => Element.panel != null && Element.panel.focusController.focusedElement is VisualElement focused && (focused == Element || Element.Contains(focused));

		public static bool IsSupported()
		{
			var type = typeof(V);
			return type == typeof(int) || type == typeof(float) || type == typeof(double) || type == typeof(bool) || type == typeof(string)
			       || type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Color)
			       || type.IsEnum || typeof(UnityEngine.Object).IsAssignableFrom(type);
		}

		public static LiveInput<V> Create(string label, float? min, float? max)
		{
			var type = typeof(V);
			if (type == typeof(float))
			{
				return min.HasValue
					? Make(new Slider(label, min.Value, max.Value) { showInputField = true }, value => (float)(object)value, value => (V)(object)value)
					: Make(new FloatField(label), value => (float)(object)value, value => (V)(object)value);
			}
			if (type == typeof(int))
			{
				return min.HasValue
					? Make(new SliderInt(label, Mathf.RoundToInt(min.Value), Mathf.RoundToInt(max.Value)) { showInputField = true }, value => (int)(object)value, value => (V)(object)value)
					: Make(new IntegerField(label), value => (int)(object)value, value => (V)(object)value);
			}
			if (type == typeof(double))
				return Make(new DoubleField(label), value => (double)(object)value, value => (V)(object)value);
			if (type == typeof(bool))
				return Make(new Toggle(label), value => (bool)(object)value, value => (V)(object)value);
			if (type == typeof(string))
				return Make(new TextField(label) { isDelayed = true }, value => (string)(object)value ?? "", value => (V)(object)value);
			if (type == typeof(Vector2))
				return Make(new Vector2Field(label), value => (Vector2)(object)value, value => (V)(object)value);
			if (type == typeof(Vector3))
				return Make(new Vector3Field(label), value => (Vector3)(object)value, value => (V)(object)value);
			if (type == typeof(Color))
				return Make(new ColorField(label), value => (Color)(object)value, value => (V)(object)value);
			if (type.IsEnum)
				return Make(new EnumField(label, (Enum)Enum.ToObject(type, 0)), value => (Enum)(object)value, value => (V)(object)value);
			if (typeof(UnityEngine.Object).IsAssignableFrom(type))
				return Make(new ObjectField(label) { objectType = type, allowSceneObjects = true }, value => (UnityEngine.Object)(object)value, value => (V)(object)value);
			throw new NotSupportedException($"An editable live value of type '{type.FullName}' is not supported.");
		}

		private static LiveInput<V> Make<TField>(BaseField<TField> field, Func<V, TField> toField, Func<TField, V> fromField)
		{
			field.AddToClassList(BaseField<TField>.alignedFieldUssClassName);
			field.AddToClassList("ext-live-field");
			var input = new LiveInput<V> { Element = field };
			input.Show = value => field.SetValueWithoutNotify(toField(value));
			field.RegisterValueChangedCallback(evt =>
			{
				if (evt.target == field)
					input.Changed?.Invoke(fromField(evt.newValue));
			});
			return input;
		}
	}

	/// <summary>
	/// A collection that is not serialized, listed read-only in a foldout and read again while it is open:
	/// a list (one text per element) or a dictionary (key and value).
	/// </summary>
	public sealed class LiveListNode<T> : InspectorNode<T, LiveListNode<T>>
	{
		/// <summary>More elements than this are counted but not listed.</summary>
		public const int MaxShownElements = 100;

		public string Label;
		/// <summary>The target to its elements, each a text pair: key and value (the key is null for a plain list).</summary>
		public Func<object, IEnumerable<(object Key, object Value)>> Getter;
		public bool IsTable;
		public bool IsPlayModeOnly;
		public bool IsExpandedByDefault;

		/// <summary>Outside Play Mode the collection is not read and a muted note is shown.</summary>
		public LiveListNode<T> PlayModeOnly()
		{
			IsPlayModeOnly = true;
			return this;
		}

		/// <summary>
		/// The list starts open until it is closed by hand. Its state is kept per target type and label for the
		/// editor session, like a foldout group.
		/// </summary>
		public LiveListNode<T> ExpandedByDefault()
		{
			IsExpandedByDefault = true;
			return this;
		}

		public override VisualElement Build(InspectorInstance instance)
		{
			var targetType = instance.Blueprint.TargetType;
			var stateKey = "Live:" + Label;
			var foldout = new Foldout { text = Label, value = FoldoutStates.Get(targetType, stateKey, IsExpandedByDefault) };
			foldout.AddToClassList("ext-live-list");
			foldout.RegisterValueChangedCallback(evt =>
			{
				if (evt.target == foldout)
					FoldoutStates.Set(targetType, stateKey, evt.newValue);
			});
			var count = new Label { pickingMode = PickingMode.Ignore };
			count.AddToClassList("ext-list__count");
			foldout.Q<Toggle>(className: Foldout.toggleUssClassName).Add(count);
			var rows = new VisualElement();
			rows.AddToClassList("ext-live-list__rows");
			foldout.Add(rows);

			void Refresh(EvaluationPass pass)
			{
				if (IsPlayModeOnly && !pass.Context.IsPlaying)
				{
					count.text = LiveNode<T, object>.PlayModeOnlyText;
					rows.Clear();
					return;
				}
				if (!pass.HasSingleTarget)
				{
					count.text = LiveNode<T, object>.MultipleObjectsText;
					rows.Clear();
					return;
				}
				var elements = Getter(pass.SingleTarget);
				if (elements == null)
				{
					count.text = "null";
					rows.Clear();
					return;
				}

				var total = 0;
				var shown = 0;
				foreach (var (key, value) in elements)
				{
					total++;
					if (!foldout.value || shown >= MaxShownElements)
						continue;
					var row = shown < rows.childCount ? (LiveListRow)rows[shown] : null;
					if (row == null)
					{
						row = new LiveListRow(IsTable);
						rows.Add(row);
					}
					row.Show(shown, key, value);
					shown++;
				}
				count.text = total == 1 ? "1 item" : total + " items";
				if (foldout.value && total > shown)
				{
					var more = shown < rows.childCount ? (LiveListRow)rows[shown] : null;
					if (more == null)
					{
						more = new LiveListRow(IsTable);
						rows.Add(more);
					}
					more.ShowNote($"... and {total - shown} more");
					shown++;
				}
				while (rows.childCount > shown)
					rows.RemoveAt(rows.childCount - 1);
			}
			instance.OnEvaluate(Refresh);
			instance.OnRefreshLive(Refresh);
			return foldout;
		}

		private sealed class LiveListRow : VisualElement
		{
			private readonly Label KeyLabel;
			private readonly Label ValueLabel;
			private readonly bool IsTable;
			private UnityEngine.Object PingTarget;

			public LiveListRow(bool isTable)
			{
				IsTable = isTable;
				AddToClassList("ext-live-list__row");
				KeyLabel = new Label();
				KeyLabel.AddToClassList("ext-live-list__key");
				Add(KeyLabel);
				ValueLabel = new Label();
				ValueLabel.selection.isSelectable = true;
				ValueLabel.AddToClassList("ext-live-list__value");
				Add(ValueLabel);
				RegisterCallback<ClickEvent>(evt =>
				{
					if (evt.clickCount == 2 && PingTarget != null)
						EditorGUIUtility.PingObject(PingTarget);
				});
			}

			public void Show(int index, object key, object value)
			{
				KeyLabel.text = IsTable ? LiveValueField.Format(key) : index.ToString();
				KeyLabel.EnableInClassList("ext-live-list__key--index", !IsTable);
				ValueLabel.text = LiveValueField.Format(value);
				PingTarget = value as UnityEngine.Object;
				tooltip = PingTarget != null ? "Double-click to show the object." : "";
			}

			public void ShowNote(string text)
			{
				KeyLabel.text = "";
				ValueLabel.text = text;
				PingTarget = null;
				tooltip = "";
			}
		}
	}

	public static class LiveFeature
	{
		/// <summary>
		/// Shows a value that is not serialized, read again every 100 ms while the inspector is shown. It needs a
		/// single selected object. In a nested inspector the getter gets a copy made from serialized data.
		/// </summary>
		public static LiveNode<T, V> Live<T, V>(this IInspectorNodeContainer<T> container, string label, Func<T, V> getter)
		{
			if (string.IsNullOrEmpty(label))
				throw new ArgumentException("A live value needs a label.", nameof(label));
			if (getter == null)
				throw new ArgumentNullException(nameof(getter));
			return container.Add(new LiveNode<T, V> { Label = label, Getter = target => getter((T)target) });
		}

		/// <summary>
		/// An editable value that is not serialized. Supported types: int, float, double, bool, string, enums,
		/// Vector2, Vector3, Color and Unity objects. No undo step is recorded for an object's own state.
		/// </summary>
		public static LiveNode<T, V> LiveField<T, V>(this IInspectorNodeContainer<T> container, string label, Func<T, V> getter, Action<T, V> setter)
		{
			if (string.IsNullOrEmpty(label))
				throw new ArgumentException("A live field needs a label.", nameof(label));
			if (getter == null)
				throw new ArgumentNullException(nameof(getter));
			if (setter == null)
				throw new ArgumentNullException(nameof(setter));
			if (!LiveInput<V>.IsSupported())
				throw new NotSupportedException($"The live field '{label}' is a '{typeof(V).Name}', which the inspector has no field for. Supported: int, float, double, bool, string, enums, Vector2, Vector3, Color and Unity objects.");
			return container.Add(new LiveNode<T, V>
			{
				Label = label,
				Getter = target => getter((T)target),
				Setter = (target, value) => setter((T)target, value),
			});
		}

		/// <summary>Lists a collection that is not serialized, read-only, in a foldout that shows the element count.</summary>
		public static LiveListNode<T> LiveList<T, E>(this IInspectorNodeContainer<T> container, string label, Func<T, IEnumerable<E>> getter)
		{
			if (string.IsNullOrEmpty(label))
				throw new ArgumentException("A live list needs a label.", nameof(label));
			if (getter == null)
				throw new ArgumentNullException(nameof(getter));
			return container.Add(new LiveListNode<T>
			{
				Label = label,
				Getter = target => Enumerate(getter((T)target)),
			});
		}

		/// <summary>Lists a dictionary that is not serialized, read-only, as keys and values.</summary>
		public static LiveListNode<T> LiveTable<T, K, V>(this IInspectorNodeContainer<T> container, string label, Func<T, IEnumerable<KeyValuePair<K, V>>> getter)
		{
			if (string.IsNullOrEmpty(label))
				throw new ArgumentException("A live table needs a label.", nameof(label));
			if (getter == null)
				throw new ArgumentNullException(nameof(getter));
			return container.Add(new LiveListNode<T>
			{
				Label = label,
				IsTable = true,
				Getter = target => EnumeratePairs(getter((T)target)),
			});
		}

		private static IEnumerable<(object, object)> Enumerate<E>(IEnumerable<E> elements)
		{
			if (elements == null)
				return null;
			return EnumerateNotNull(elements);
		}

		private static IEnumerable<(object, object)> EnumerateNotNull<E>(IEnumerable<E> elements)
		{
			foreach (var element in elements)
				yield return (null, element);
		}

		private static IEnumerable<(object, object)> EnumeratePairs<K, V>(IEnumerable<KeyValuePair<K, V>> pairs)
		{
			if (pairs == null)
				return null;
			return EnumeratePairsNotNull(pairs);
		}

		private static IEnumerable<(object, object)> EnumeratePairsNotNull<K, V>(IEnumerable<KeyValuePair<K, V>> pairs)
		{
			foreach (var pair in pairs)
				yield return (pair.Key, pair.Value);
		}
	}

}
