using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Extenity.DataTypes
{

	public static class TypeTools
	{
		public static bool InheritsOrImplements(this Type child, Type parent)
		{
			parent = ResolveGenericTypeDefinition(parent);

			var currentChild = child.IsGenericType
				? child.GetGenericTypeDefinition()
				: child;

			while (currentChild != typeof(object))
			{
				if (parent == currentChild || HasAnyInterfaces(parent, currentChild))
					return true;

				currentChild = currentChild.BaseType != null && currentChild.BaseType.IsGenericType
						? currentChild.BaseType.GetGenericTypeDefinition()
						: currentChild.BaseType;

				if (currentChild == null)
					return false;
			}
			return false;
		}

		private static bool HasAnyInterfaces(Type parent, Type child)
		{
			//return child.GetInterfaces()
			//	.Any(childInterface =>
			//	{
			//		var currentInterface = childInterface.IsGenericType
			//			? childInterface.GetGenericTypeDefinition()
			//			: childInterface;
			//		return currentInterface == parent;
			//	});

			foreach (Type childInterface in child.GetInterfaces())
			{
				var currentInterface = childInterface.IsGenericType
					? childInterface.GetGenericTypeDefinition()
					: childInterface;

				if (currentInterface == parent)
					return true;
			}
			return false;
		}

		private static Type ResolveGenericTypeDefinition(Type parent)
		{
			bool shouldUseGenericType = !(parent.IsGenericType && parent.GetGenericTypeDefinition() != parent);

			if (parent.IsGenericType && shouldUseGenericType)
				parent = parent.GetGenericTypeDefinition();
			return parent;
		}

		public static string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
		{
			MemberExpression me = propertyLambda.Body as MemberExpression;
			if (me == null)
			{
				throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
			}

			return me.Member.Name;
		}

		public static string GetFullPropertyName<T>(Expression<Func<T>> propertyLambda)
		{
			MemberExpression me = propertyLambda.Body as MemberExpression;
			if (me == null)
			{
				throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
			}

			string result = string.Empty;
			do
			{
				result = me.Member.Name + "." + result;
				me = me.Expression as MemberExpression;
			} while (me != null);

			result = result.Remove(result.Length - 1); // remove the trailing "."
			return result;
		}

		#region Get Default Value Of Type

		private static Dictionary<Type, object> DefaultValues = new Dictionary<Type, object>();

		public static object GetDefaultValue(this Type type)
		{
			// Check if this is a 'value type'. Create an instance to get it's default value.
			if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
			{
				// Try to get it from cache
				object defaultValue;
				if (DefaultValues.TryGetValue(type, out defaultValue))
					return defaultValue;

				defaultValue = Activator.CreateInstance(type);
				DefaultValues.Add(type, defaultValue);
				return defaultValue;
			}

			// Otherwise this is a 'reference type' and the default value for reference types is 'null'.
			return null;
		}

		#endregion

		#region Find Derived Types

		public static IEnumerable<Type> FindDerivedTypes(this Assembly assembly, Type baseType)
		{
			return assembly.GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t));
		}

		public static IEnumerable<Type> FindDerivedTypesInAllAssemblies(Type baseType)
		{
			return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.FindDerivedTypes(baseType));
		}

		#endregion

		#region Cached Type Getters - Public And Private Fields

		private static Dictionary<Type, FieldInfo[]> PublicAndPrivateInstanceFieldsIncludingBaseTypes = new Dictionary<Type, FieldInfo[]>();

		/// <summary>
		/// Gets public and private instance (non-static) fields. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static FieldInfo[] GetPublicAndPrivateInstanceFields(this Type type)
		{
			FieldInfo[] fields;

			// Try to get it from cache
			if (PublicAndPrivateInstanceFieldsIncludingBaseTypes.TryGetValue(type, out fields))
			{
				//Debug.LogFormat("Getting field info from cache for '{0}' with types: \n{1}", type, fields.Serialize('\n'));
				return fields;
			}

			const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

			var baseType = type.BaseType;
			if (baseType != typeof(MonoBehaviour) &&
				baseType != typeof(Behaviour) &&
				baseType != typeof(Component) &&
				baseType != typeof(UnityEngine.Object) &&
				baseType != typeof(System.Object))
			{
				var baseFields = GetPublicAndPrivateInstanceFields(baseType);
				fields = type.GetFields(bindingFlags);
				fields = fields.AddRange(baseFields);
			}
			else
			{
				fields = type.GetFields(bindingFlags);
			}

			// Add to cache
			//Debug.LogFormat("Adding field info to cache for '{0}' with types: \n{1}", type, fields.Serialize('\n'));
			PublicAndPrivateInstanceFieldsIncludingBaseTypes.Add(type, fields);
			return fields;
		}

		#endregion

		#region Cached Attribute Getters - Public And Private Fields

		private static Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]> PublicAndPrivateInstanceFieldsWithAttributeIncludingBaseTypes = new Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]>();
		private static Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]> PublicAndPrivateInstanceFieldsWithoutAttributeIncludingBaseTypes = new Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]>();

		/// <summary>
		/// Gets public and private instance (non-static) fields that has the specified attribute. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static KeyValuePair<FieldInfo, Attribute[]>[] GetPublicAndPrivateInstanceFieldsWithAttribute<TAttribute>(this Type type) where TAttribute : Attribute
		{
			KeyValuePair<FieldInfo, Attribute[]>[] fieldsWithAttributes;

			// Try to get it from cache
			var thisTypeAndAttributeTypeCombination = new KeyValuePair<Type, Type>(type, typeof(TAttribute));
			if (PublicAndPrivateInstanceFieldsWithAttributeIncludingBaseTypes.TryGetValue(thisTypeAndAttributeTypeCombination, out fieldsWithAttributes))
			{
				//Debug.LogFormat("Getting field-with-attributes info from cache for '{0}' with types: \n{1}", type, fields.Serialize('\n'));
				return fieldsWithAttributes;
			}

			var list = new List<KeyValuePair<FieldInfo, Attribute[]>>();
			var fields = GetPublicAndPrivateInstanceFields(type);
			for (int i = 0; i < fields.Length; i++)
			{
				var fieldInfo = fields[i];
				const bool inherit = false; // Note: I don't have any clue what does it do and I think it would be the reason if something does not work right.
				var attributes = fieldInfo.GetCustomAttributes(typeof(TAttribute), inherit);
				if (attributes.Length > 0)
					list.Add(new KeyValuePair<FieldInfo, Attribute[]>(fieldInfo, (Attribute[])attributes));
			}
			fieldsWithAttributes = list.ToArray();

			// Add to cache
			//Debug.LogFormat("Adding field-with-attributes info to cache for '{0}' with types: \n{1}", type, fields.Serialize('\n'));
			PublicAndPrivateInstanceFieldsWithAttributeIncludingBaseTypes.Add(
				thisTypeAndAttributeTypeCombination,
				fieldsWithAttributes);

			return fieldsWithAttributes;
		}

		/// <summary>
		/// Gets public and private instance (non-static) fields that doesn't have the specified attribute. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static KeyValuePair<FieldInfo, Attribute[]>[] GetPublicAndPrivateInstanceFieldsWithoutAttribute<TAttribute>(this Type type) where TAttribute : Attribute
		{
			KeyValuePair<FieldInfo, Attribute[]>[] fieldsWithoutAttributes;

			// Try to get it from cache
			var thisTypeAndAttributeTypeCombination = new KeyValuePair<Type, Type>(type, typeof(TAttribute));
			if (PublicAndPrivateInstanceFieldsWithoutAttributeIncludingBaseTypes.TryGetValue(thisTypeAndAttributeTypeCombination, out fieldsWithoutAttributes))
			{
				//Debug.LogFormat("Getting field-without-attributes info from cache for '{0}' with types: \n{1}", type, fields.Serialize('\n'));
				return fieldsWithoutAttributes;
			}

			var list = new List<KeyValuePair<FieldInfo, Attribute[]>>();
			var fields = GetPublicAndPrivateInstanceFields(type);
			for (int i = 0; i < fields.Length; i++)
			{
				var fieldInfo = fields[i];
				const bool inherit = false; // Note: I don't have any clue what does it do and I think it would be the reason if something does not work right.
				var attributes = fieldInfo.GetCustomAttributes(typeof(TAttribute), inherit);
				if (attributes.Length == 0)
					list.Add(new KeyValuePair<FieldInfo, Attribute[]>(fieldInfo, (Attribute[])attributes));
			}
			fieldsWithoutAttributes = list.ToArray();

			// Add to cache
			//Debug.LogFormat("Adding field-without-attributes info to cache for '{0}' with types: \n{1}", type, fields.Serialize('\n'));
			PublicAndPrivateInstanceFieldsWithoutAttributeIncludingBaseTypes.Add(
				thisTypeAndAttributeTypeCombination,
				fieldsWithoutAttributes);

			return fieldsWithoutAttributes;
		}

		#endregion
	}

}
