//#define LogCachedTypeGetters
using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

		public static bool IsUnityBaseType(this Type type)
		{
			return
				type == typeof(MonoBehaviour) ||
				type == typeof(ScriptableObject) ||
				type == typeof(Behaviour) ||
				type == typeof(Component) ||
				type == typeof(UnityEngine.Object) ||
				type == typeof(System.Object);
		}

		public static T GetAttribute<T>(this ICustomAttributeProvider customAttributeProvider, bool inherit) where T : Attribute
		{
			if (customAttributeProvider == null)
				return default(T);
			var customAttributes = customAttributeProvider.GetCustomAttributes(typeof(T), inherit);
			if (customAttributes == null || customAttributes.Length == 0)
				return default(T);
			return (T)customAttributes[0];
		}

		public static IEnumerable<T> GetAttributes<T>(this ICustomAttributeProvider customAttributeProvider, bool inherit) where T : Attribute
		{
			if (customAttributeProvider == null)
				return null;
			var customAttributes = customAttributeProvider.GetCustomAttributes(typeof(T), inherit);
			if (customAttributes == null)
				return null;
			return customAttributes.Cast<T>();
		}

		public static bool IsDefined<T>(this ICustomAttributeProvider customAttributeProvider, bool inherit) where T : Attribute
		{
			if (customAttributeProvider == null)
				return false;
			return customAttributeProvider.IsDefined(typeof(T), inherit);
		}

		#region Unity Serialized Fields

		public static bool IsUnitySerialized(this FieldInfo fieldInfo)
		{
			return fieldInfo.IsPublic || fieldInfo.IsDefined<SerializeField>(true);
		}

		public static FieldInfo[] FilterUnitySerializedFields(this FieldInfo[] unfilteredFields)
		{
			if (unfilteredFields != null && unfilteredFields.Length > 0)
			{
				var fieldsList = new List<FieldInfo>(unfilteredFields.Length);
				for (int i = 0; i < unfilteredFields.Length; i++)
				{
					if (unfilteredFields[i].IsUnitySerialized())
					{
						fieldsList.Add(unfilteredFields[i]);
					}
				}
				return fieldsList.ToArray();
			}
			return new FieldInfo[0];
		}

		public static FieldInfo[] FilterUnityNonSerializedFields(this FieldInfo[] unfilteredFields)
		{
			if (unfilteredFields != null && unfilteredFields.Length > 0)
			{
				var fieldsList = new List<FieldInfo>(unfilteredFields.Length);
				for (int i = 0; i < unfilteredFields.Length; i++)
				{
					if (!unfilteredFields[i].IsUnitySerialized())
					{
						fieldsList.Add(unfilteredFields[i]);
					}
				}
				return fieldsList.ToArray();
			}
			return new FieldInfo[0];
		}

		private static KeyValuePair<FieldInfo, Attribute[]>[] FilterUnitySerializedFields(KeyValuePair<FieldInfo, Attribute[]>[] unfilteredFields)
		{
			if (unfilteredFields != null && unfilteredFields.Length > 0)
			{
				var fieldsList = new List<KeyValuePair<FieldInfo, Attribute[]>>(unfilteredFields.Length);
				for (int i = 0; i < unfilteredFields.Length; i++)
				{
					if (unfilteredFields[i].Key.IsUnitySerialized())
					{
						fieldsList.Add(unfilteredFields[i]);
					}
				}
				return fieldsList.ToArray();
			}
			return new KeyValuePair<FieldInfo, Attribute[]>[0];
		}

		private static KeyValuePair<FieldInfo, Attribute[]>[] FilterUnityNonSerializedFields(KeyValuePair<FieldInfo, Attribute[]>[] unfilteredFields)
		{
			if (unfilteredFields != null && unfilteredFields.Length > 0)
			{
				var fieldsList = new List<KeyValuePair<FieldInfo, Attribute[]>>(unfilteredFields.Length);
				for (int i = 0; i < unfilteredFields.Length; i++)
				{
					if (!unfilteredFields[i].Key.IsUnitySerialized())
					{
						fieldsList.Add(unfilteredFields[i]);
					}
				}
				return fieldsList.ToArray();
			}
			return new KeyValuePair<FieldInfo, Attribute[]>[0];
		}

		#endregion

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

		#region Cached Type Getters - General

		[Conditional("LogCachedTypeGetters")]
		private static void LogCTG(string format, params object[] args)
		{
			Debug.LogFormat(format, args);
		}

		#endregion

		#region Cached Type Getters - Public And Private Fields

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

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
				LogCTG("Getting field info from cache for '{0}' with fields ({1}): \n{2}", type, fields.Length, fields.Serialize('\n'));
				return fields;
			}

			const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

			var baseType = type.BaseType;
			if (!IsUnityBaseType(baseType))
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
			LogCTG("Adding field info to cache for '{0}' with fields ({1}): \n{2}", type, fields.Length, fields.Serialize('\n'));
			PublicAndPrivateInstanceFieldsIncludingBaseTypes.Add(type, fields);
			return fields;
		}

		#endregion

		#region Cached Type Getters - Public And Private Fields Of Type

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Key of KeyValuePair: Class type
		/// Value of KeyValuePair: Field type
		/// FieldInfo[]: List of fields with specified field type in specified class.
		/// </summary>
		private static Dictionary<KeyValuePair<Type, Type>, FieldInfo[]> PublicAndPrivateInstanceFieldsOfTypeIncludingBaseTypes = new Dictionary<KeyValuePair<Type, Type>, FieldInfo[]>();

		/// <summary>
		/// Gets public and private instance (non-static) fields with specified field type. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static FieldInfo[] GetPublicAndPrivateInstanceFieldsOfType(this Type type, Type fieldType)
		{
			var key = new KeyValuePair<Type, Type>(type, fieldType);
			FieldInfo[] fields;

			// Try to get it from cache
			if (PublicAndPrivateInstanceFieldsOfTypeIncludingBaseTypes.TryGetValue(key, out fields))
			{
				LogCTG("Getting field info from cache for '{0}' and field type '{1}' with fields ({2}): \n{3}", type, fieldType, fields.Length, fields.Serialize('\n'));
				return fields;
			}

			var allFields = GetPublicAndPrivateInstanceFields(type);
			var fieldsAsList = new List<FieldInfo>();
			for (int i = 0; i < allFields.Length; i++)
			{
				var fieldInfo = allFields[i];
				if (fieldInfo.FieldType == fieldType)
				{
					fieldsAsList.Add(fieldInfo);
				}
			}
			fields = fieldsAsList.ToArray();

			// Add to cache
			LogCTG("Adding field info to cache for '{0}' and field type '{1}' with fields ({2}): \n{3}", type, fieldType, fields.Length, fields.Serialize('\n'));
			PublicAndPrivateInstanceFieldsOfTypeIncludingBaseTypes.Add(key, fields);
			return fields;
		}

		#endregion

		#region Cached Type Getters - Public And Private Fields With/Without Attribute

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

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
				LogCTG("Getting field-with-attributes info from cache for '{0}' with fields ({1}): \n{2}", type, fieldsWithAttributes.Length, fieldsWithAttributes.Serialize(item => item.Key.ToString(), '\n'));
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
			LogCTG("Adding field-with-attributes info to cache for '{0}' with fields ({1}): \n{2}", type, fieldsWithAttributes.Length, fieldsWithAttributes.Serialize(item => item.Key.ToString(), '\n'));
			PublicAndPrivateInstanceFieldsWithAttributeIncludingBaseTypes.Add(thisTypeAndAttributeTypeCombination, fieldsWithAttributes);
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
				LogCTG("Getting field-without-attributes info from cache for '{0}' with fields ({1}): \n{2}", type, fieldsWithoutAttributes.Length, fieldsWithoutAttributes.Serialize(item => item.Key.ToString(), '\n'));
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
			LogCTG("Adding field-without-attributes info to cache for '{0}' with fields ({1}): \n{2}", type, fieldsWithoutAttributes.Length, fieldsWithoutAttributes.Serialize(item => item.Key.ToString(), '\n'));
			PublicAndPrivateInstanceFieldsWithoutAttributeIncludingBaseTypes.Add(thisTypeAndAttributeTypeCombination, fieldsWithoutAttributes);
			return fieldsWithoutAttributes;
		}

		#endregion

		#region Cached Type Getters - Public And Private Properties

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

		private static Dictionary<Type, PropertyInfo[]> PublicAndPrivateInstancePropertiesIncludingBaseTypes = new Dictionary<Type, PropertyInfo[]>();

		/// <summary>
		/// Gets public and private instance (non-static) properties. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private properties in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static PropertyInfo[] GetPublicAndPrivateInstanceProperties(this Type type)
		{
			PropertyInfo[] properties;

			// Try to get it from cache
			if (PublicAndPrivateInstancePropertiesIncludingBaseTypes.TryGetValue(type, out properties))
			{
				LogCTG("Getting property info from cache for '{0}' with properties ({1}): \n{2}", type, properties.Length, properties.Serialize('\n'));
				return properties;
			}

			const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

			var baseType = type.BaseType;
			if (!IsUnityBaseType(baseType))
			{
				var baseProperties = GetPublicAndPrivateInstanceProperties(baseType);
				properties = type.GetProperties(bindingFlags);
				properties = properties.AddRange(baseProperties);
			}
			else
			{
				properties = type.GetProperties(bindingFlags);
			}

			// Add to cache
			LogCTG("Adding property info to cache for '{0}' with properties ({1}): \n{2}", type, properties.Length, properties.Serialize('\n'));
			PublicAndPrivateInstancePropertiesIncludingBaseTypes.Add(type, properties);
			return properties;
		}

		#endregion

		#region Cached Type Getters - Public And Private Properties Of Type

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Key of KeyValuePair: Class type
		/// Value of KeyValuePair: Property type
		/// PropertyInfo[]: List of properties with specified property type in specified class.
		/// </summary>
		private static Dictionary<KeyValuePair<Type, Type>, PropertyInfo[]> PublicAndPrivateInstancePropertiesOfTypeIncludingBaseTypes = new Dictionary<KeyValuePair<Type, Type>, PropertyInfo[]>();

		/// <summary>
		/// Gets public and private instance (non-static) properties with specified property type. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private properties in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static PropertyInfo[] GetPublicAndPrivateInstancePropertiesOfType(this Type type, Type propertyType)
		{
			var key = new KeyValuePair<Type, Type>(type, propertyType);
			PropertyInfo[] properties;

			// Try to get it from cache
			if (PublicAndPrivateInstancePropertiesOfTypeIncludingBaseTypes.TryGetValue(key, out properties))
			{
				LogCTG("Getting property info from cache for '{0}' and property type '{1}' with properties ({2}): \n{3}", type, propertyType, properties.Length, properties.Serialize('\n'));
				return properties;
			}

			var allProperties = GetPublicAndPrivateInstanceProperties(type);
			var propertiesAsList = new List<PropertyInfo>();
			for (int i = 0; i < allProperties.Length; i++)
			{
				var propertyInfo = allProperties[i];
				if (propertyInfo.PropertyType == propertyType)
				{
					propertiesAsList.Add(propertyInfo);
				}
			}
			properties = propertiesAsList.ToArray();

			// Add to cache
			LogCTG("Adding property info to cache for '{0}' and property type '{1}' with properties ({2}): \n{3}", type, propertyType, properties.Length, properties.Serialize('\n'));
			PublicAndPrivateInstancePropertiesOfTypeIncludingBaseTypes.Add(key, properties);
			return properties;
		}

		#endregion

		#region Cached Type Getters - Public And Private Properties With/Without Attribute

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

		private static Dictionary<KeyValuePair<Type, Type>, KeyValuePair<PropertyInfo, Attribute[]>[]> PublicAndPrivateInstancePropertiesWithAttributeIncludingBaseTypes = new Dictionary<KeyValuePair<Type, Type>, KeyValuePair<PropertyInfo, Attribute[]>[]>();
		private static Dictionary<KeyValuePair<Type, Type>, KeyValuePair<PropertyInfo, Attribute[]>[]> PublicAndPrivateInstancePropertiesWithoutAttributeIncludingBaseTypes = new Dictionary<KeyValuePair<Type, Type>, KeyValuePair<PropertyInfo, Attribute[]>[]>();

		/// <summary>
		/// Gets public and private instance (non-static) properties that has the specified attribute. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private properties in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static KeyValuePair<PropertyInfo, Attribute[]>[] GetPublicAndPrivateInstancePropertiesWithAttribute<TAttribute>(this Type type) where TAttribute : Attribute
		{
			KeyValuePair<PropertyInfo, Attribute[]>[] propertiesWithAttributes;

			// Try to get it from cache
			var thisTypeAndAttributeTypeCombination = new KeyValuePair<Type, Type>(type, typeof(TAttribute));
			if (PublicAndPrivateInstancePropertiesWithAttributeIncludingBaseTypes.TryGetValue(thisTypeAndAttributeTypeCombination, out propertiesWithAttributes))
			{
				LogCTG("Getting property-with-attributes info from cache for '{0}' with properties ({1}): \n{2}", type, propertiesWithAttributes.Length, propertiesWithAttributes.Serialize(item => item.Key.ToString(), '\n'));
				return propertiesWithAttributes;
			}

			var list = new List<KeyValuePair<PropertyInfo, Attribute[]>>();
			var properties = GetPublicAndPrivateInstanceProperties(type);
			for (int i = 0; i < properties.Length; i++)
			{
				var propertyInfo = properties[i];
				const bool inherit = false; // Note: I don't have any clue what does it do and I think it would be the reason if something does not work right.
				var attributes = propertyInfo.GetCustomAttributes(typeof(TAttribute), inherit);
				if (attributes.Length > 0)
					list.Add(new KeyValuePair<PropertyInfo, Attribute[]>(propertyInfo, (Attribute[])attributes));
			}
			propertiesWithAttributes = list.ToArray();

			// Add to cache
			LogCTG("Adding property-with-attributes info to cache for '{0}' with properties ({1}): \n{2}", type, properties.Length, propertiesWithAttributes.Serialize(item => item.Key.ToString(), '\n'));
			PublicAndPrivateInstancePropertiesWithAttributeIncludingBaseTypes.Add(thisTypeAndAttributeTypeCombination, propertiesWithAttributes);
			return propertiesWithAttributes;
		}

		/// <summary>
		/// Gets public and private instance (non-static) properties that doesn't have the specified attribute. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private properties in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static KeyValuePair<PropertyInfo, Attribute[]>[] GetPublicAndPrivateInstancePropertiesWithoutAttribute<TAttribute>(this Type type) where TAttribute : Attribute
		{
			KeyValuePair<PropertyInfo, Attribute[]>[] propertiesWithoutAttributes;

			// Try to get it from cache
			var thisTypeAndAttributeTypeCombination = new KeyValuePair<Type, Type>(type, typeof(TAttribute));
			if (PublicAndPrivateInstancePropertiesWithoutAttributeIncludingBaseTypes.TryGetValue(thisTypeAndAttributeTypeCombination, out propertiesWithoutAttributes))
			{
				LogCTG("Getting property-without-attributes info from cache for '{0}' with properties ({1}): \n{2}", type, propertiesWithoutAttributes.Length, propertiesWithoutAttributes.Serialize(item => item.Key.ToString(), '\n'));
				return propertiesWithoutAttributes;
			}

			var list = new List<KeyValuePair<PropertyInfo, Attribute[]>>();
			var properties = GetPublicAndPrivateInstanceProperties(type);
			for (int i = 0; i < properties.Length; i++)
			{
				var propertyInfo = properties[i];
				const bool inherit = false; // Note: I don't have any clue what does it do and I think it would be the reason if something does not work right.
				var attributes = propertyInfo.GetCustomAttributes(typeof(TAttribute), inherit);
				if (attributes.Length == 0)
					list.Add(new KeyValuePair<PropertyInfo, Attribute[]>(propertyInfo, (Attribute[])attributes));
			}
			propertiesWithoutAttributes = list.ToArray();

			// Add to cache
			LogCTG("Adding property-without-attributes info to cache for '{0}' with properties ({1}): \n{2}", type, properties.Length, propertiesWithoutAttributes.Serialize(item => item.Key.ToString(), '\n'));
			PublicAndPrivateInstancePropertiesWithoutAttributeIncludingBaseTypes.Add(thisTypeAndAttributeTypeCombination, propertiesWithoutAttributes);
			return propertiesWithoutAttributes;
		}

		#endregion

		#region Cached Type Getters - Serialized Fields

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

		private static Dictionary<Type, FieldInfo[]> SerializedFieldsIncludingBaseTypes = new Dictionary<Type, FieldInfo[]>();

		/// <summary>
		/// Gets fields that Unity uses in serialization (public instance fields and SerializeField tagged instance fields). Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static FieldInfo[] GetSerializedFields(this Type type)
		{
			FieldInfo[] fields;

			// Try to get it from cache
			if (SerializedFieldsIncludingBaseTypes.TryGetValue(type, out fields))
			{
				LogCTG("Getting Unity-serialized field info from cache for '{0}' with fields ({1}): \n{2}", type, fields.Length, fields.Serialize('\n'));
				return fields;
			}

			var unfilteredFields = GetPublicAndPrivateInstanceFields(type);
			fields = FilterUnitySerializedFields(unfilteredFields);

			// Add to cache
			LogCTG("Adding Unity-serialized field info to cache for '{0}' with fields ({1}): \n{2}", type, fields.Length, fields.Serialize('\n'));
			SerializedFieldsIncludingBaseTypes.Add(type, fields);
			return fields;
		}

		#endregion

		#region Cached Type Getters - Serialized Fields Of Type

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Key of KeyValuePair: Class type
		/// Value of KeyValuePair: Field type
		/// FieldInfo[]: List of fields with specified field type in specified class.
		/// </summary>
		private static Dictionary<KeyValuePair<Type, Type>, FieldInfo[]> SerializedFieldsOfTypeIncludingBaseTypes = new Dictionary<KeyValuePair<Type, Type>, FieldInfo[]>();

		/// <summary>
		/// Gets fields that Unity uses in serialization (public instance fields and SerializeField tagged instance fields) with specified field type. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static FieldInfo[] GetSerializedFieldsOfType(this Type type, Type fieldType)
		{
			var key = new KeyValuePair<Type, Type>(type, fieldType);
			FieldInfo[] fields;

			// Try to get it from cache
			if (SerializedFieldsOfTypeIncludingBaseTypes.TryGetValue(key, out fields))
			{
				LogCTG("Getting Unity-serialized field info from cache for '{0}' and field type '{1}' with fields ({2}): \n{3}", type, fieldType, fields.Length, fields.Serialize('\n'));
				return fields;
			}

			var unfilteredFields = GetPublicAndPrivateInstanceFieldsOfType(type, fieldType);
			fields = FilterUnitySerializedFields(unfilteredFields);

			// Add to cache
			LogCTG("Adding Unity-serialized field info to cache for '{0}' and field type '{1}' with fields ({2}): \n{3}", type, fieldType, fields.Length, fields.Serialize('\n'));
			SerializedFieldsOfTypeIncludingBaseTypes.Add(key, fields);
			return fields;
		}

		#endregion

		#region Cached Type Getters - Serialized Fields With/Without Attribute

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

		private static Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]> SerializedFieldsWithAttributeIncludingBaseTypes = new Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]>();
		private static Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]> SerializedFieldsWithoutAttributeIncludingBaseTypes = new Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]>();

		/// <summary>
		/// Gets fields that Unity uses in serialization (public instance fields and SerializeField tagged instance fields) that has the specified attribute. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static KeyValuePair<FieldInfo, Attribute[]>[] GetSerializedFieldsWithAttribute<TAttribute>(this Type type) where TAttribute : Attribute
		{
			KeyValuePair<FieldInfo, Attribute[]>[] fieldsWithAttributes;

			// Try to get it from cache
			var thisTypeAndAttributeTypeCombination = new KeyValuePair<Type, Type>(type, typeof(TAttribute));
			if (SerializedFieldsWithAttributeIncludingBaseTypes.TryGetValue(thisTypeAndAttributeTypeCombination, out fieldsWithAttributes))
			{
				LogCTG("Getting Unity-serialized field-with-attributes info from cache for '{0}' with fields ({1}): \n{2}", type, fieldsWithAttributes.Length, fieldsWithAttributes.Serialize(item => item.Key.ToString(), '\n'));
				return fieldsWithAttributes;
			}

			var unfilteredFields = GetPublicAndPrivateInstanceFieldsWithAttribute<TAttribute>(type);
			fieldsWithAttributes = FilterUnitySerializedFields(unfilteredFields);

			// Add to cache
			LogCTG("Adding Unity-serialized field-with-attributes info to cache for '{0}' with fields ({1}): \n{2}", type, fieldsWithAttributes.Length, fieldsWithAttributes.Serialize(item => item.Key.ToString(), '\n'));
			SerializedFieldsWithAttributeIncludingBaseTypes.Add(thisTypeAndAttributeTypeCombination, fieldsWithAttributes);
			return fieldsWithAttributes;
		}

		/// <summary>
		/// Gets fields that Unity uses in serialization (public instance fields and SerializeField tagged instance fields) that doesn't have the specified attribute. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static KeyValuePair<FieldInfo, Attribute[]>[] GetSerializedFieldsWithoutAttribute<TAttribute>(this Type type) where TAttribute : Attribute
		{
			KeyValuePair<FieldInfo, Attribute[]>[] fieldsWithoutAttributes;

			// Try to get it from cache
			var thisTypeAndAttributeTypeCombination = new KeyValuePair<Type, Type>(type, typeof(TAttribute));
			if (SerializedFieldsWithoutAttributeIncludingBaseTypes.TryGetValue(thisTypeAndAttributeTypeCombination, out fieldsWithoutAttributes))
			{
				LogCTG("Getting Unity-serialized field-without-attributes info from cache for '{0}' with fields ({1}): \n{2}", type, fieldsWithoutAttributes.Length, fieldsWithoutAttributes.Serialize(item => item.Key.ToString(), '\n'));
				return fieldsWithoutAttributes;
			}

			var unfilteredFields = GetPublicAndPrivateInstanceFieldsWithoutAttribute<TAttribute>(type);
			fieldsWithoutAttributes = FilterUnitySerializedFields(unfilteredFields);

			// Add to cache
			LogCTG("Adding Unity-serialized field-without-attributes info to cache for '{0}' with fields ({1}): \n{2}", type, fieldsWithoutAttributes.Length, fieldsWithoutAttributes.Serialize(item => item.Key.ToString(), '\n'));
			SerializedFieldsWithoutAttributeIncludingBaseTypes.Add(thisTypeAndAttributeTypeCombination, fieldsWithoutAttributes);
			return fieldsWithoutAttributes;
		}

		#endregion

		#region Cached Type Getters - NonSerialized Fields

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

		private static Dictionary<Type, FieldInfo[]> NonSerializedFieldsIncludingBaseTypes = new Dictionary<Type, FieldInfo[]>();

		/// <summary>
		/// Gets fields that Unity does not use in serialization (public instance fields and SerializeField tagged instance fields). Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static FieldInfo[] GetNonSerializedFields(this Type type)
		{
			FieldInfo[] fields;

			// Try to get it from cache
			if (NonSerializedFieldsIncludingBaseTypes.TryGetValue(type, out fields))
			{
				LogCTG("Getting Unity-NonSerialized field info from cache for '{0}' with fields ({1}): \n{2}", type, fields.Length, fields.Serialize('\n'));
				return fields;
			}

			var unfilteredFields = GetPublicAndPrivateInstanceFields(type);
			fields = FilterUnityNonSerializedFields(unfilteredFields);

			// Add to cache
			LogCTG("Adding Unity-NonSerialized field info to cache for '{0}' with fields ({1}): \n{2}", type, fields.Length, fields.Serialize('\n'));
			NonSerializedFieldsIncludingBaseTypes.Add(type, fields);
			return fields;
		}

		#endregion

		#region Cached Type Getters - NonSerialized Fields Of Type

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Key of KeyValuePair: Class type
		/// Value of KeyValuePair: Field type
		/// FieldInfo[]: List of fields with specified field type in specified class.
		/// </summary>
		private static Dictionary<KeyValuePair<Type, Type>, FieldInfo[]> NonSerializedFieldsOfTypeIncludingBaseTypes = new Dictionary<KeyValuePair<Type, Type>, FieldInfo[]>();

		/// <summary>
		/// Gets fields that Unity does not use in serialization (public instance fields and SerializeField tagged instance fields) with specified field type. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static FieldInfo[] GetNonSerializedFieldsOfType(this Type type, Type fieldType)
		{
			var key = new KeyValuePair<Type, Type>(type, fieldType);
			FieldInfo[] fields;

			// Try to get it from cache
			if (NonSerializedFieldsOfTypeIncludingBaseTypes.TryGetValue(key, out fields))
			{
				LogCTG("Getting Unity-NonSerialized field info from cache for '{0}' and field type '{1}' with fields ({2}): \n{3}", type, fieldType, fields.Length, fields.Serialize('\n'));
				return fields;
			}

			var unfilteredFields = GetPublicAndPrivateInstanceFieldsOfType(type, fieldType);
			fields = FilterUnityNonSerializedFields(unfilteredFields);

			// Add to cache
			LogCTG("Adding Unity-NonSerialized field info to cache for '{0}' and field type '{1}' with fields ({2}): \n{3}", type, fieldType, fields.Length, fields.Serialize('\n'));
			NonSerializedFieldsOfTypeIncludingBaseTypes.Add(key, fields);
			return fields;
		}

		#endregion

		#region Cached Type Getters - NonSerialized Fields With/Without Attribute

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

		private static Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]> NonSerializedFieldsWithAttributeIncludingBaseTypes = new Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]>();
		private static Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]> NonSerializedFieldsWithoutAttributeIncludingBaseTypes = new Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]>();

		/// <summary>
		/// Gets fields that Unity does not use in serialization (public instance fields and SerializeField tagged instance fields) that has the specified attribute. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static KeyValuePair<FieldInfo, Attribute[]>[] GetNonSerializedFieldsWithAttribute<TAttribute>(this Type type) where TAttribute : Attribute
		{
			KeyValuePair<FieldInfo, Attribute[]>[] fieldsWithAttributes;

			// Try to get it from cache
			var thisTypeAndAttributeTypeCombination = new KeyValuePair<Type, Type>(type, typeof(TAttribute));
			if (NonSerializedFieldsWithAttributeIncludingBaseTypes.TryGetValue(thisTypeAndAttributeTypeCombination, out fieldsWithAttributes))
			{
				LogCTG("Getting Unity-NonSerialized field-with-attributes info from cache for '{0}' with fields ({1}): \n{2}", type, fieldsWithAttributes.Length, fieldsWithAttributes.Serialize(item => item.Key.ToString(), '\n'));
				return fieldsWithAttributes;
			}

			var unfilteredFields = GetPublicAndPrivateInstanceFieldsWithAttribute<TAttribute>(type);
			fieldsWithAttributes = FilterUnityNonSerializedFields(unfilteredFields);

			// Add to cache
			LogCTG("Adding Unity-NonSerialized field-with-attributes info to cache for '{0}' with fields ({1}): \n{2}", type, fieldsWithAttributes.Length, fieldsWithAttributes.Serialize(item => item.Key.ToString(), '\n'));
			NonSerializedFieldsWithAttributeIncludingBaseTypes.Add(thisTypeAndAttributeTypeCombination, fieldsWithAttributes);
			return fieldsWithAttributes;
		}

		/// <summary>
		/// Gets fields that Unity does not use in serialization (public instance fields and SerializeField tagged instance fields) that doesn't have the specified attribute. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static KeyValuePair<FieldInfo, Attribute[]>[] GetNonSerializedFieldsWithoutAttribute<TAttribute>(this Type type) where TAttribute : Attribute
		{
			KeyValuePair<FieldInfo, Attribute[]>[] fieldsWithoutAttributes;

			// Try to get it from cache
			var thisTypeAndAttributeTypeCombination = new KeyValuePair<Type, Type>(type, typeof(TAttribute));
			if (NonSerializedFieldsWithoutAttributeIncludingBaseTypes.TryGetValue(thisTypeAndAttributeTypeCombination, out fieldsWithoutAttributes))
			{
				LogCTG("Getting Unity-NonSerialized field-without-attributes info from cache for '{0}' with fields ({1}): \n{2}", type, fieldsWithoutAttributes.Length, fieldsWithoutAttributes.Serialize(item => item.Key.ToString(), '\n'));
				return fieldsWithoutAttributes;
			}

			var unfilteredFields = GetPublicAndPrivateInstanceFieldsWithoutAttribute<TAttribute>(type);
			fieldsWithoutAttributes = FilterUnityNonSerializedFields(unfilteredFields);

			// Add to cache
			LogCTG("Adding Unity-NonSerialized field-without-attributes info to cache for '{0}' with fields ({1}): \n{2}", type, fieldsWithoutAttributes.Length, fieldsWithoutAttributes.Serialize(item => item.Key.ToString(), '\n'));
			NonSerializedFieldsWithoutAttributeIncludingBaseTypes.Add(thisTypeAndAttributeTypeCombination, fieldsWithoutAttributes);
			return fieldsWithoutAttributes;
		}

		#endregion
	}

}
