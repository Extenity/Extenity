using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// A button that runs an action on the inspected target. With parameters it is a small form: a box with one
	/// field per parameter and the button under them.
	/// </summary>
	public sealed class ButtonNode<T> : InspectorNode<T, ButtonNode<T>>
	{
		public string Label;
		/// <summary>Target and arguments to the target to keep (the same one, or a changed copy of a struct).</summary>
		public Func<object, object[], object> Invoke;
		public (string Name, Type Type)[] Parameters = Array.Empty<(string, Type)>();
		public object[] DefaultArguments;
		public ButtonTone ToneValue;
		public ButtonSize SizeValue = ButtonSize.Normal;
		public bool IsPlayModeOnly;
		public bool RecordsChange = true;
		public float WidthValue;
		public float SpaceBeforeValue;
		public readonly List<Func<object, InspectorContext, bool>> EnabledRules = new List<Func<object, InspectorContext, bool>>();
		public readonly List<Func<object, InspectorContext, bool>> VisibleRules = new List<Func<object, InspectorContext, bool>>();

		public ButtonNode<T> Tone(ButtonTone tone)
		{
			ToneValue = tone;
			return this;
		}

		public ButtonNode<T> Size(ButtonSize size)
		{
			SizeValue = size;
			return this;
		}

		/// <summary>A fixed width in pixels. Without it a button takes the width it is given.</summary>
		public ButtonNode<T> Width(float pixels)
		{
			if (pixels <= 0f)
				throw new ArgumentOutOfRangeException(nameof(pixels));
			WidthValue = pixels;
			return this;
		}

		/// <summary>Empty space above the button, in pixels: sets an action apart from the fields above it.</summary>
		public ButtonNode<T> SpaceBefore(float pixels)
		{
			if (pixels < 0f)
				throw new ArgumentOutOfRangeException(nameof(pixels));
			SpaceBeforeValue = pixels;
			return this;
		}

		public ButtonNode<T> EnabledWhen(Func<T, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			EnabledRules.Add((target, context) => predicate((T)target));
			return this;
		}

		public ButtonNode<T> EnabledWhen(Func<T, InspectorContext, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			EnabledRules.Add((target, context) => predicate((T)target, context));
			return this;
		}

		/// <summary>The button (with its parameter form) is shown only while the predicate holds; with several objects selected, while it holds for any of them.</summary>
		public ButtonNode<T> VisibleWhen(Func<T, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			VisibleRules.Add((target, context) => predicate((T)target));
			return this;
		}

		public ButtonNode<T> VisibleWhen(Func<T, InspectorContext, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			VisibleRules.Add((target, context) => predicate((T)target, context));
			return this;
		}

		/// <summary>The button is disabled outside Play Mode.</summary>
		public ButtonNode<T> PlayModeOnly()
		{
			IsPlayModeOnly = true;
			return this;
		}

		/// <summary>
		/// For an action that changes nothing that is serialized: no undo step is recorded, the object is not
		/// marked dirty, and for a nested value nothing is written back.
		/// </summary>
		public ButtonNode<T> NoUndo()
		{
			RecordsChange = false;
			return this;
		}

		/// <summary>The initial values of the parameter fields, in parameter order.</summary>
		public ButtonNode<T> Defaults(params object[] arguments)
		{
			if (arguments == null || arguments.Length != Parameters.Length)
				throw new ArgumentException($"The button '{Label}' has {Parameters.Length} parameters but {arguments?.Length ?? 0} default values were given.");
			for (var i = 0; i < arguments.Length; i++)
			{
				var isAssignable = arguments[i] == null ? !Parameters[i].Type.IsValueType : Parameters[i].Type.IsInstanceOfType(arguments[i]);
				if (!isAssignable)
					throw new ArgumentException($"The default value of parameter '{Parameters[i].Name}' of the button '{Label}' must be a '{Parameters[i].Type.Name}', not '{arguments[i]?.GetType().Name ?? "null"}'.");
			}
			DefaultArguments = arguments;
			return this;
		}

		public override VisualElement Build(InspectorInstance instance)
		{
			var inputs = new ButtonParameterInput[Parameters.Length];
			void Click()
			{
				var arguments = new object[inputs.Length];
				for (var i = 0; i < inputs.Length; i++)
					arguments[i] = inputs[i].Read();
				instance.Mutate(Label, RecordsChange, target => Invoke(target, arguments));
			}

			var button = ButtonRow.CreateButton(Label, Click, ToneValue, SizeValue);
			VisualElement element;
			if (Parameters.Length == 0)
			{
				element = button;
			}
			else
			{
				// The box has no title: the button under the fields already names the action.
				var form = new BoxGroup();
				form.AddToClassList("ext-button-form");
				for (var i = 0; i < Parameters.Length; i++)
				{
					inputs[i] = ButtonParameterInput.Create(Parameters[i].Name, Parameters[i].Type, DefaultArguments?[i]);
					form.Add(inputs[i].Element);
				}
				var footer = new VisualElement();
				footer.AddToClassList("ext-button-form__footer");
				footer.Add(button);
				form.Add(footer);
				element = form;
			}
			if (SpaceBeforeValue > 0f)
				element.style.marginTop = SpaceBeforeValue;
			if (WidthValue > 0f)
			{
				button.style.width = WidthValue;
				button.style.flexGrow = 0f;
			}

			if (VisibleRules.Count > 0)
			{
				instance.OnEvaluate(pass =>
				{
					var isVisible = pass.Any((target, context) =>
					{
						foreach (var rule in VisibleRules)
						{
							if (!rule(target, context))
								return false;
						}
						return true;
					});
					element.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
				});
			}

			if (EnabledRules.Count > 0 || IsPlayModeOnly)
			{
				void UpdateEnabled(EvaluationPass pass)
				{
					var isEnabled = (!IsPlayModeOnly || pass.Context.IsPlaying) && pass.All((target, context) =>
					{
						foreach (var rule in EnabledRules)
						{
							if (!rule(target, context))
								return false;
						}
						return true;
					});
					button.SetEnabled(isEnabled);
					button.tooltip = IsPlayModeOnly && !pass.Context.IsPlaying ? "Available in Play Mode." : "";
				}
				instance.OnEvaluate(UpdateEnabled);
				// Entering or leaving Play Mode changes no data, so nothing would evaluate the button again.
				if (IsPlayModeOnly)
					instance.OnRefreshLive(UpdateEnabled);
			}
			return element;
		}
	}

	/// <summary>The field for one parameter of a button.</summary>
	internal sealed class ButtonParameterInput
	{
		public VisualElement Element;
		public Func<object> Read;

		public static bool IsSupported(Type type)
		{
			return type == typeof(int) || type == typeof(float) || type == typeof(bool) || type == typeof(string)
			       || type == typeof(Vector2) || type == typeof(Vector3)
			       || type.IsEnum || typeof(UnityEngine.Object).IsAssignableFrom(type);
		}

		public static ButtonParameterInput Create(string name, Type type, object defaultValue)
		{
			var label = UnityEditor.ObjectNames.NicifyVariableName(name);
			if (type == typeof(int))
				return Make(new IntegerField(label), defaultValue);
			if (type == typeof(float))
				return Make(new FloatField(label), defaultValue);
			if (type == typeof(bool))
				return Make(new Toggle(label), defaultValue);
			if (type == typeof(string))
				return Make(new TextField(label), defaultValue ?? "");
			if (type == typeof(Vector2))
				return Make(new Vector2Field(label), defaultValue);
			if (type == typeof(Vector3))
				return Make(new Vector3Field(label), defaultValue);
			if (type.IsEnum)
			{
				var field = new EnumField(label, (Enum)(defaultValue ?? Enum.ToObject(type, 0)));
				field.AddToClassList(BaseField<Enum>.alignedFieldUssClassName);
				return new ButtonParameterInput { Element = field, Read = () => field.value };
			}
			if (typeof(UnityEngine.Object).IsAssignableFrom(type))
			{
				var field = new ObjectField(label) { objectType = type, allowSceneObjects = true, value = (UnityEngine.Object)defaultValue };
				field.AddToClassList(BaseField<UnityEngine.Object>.alignedFieldUssClassName);
				return new ButtonParameterInput { Element = field, Read = () => field.value };
			}
			throw new NotSupportedException($"A button parameter of type '{type.FullName}' is not supported.");
		}

		private static ButtonParameterInput Make<TValue>(BaseField<TValue> field, object defaultValue)
		{
			if (defaultValue != null)
				field.SetValueWithoutNotify((TValue)defaultValue);
			field.AddToClassList(BaseField<TValue>.alignedFieldUssClassName);
			return new ButtonParameterInput { Element = field, Read = () => field.value };
		}
	}

	public static class ButtonFeature
	{
		/// <summary>
		/// A button that runs the action on the target. In the inspector of an object the change is recorded
		/// for undo and the object is marked dirty. For a nested class the changed copy is written back.
		/// A struct needs the <c>ref</c> form, because an action on a copy of a struct changes nothing.
		/// </summary>
		public static ButtonNode<T> Button<T>(this IInspectorNodeContainer<T> container, string label, Action<T> action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));
			ThrowIfStruct<T>(label);
			return container.Add(Create<T>(label, null, (target, arguments) =>
			{
				action((T)target);
				return target;
			}));
		}

		/// <summary>A button whose action can change or replace the target: the form for structs.</summary>
		public static ButtonNode<T> Button<T>(this IInspectorNodeContainer<T> container, string label, RefAction<T> action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));
			return container.Add(Create<T>(label, null, (target, arguments) =>
			{
				var typed = (T)target;
				action(ref typed);
				return typed;
			}));
		}

		/// <summary>A button with one parameter, which gets a field above the button. The field's label is the name of the lambda's parameter.</summary>
		public static ButtonNode<T> Button<T, P1>(this IInspectorNodeContainer<T> container, string label, Action<T, P1> action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));
			ThrowIfStruct<T>(label);
			return container.Add(Create<T>(label, action.Method, (target, arguments) =>
			{
				action((T)target, (P1)arguments[0]);
				return target;
			}, typeof(P1)));
		}

		/// <summary>A button with two parameters.</summary>
		public static ButtonNode<T> Button<T, P1, P2>(this IInspectorNodeContainer<T> container, string label, Action<T, P1, P2> action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));
			ThrowIfStruct<T>(label);
			return container.Add(Create<T>(label, action.Method, (target, arguments) =>
			{
				action((T)target, (P1)arguments[0], (P2)arguments[1]);
				return target;
			}, typeof(P1), typeof(P2)));
		}

		private static void ThrowIfStruct<T>(string label)
		{
			if (typeof(T).IsValueType)
			{
				throw new InvalidOperationException(
					$"The button '{label}' gets '{typeof(T).Name}', a struct, by value, so what it changes would be lost. Use the ref form: (ref {typeof(T).Name} target) => ...");
			}
		}

		private static ButtonNode<T> Create<T>(string label, MethodInfo method, Func<object, object[], object> invoke, params Type[] parameterTypes)
		{
			if (string.IsNullOrEmpty(label))
				throw new ArgumentException("A button needs a label.", nameof(label));

			var parameters = new (string, Type)[parameterTypes.Length];
			if (parameterTypes.Length > 0)
			{
				// The delegate's method ends with the parameters after the target, whatever precedes them
				// (a closure, or nothing).
				var methodParameters = method.GetParameters();
				var offset = methodParameters.Length - parameterTypes.Length;
				for (var i = 0; i < parameterTypes.Length; i++)
				{
					if (!ButtonParameterInput.IsSupported(parameterTypes[i]))
					{
						throw new NotSupportedException(
							$"The button '{label}' has a parameter of type '{parameterTypes[i].Name}', which the inspector has no field for. Supported: int, float, bool, string, enums, Unity objects, Vector2 and Vector3.");
					}
					var name = offset >= 0 ? methodParameters[offset + i].Name : null;
					parameters[i] = (string.IsNullOrEmpty(name) ? "Parameter " + (i + 1) : name, parameterTypes[i]);
				}
			}
			return new ButtonNode<T> { Label = label, Invoke = invoke, Parameters = parameters };
		}
	}

}
