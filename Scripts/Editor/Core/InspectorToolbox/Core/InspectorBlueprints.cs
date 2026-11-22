using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// An editor or property drawer whose content is declared with the inspector toolbox.
	/// </summary>
	public interface ICustomizedInspector
	{
		Type TargetType { get; }
		InspectorBlueprint GetBlueprint();
	}

	/// <summary>
	/// The blueprints built so far, one per editor or drawer class, and the lookup from a value type to the
	/// property drawer Unity would use for it.
	/// </summary>
	public static class InspectorBlueprints
	{
		private static readonly Dictionary<Type, InspectorBlueprint> ByInspectorType = new Dictionary<Type, InspectorBlueprint>();

		/// <summary>The blueprint of an editor or drawer class, built on first use.</summary>
		public static InspectorBlueprint GetOrBuild(Type inspectorType, Func<InspectorBlueprint> build)
		{
			if (ByInspectorType.TryGetValue(inspectorType, out var blueprint))
				return blueprint;
			blueprint = build() ?? throw new InvalidOperationException($"'{inspectorType.FullName}' built no blueprint.");
			ByInspectorType.Add(inspectorType, blueprint);
			return blueprint;
		}

		/// <summary>
		/// The blueprint of the nested inspector that Unity would draw a value of the given type with, or false
		/// when the type has no property drawer or its drawer is not a nested inspector.
		/// </summary>
		public static bool TryGetForValueType(Type valueType, out InspectorBlueprint blueprint)
		{
			blueprint = null;
			if (!PropertyDrawerLookup.TryGetDrawerType(valueType, out var drawerType))
				return false;
			if (!typeof(ICustomizedInspector).IsAssignableFrom(drawerType))
				return false;
			if (!ByInspectorType.TryGetValue(drawerType, out blueprint))
			{
				// Building the blueprint needs an instance of the drawer, exactly as Unity creates one.
				var drawer = (ICustomizedInspector)Activator.CreateInstance(drawerType);
				blueprint = drawer.GetBlueprint();
			}
			return true;
		}
	}

	/// <summary>
	/// Tells which property drawer class is registered for a value type, following Unity's rule: the drawer
	/// of the type itself, otherwise the drawer of the nearest base type that is registered for children.
	/// </summary>
	public static class PropertyDrawerLookup
	{
		private static Dictionary<Type, (Type DrawerType, bool UseForChildren)> _Registered;

		private static Dictionary<Type, (Type DrawerType, bool UseForChildren)> Registered
		{
			get
			{
				if (_Registered != null)
					return _Registered;

				var typeField = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				                ?? throw new MissingFieldException(nameof(CustomPropertyDrawer), "m_Type");
				var childrenField = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				                    ?? throw new MissingFieldException(nameof(CustomPropertyDrawer), "m_UseForChildren");

				var registered = new Dictionary<Type, (Type, bool)>();
				foreach (var drawerType in TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>())
				{
					if (drawerType.IsAbstract)
						continue;
					foreach (var attribute in drawerType.GetCustomAttributes(typeof(CustomPropertyDrawer), true))
					{
						var targetType = (Type)typeField.GetValue(attribute);
						if (targetType == null)
							continue;
						var useForChildren = (bool)childrenField.GetValue(attribute);
						// When two drawers name the same type, the first one stays. Unity warns about that case itself.
						if (!registered.ContainsKey(targetType))
							registered.Add(targetType, (drawerType, useForChildren));
					}
				}
				_Registered = registered;
				return _Registered;
			}
		}

		public static bool TryGetDrawerType(Type valueType, out Type drawerType)
		{
			if (valueType == null)
				throw new ArgumentNullException(nameof(valueType));

			var registered = Registered;
			if (registered.TryGetValue(valueType, out var exact))
			{
				drawerType = exact.DrawerType;
				return true;
			}
			if (valueType.IsGenericType && registered.TryGetValue(valueType.GetGenericTypeDefinition(), out var generic))
			{
				drawerType = generic.DrawerType;
				return true;
			}
			for (var baseType = valueType.BaseType; baseType != null; baseType = baseType.BaseType)
			{
				if (registered.TryGetValue(baseType, out var inherited) && inherited.UseForChildren)
				{
					drawerType = inherited.DrawerType;
					return true;
				}
			}
			foreach (var interfaceType in valueType.GetInterfaces())
			{
				if (registered.TryGetValue(interfaceType, out var inherited) && inherited.UseForChildren)
				{
					drawerType = inherited.DrawerType;
					return true;
				}
			}
			drawerType = null;
			return false;
		}

		public static bool HasDrawer(Type valueType)
		{
			return TryGetDrawerType(valueType, out _);
		}
	}

}
