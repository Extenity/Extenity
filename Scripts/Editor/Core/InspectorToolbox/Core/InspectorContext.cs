using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// What a rule can know about the place it is evaluated in, beyond its typed target.
	/// </summary>
	public sealed class InspectorContext
	{
		public SerializedObject SerializedObject { get; }

		/// <summary>The path of the property the inspector draws. Null for the inspector of the object itself.</summary>
		public string PropertyPath { get; }

		public InspectorContext(SerializedObject serializedObject, string propertyPath)
		{
			SerializedObject = serializedObject ?? throw new ArgumentNullException(nameof(serializedObject));
			PropertyPath = string.IsNullOrEmpty(propertyPath) ? null : propertyPath;
		}

		/// <summary>
		/// The property the inspector draws, looked up again on every call. Null for the inspector of the object
		/// itself, and null when the property no longer exists (a removed list element).
		/// </summary>
		public SerializedProperty Property => PropertyPath == null ? null : SerializedObject.FindProperty(PropertyPath);

		/// <summary>The object that owns the data. With several objects selected, the first one.</summary>
		public Object RootObject => SerializedObject.targetObject;

		/// <summary>The owning object as <typeparamref name="TRoot"/>. Throws when it is of another type.</summary>
		public TRoot Root<TRoot>() where TRoot : Object
		{
			var root = RootObject;
			if (root is TRoot typed)
				return typed;
			throw new InvalidOperationException(
				$"The inspected object is '{(root != null ? root.GetType().FullName : "null")}', not '{typeof(TRoot).FullName}'.");
		}

		/// <summary>The owning object as <typeparamref name="TRoot"/>, or false when it is of another type.</summary>
		public bool TryGetRoot<TRoot>(out TRoot root) where TRoot : Object
		{
			root = RootObject as TRoot;
			return root != null;
		}

		public bool IsPlaying => EditorApplication.isPlaying;
		public bool IsMultiEdit => SerializedObject.isEditingMultipleObjects;
		public bool IsPrefabInstance => RootObject != null && PrefabUtility.IsPartOfPrefabInstance(RootObject);
		public bool IsPersistentAsset => RootObject != null && EditorUtility.IsPersistent(RootObject);
	}

	/// <summary>
	/// The targets of one evaluation: boxed once and shared by every rule, validator and title of an inspector
	/// instance. For the inspector of an object these are the live objects. For a nested inspector they are copies
	/// made from serialized data (the live instance for a managed reference), so they carry no non-serialized state.
	/// </summary>
	public sealed class EvaluationPass
	{
		public InspectorContext Context { get; }
		/// <summary>One entry per inspected object. Never contains null.</summary>
		public object[] Targets { get; }

		public EvaluationPass(InspectorContext context, object[] targets)
		{
			Context = context;
			Targets = targets;
		}

		public bool HasSingleTarget => Targets.Length == 1;

		/// <summary>The target when exactly one object is inspected, otherwise null.</summary>
		public object SingleTarget => Targets.Length == 1 ? Targets[0] : null;

		/// <summary>True when the predicate holds for at least one target.</summary>
		public bool Any(Func<object, InspectorContext, bool> predicate)
		{
			for (var i = 0; i < Targets.Length; i++)
			{
				if (predicate(Targets[i], Context))
					return true;
			}
			return false;
		}

		/// <summary>True when the predicate holds for every target.</summary>
		public bool All(Func<object, InspectorContext, bool> predicate)
		{
			for (var i = 0; i < Targets.Length; i++)
			{
				if (!predicate(Targets[i], Context))
					return false;
			}
			return true;
		}

		/// <summary>The most severe result of the rule over all targets.</summary>
		public ValidationResult Worst(Func<object, InspectorContext, ValidationResult> rule)
		{
			var worst = ValidationResult.Ok;
			for (var i = 0; i < Targets.Length; i++)
			{
				var result = rule(Targets[i], Context);
				if (result.Severity > worst.Severity)
					worst = result;
			}
			return worst;
		}
	}

}
