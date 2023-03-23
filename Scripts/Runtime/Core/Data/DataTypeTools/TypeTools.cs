//#define LogCachedTypeGetters
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
#if UNITY
using UnityEngine;
using UnityEngine.Events;
#endif

namespace Extenity.DataToolbox
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

#if UNITY
		public static bool IsUnityBaseType(this Type type)
		{
			return
				type == typeof(UnityEngine.MonoBehaviour) ||
				type == typeof(UnityEngine.ScriptableObject) ||
				type == typeof(UnityEngine.Behaviour) ||
				type == typeof(UnityEngine.Component) ||
				type == typeof(UnityEngine.Object) ||
				type == typeof(System.Object);
		}
#endif

		public static bool HasAttribute<T>(this Type type)
		{
			return Attribute.GetCustomAttribute(type, typeof(T)) != null;
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

		/// <summary>
		/// Source: https://stackoverflow.com/questions/2742276/how-do-i-check-if-a-type-is-a-subtype-or-the-type-of-an-object
		/// </summary>
		public static bool IsSameOrSubclassOf(this Type derivedClass, Type baseClass)
		{
			return derivedClass.IsSubclassOf(baseClass) || derivedClass == baseClass;
		}

		/// <summary>
		/// Source: https://stackoverflow.com/questions/457676/check-if-a-class-is-derived-from-a-generic-class
		/// </summary>
		public static bool IsSubclassOfRawGeneric(this Type derived, Type generic)
		{
			while (derived != null && derived != typeof(object))
			{
				var cur = derived.IsGenericType ? derived.GetGenericTypeDefinition() : derived;
				if (generic == cur)
				{
					return true;
				}
				derived = derived.BaseType;
			}
			return false;
		}

		/// <summary>
		/// Source: https://stackoverflow.com/questions/457676/check-if-a-class-is-derived-from-a-generic-class
		/// </summary>
		public static Type GetRawGenericSubclass(this Type derived, Type generic)
		{
			while (derived != null && derived != typeof(object))
			{
				var cur = derived.IsGenericType ? derived.GetGenericTypeDefinition() : derived;
				if (generic == cur)
				{
					return derived;
				}
				derived = derived.BaseType;
			}
			return null;
		}

		public static bool IsNumberType(this object value)
		{
			var type = value as Type;
			if (type != null)
			{
				return type == typeof(sbyte)
					|| type == typeof(byte)
					|| type == typeof(short)
					|| type == typeof(ushort)
					|| type == typeof(int)
					|| type == typeof(uint)
					|| type == typeof(long)
					|| type == typeof(ulong)
					|| type == typeof(float)
					|| type == typeof(double)
					|| type == typeof(decimal);
			}
			return value is sbyte
				   || value is byte
				   || value is short
				   || value is ushort
				   || value is int
				   || value is uint
				   || value is long
				   || value is ulong
				   || value is float
				   || value is double
				   || value is decimal;
		}

		public static bool IsPrimitiveType(this object value)
		{
			var type = value as Type;
			if (type != null)
			{
				return type == typeof(sbyte)
					   || type == typeof(byte)
					   || type == typeof(short)
					   || type == typeof(ushort)
					   || type == typeof(int)
					   || type == typeof(uint)
					   || type == typeof(long)
					   || type == typeof(ulong)
					   || type == typeof(float)
					   || type == typeof(double)
					   || type == typeof(decimal)
					   || type == typeof(bool)
					   || type == typeof(char)
					   || type == typeof(string);
			}
			return value is sbyte
				   || value is byte
				   || value is short
				   || value is ushort
				   || value is int
				   || value is uint
				   || value is long
				   || value is ulong
				   || value is float
				   || value is double
				   || value is decimal
				   || value is bool
				   || value is char
				   || value is string;
		}

		/// <summary>
		/// Source: https://stackoverflow.com/questions/1827425/how-to-check-programmatically-if-a-type-is-a-struct-or-a-class
		/// </summary>
		public static bool IsStruct(this Type type)
		{
			// Note that Nullable types are also structs and should be implemented if required. See the source link for more.

			// Enums and Primitives are also Value Types so exclude them.
			return type.IsValueType && !type.IsEnum && !type.IsPrimitiveType();
		}

		public static bool IsClassOrStruct(this Type type)
		{
			return type.IsClass || type.IsStruct();
		}

		public static bool IsGenericList(this object value)
		{
			var type = value as Type;
			if (type == null)
				type = value.GetType();
			return type.GetTypeInfo().IsGenericType &&
				   type.GetGenericTypeDefinition() == typeof(List<>);
		}

		public static bool IsDictionary(this object value)
		{
			var type = value as Type;
			if (type == null)
				type = value.GetType();
			return type.GetTypeInfo().IsGenericType &&
				   type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
		}

		public static bool IsIndexer(this PropertyInfo propertyInfo)
		{
			return propertyInfo.GetIndexParameters().Length > 0;
		}

		public static bool IsReadOnly(this FieldInfo fieldInfo)
		{
			return fieldInfo.IsInitOnly;
		}

		public static bool IsConst(this FieldInfo fieldInfo)
		{
			// Source: https://stackoverflow.com/questions/10261824/how-can-i-get-all-constants-of-a-type-by-reflection
			// IsLiteral determines if its value is written at compile time and not changeable.
			// IsInitOnly determines if the field can be set in the body of the constructor.
			// For C# a field which is readonly keyword would have both true but a const field would have only IsLiteral equal to true.
			return fieldInfo.IsLiteral && !fieldInfo.IsInitOnly;
		}

		public static bool IsStaticReadOnlyValueType(this FieldInfo fieldInfo)
		{
			return fieldInfo.IsStatic && fieldInfo.IsInitOnly && fieldInfo.FieldType.IsValueType;
		}

		/// <summary>
		/// Tells if the value of the field is not expected to change in any way.
		/// Note that reference types may get modified even if they are marked as static readonly.
		/// </summary>
		public static bool IsConstOrStaticReadOnlyValueType(this FieldInfo fieldInfo)
		{
			return fieldInfo.IsConst() || fieldInfo.IsStaticReadOnlyValueType();
		}

		#region Unity Serialized Fields

#if UNITY

		public static bool IsUnitySerialized(this FieldInfo fieldInfo)
		{
			return fieldInfo.IsPublic || fieldInfo.IsDefined<SerializeField>(true);
		}

		public static bool IsUnityNonSerialized(this FieldInfo fieldInfo)
		{
			return (!fieldInfo.IsPublic && !fieldInfo.IsDefined<SerializeField>(true)) || fieldInfo.IsDefined<NonSerializedAttribute>(true);
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
			return Array.Empty<FieldInfo>();
		}

		public static FieldInfo[] FilterUnityNonSerializedFields(this FieldInfo[] unfilteredFields)
		{
			if (unfilteredFields != null && unfilteredFields.Length > 0)
			{
				var fieldsList = new List<FieldInfo>(unfilteredFields.Length);
				for (int i = 0; i < unfilteredFields.Length; i++)
				{
					if (unfilteredFields[i].IsUnityNonSerialized())
					{
						fieldsList.Add(unfilteredFields[i]);
					}
				}
				return fieldsList.ToArray();
			}
			return Array.Empty<FieldInfo>();
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
			return Array.Empty<KeyValuePair<FieldInfo, Attribute[]>>();
		}

		private static KeyValuePair<FieldInfo, Attribute[]>[] FilterUnityNonSerializedFields(KeyValuePair<FieldInfo, Attribute[]>[] unfilteredFields)
		{
			if (unfilteredFields != null && unfilteredFields.Length > 0)
			{
				var fieldsList = new List<KeyValuePair<FieldInfo, Attribute[]>>(unfilteredFields.Length);
				for (int i = 0; i < unfilteredFields.Length; i++)
				{
					if (unfilteredFields[i].Key.IsUnityNonSerialized())
					{
						fieldsList.Add(unfilteredFields[i]);
					}
				}
				return fieldsList.ToArray();
			}
			return Array.Empty<KeyValuePair<FieldInfo, Attribute[]>>();
		}

#endif

		#endregion

		#region Component Helpers

#if UNITY

		public static List<FieldInfo> GetNotAssignedSerializedComponentFields(this Component component)
		{
			if (!component)
			{
				throw new ArgumentNullException(nameof(component));
			}
			return component.GetType().GetSerializedFields().Where(
				field => !field.FieldType.IsValueType
						 && field.FieldType.IsSubclassOf(typeof(Component))
						 && !field.FieldType.IsSubclassOf(typeof(UnityEventBase))
						 && !field.FieldType.IsSubclassOf(typeof(IEnumerable))
						 && (Component)field.GetValue(component) == null // Converting to Component is required so Unity can run internal == operator on Components.
			).ToList();
		}

#endif

		#endregion

		#region Pretty Type Names

		public static readonly Dictionary<Type, string> PrettyTypeNames = new Dictionary<Type, string>
		{
			{ typeof(Single), "Float" },
			{ typeof(Double), "Double" },
			{ typeof(Int16), "Short" },
			{ typeof(Int32), "Int" },
			{ typeof(Int64), "Long" },
			{ typeof(UInt16), "UShort" },
			{ typeof(UInt32), "UInt" },
			{ typeof(UInt64), "ULong" },
			{ typeof(Boolean), "Bool" },
			{ typeof(Byte), "Byte" },
			{ typeof(SByte), "SByte" },
#if UNITY
			{ typeof(UnityEngine.Vector2), "Vector2" },
			{ typeof(UnityEngine.Vector3), "Vector3" },
			{ typeof(UnityEngine.Vector4), "Vector4" },
			{ typeof(UnityEngine.Quaternion), "Quaternion" },
			{ typeof(UnityEngine.Matrix4x4), "Matrix4x4" },
#endif
		};

		public static string GetPrettyName(this Type type)
		{
			if (type == null)
			{
				return "[Unknown]";
			}
			if (PrettyTypeNames.TryGetValue(type, out var name))
			{
				return name;
			}
			return type.Name;
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
				if (DefaultValues.TryGetValue(type, out var defaultValue))
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
			Log.With("CachedTypeGetter").Info(string.Format(format, args));
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
			// Try to get it from cache
			if (PublicAndPrivateInstanceFieldsIncludingBaseTypes.TryGetValue(type, out var fields))
			{
				LogCTG("Getting field info from cache for '{0}' with fields ({1}): \n{2}", type, fields.Length, fields.Serialize('\n'));
				return fields;
			}

			const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

			var baseType = type.BaseType;
#if UNITY
			if (IsUnityBaseType(baseType))
			{
                fields = type.GetFields(bindingFlags);
			}
			else
#endif
            {
                var baseFields = GetPublicAndPrivateInstanceFields(baseType);
                fields = type.GetFields(bindingFlags);
                fields.AddRange(baseFields, out fields);
            }

			// Add to cache
			LogCTG("Adding field info to cache for '{0}' with fields ({1}): \n{2}", type, fields.Length, fields.Serialize('\n'));
			PublicAndPrivateInstanceFieldsIncludingBaseTypes.Add(type, fields);
			return fields;
		}

		#endregion

		#region Cached Type Getters - Public And Private Fields By Name

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Key of KeyValuePair: Class type
		/// Value of KeyValuePair: Field name
		/// FieldInfo[]: List of fields with specified field type in specified class.
		/// </summary>
		private static Dictionary<KeyValuePair<Type, string>, FieldInfo> PublicAndPrivateInstanceFieldsByNameIncludingBaseTypes = new Dictionary<KeyValuePair<Type, string>, FieldInfo>();

		/// <summary>
		/// Gets public and private instance (non-static) fields with specified field name. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static FieldInfo GetPublicAndPrivateInstanceFieldByName(this Type type, string fieldName)
		{
			var key = new KeyValuePair<Type, string>(type, fieldName);

			// Try to get it from cache
			if (PublicAndPrivateInstanceFieldsByNameIncludingBaseTypes.TryGetValue(key, out var field))
			{
				LogCTG("Getting field info from cache for '{0}' and field name '{1}' with field '{2}'", type, fieldName, field.ToString());
				return field;
			}

			var allFields = GetPublicAndPrivateInstanceFields(type);
			for (int i = 0; i < allFields.Length; i++)
			{
				var fieldInfo = allFields[i];
				if (fieldInfo.Name == fieldName)
				{
					field = fieldInfo;
					break;
				}
			}

			// Add to cache
			LogCTG("Adding field info to cache for '{0}' and field name '{1}' with field '{2}'", type, fieldName, field.ToString());
			PublicAndPrivateInstanceFieldsByNameIncludingBaseTypes.Add(key, field);
			return field;
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

			// Try to get it from cache
			if (PublicAndPrivateInstanceFieldsOfTypeIncludingBaseTypes.TryGetValue(key, out var fields))
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
			// Try to get it from cache
			var thisTypeAndAttributeTypeCombination = new KeyValuePair<Type, Type>(type, typeof(TAttribute));
			if (PublicAndPrivateInstanceFieldsWithAttributeIncludingBaseTypes.TryGetValue(thisTypeAndAttributeTypeCombination, out var fieldsWithAttributes))
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
			// Try to get it from cache
			var thisTypeAndAttributeTypeCombination = new KeyValuePair<Type, Type>(type, typeof(TAttribute));
			if (PublicAndPrivateInstanceFieldsWithoutAttributeIncludingBaseTypes.TryGetValue(thisTypeAndAttributeTypeCombination, out var fieldsWithoutAttributes))
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
			// Try to get it from cache
			if (PublicAndPrivateInstancePropertiesIncludingBaseTypes.TryGetValue(type, out var properties))
			{
				LogCTG("Getting property info from cache for '{0}' with properties ({1}): \n{2}", type, properties.Length, properties.Serialize('\n'));
				return properties;
			}

			const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

			var baseType = type.BaseType;
#if UNITY
			if (IsUnityBaseType(baseType))
			{
                properties = type.GetProperties(bindingFlags);
			}
			else
#endif
			{
				var baseProperties = GetPublicAndPrivateInstanceProperties(baseType);
                properties = type.GetProperties(bindingFlags);
                properties.AddRange(baseProperties, out properties);
			}

			// Add to cache
			LogCTG("Adding property info to cache for '{0}' with properties ({1}): \n{2}", type, properties.Length, properties.Serialize('\n'));
			PublicAndPrivateInstancePropertiesIncludingBaseTypes.Add(type, properties);
			return properties;
		}

		#endregion

		#region Cached Type Getters - Public And Private Properties By Name

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

		/// <summary>
		/// Key of KeyValuePair: Class type
		/// Value of KeyValuePair: Property name
		/// PropertyInfo[]: List of properties with specified property type in specified class.
		/// </summary>
		private static Dictionary<KeyValuePair<Type, string>, PropertyInfo> PublicAndPrivateInstancePropertiesByNameIncludingBaseTypes = new Dictionary<KeyValuePair<Type, string>, PropertyInfo>();

		/// <summary>
		/// Gets public and private instance (non-static) properties with specified property name. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private properties in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static PropertyInfo GetPublicAndPrivateInstancePropertyByName(this Type type, string propertyName)
		{
			var key = new KeyValuePair<Type, string>(type, propertyName);

			// Try to get it from cache
			if (PublicAndPrivateInstancePropertiesByNameIncludingBaseTypes.TryGetValue(key, out var property))
			{
				LogCTG("Getting property info from cache for '{0}' and property name '{1}' with property '{2}'", type, propertyName, property.ToString());
				return property;
			}

			var allProperties = GetPublicAndPrivateInstanceProperties(type);
			for (int i = 0; i < allProperties.Length; i++)
			{
				var propertyInfo = allProperties[i];
				if (propertyInfo.Name == propertyName)
				{
					property = propertyInfo;
				}
			}

			// Add to cache
			LogCTG("Adding property info to cache for '{0}' and property name '{1}' with property '{2}'", type, propertyName, property.ToString());
			PublicAndPrivateInstancePropertiesByNameIncludingBaseTypes.Add(key, property);
			return property;
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

			// Try to get it from cache
			if (PublicAndPrivateInstancePropertiesOfTypeIncludingBaseTypes.TryGetValue(key, out var properties))
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
			// Try to get it from cache
			var thisTypeAndAttributeTypeCombination = new KeyValuePair<Type, Type>(type, typeof(TAttribute));
			if (PublicAndPrivateInstancePropertiesWithAttributeIncludingBaseTypes.TryGetValue(thisTypeAndAttributeTypeCombination, out var propertiesWithAttributes))
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
			// Try to get it from cache
			var thisTypeAndAttributeTypeCombination = new KeyValuePair<Type, Type>(type, typeof(TAttribute));
			if (PublicAndPrivateInstancePropertiesWithoutAttributeIncludingBaseTypes.TryGetValue(thisTypeAndAttributeTypeCombination, out var propertiesWithoutAttributes))
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

#if UNITY

		private static Dictionary<Type, FieldInfo[]> SerializedFieldsIncludingBaseTypes = new Dictionary<Type, FieldInfo[]>();

		/// <summary>
		/// Gets fields that Unity uses in serialization (public instance fields and SerializeField tagged instance fields). Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static FieldInfo[] GetSerializedFields(this Type type)
		{
			// Try to get it from cache
			if (SerializedFieldsIncludingBaseTypes.TryGetValue(type, out var fields))
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

#endif

		#endregion

		#region Cached Type Getters - Serialized Fields By Name

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

#if UNITY

		/// <summary>
		/// Key of KeyValuePair: Class type
		/// Value of KeyValuePair: Field name
		/// FieldInfo[]: List of fields with specified field type in specified class.
		/// </summary>
		private static Dictionary<KeyValuePair<Type, string>, FieldInfo> SerializedFieldsByNameIncludingBaseTypes = new Dictionary<KeyValuePair<Type, string>, FieldInfo>();

		/// <summary>
		/// Gets fields that Unity uses in serialization (public instance fields and SerializeField tagged instance fields) with specified field name. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static FieldInfo GetSerializedFieldByName(this Type type, string fieldName)
		{
			var key = new KeyValuePair<Type, string>(type, fieldName);

			// Try to get it from cache
			if (SerializedFieldsByNameIncludingBaseTypes.TryGetValue(key, out var field))
			{
				LogCTG("Getting Unity-serialized field info from cache for '{0}' and field name '{1}' with field '{2}'", type, fieldName, field.ToString());
				return field;
			}

			var unfilteredField = GetPublicAndPrivateInstanceFieldByName(type, fieldName);
			field = unfilteredField.IsUnitySerialized() ? unfilteredField : null;

			// Add to cache
			LogCTG("Adding Unity-serialized field info to cache for '{0}' and field name '{1}' with field '{2}'", type, fieldName, field.ToString());
			SerializedFieldsByNameIncludingBaseTypes.Add(key, field);
			return field;
		}

#endif

		#endregion

		#region Cached Type Getters - Serialized Fields Of Type

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

#if UNITY

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

			// Try to get it from cache
			if (SerializedFieldsOfTypeIncludingBaseTypes.TryGetValue(key, out var fields))
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

#endif

		#endregion

		#region Cached Type Getters - Serialized Fields With/Without Attribute

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

#if UNITY

		private static Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]> SerializedFieldsWithAttributeIncludingBaseTypes = new Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]>();
		private static Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]> SerializedFieldsWithoutAttributeIncludingBaseTypes = new Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]>();

		/// <summary>
		/// Gets fields that Unity uses in serialization (public instance fields and SerializeField tagged instance fields) that has the specified attribute. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static KeyValuePair<FieldInfo, Attribute[]>[] GetSerializedFieldsWithAttribute<TAttribute>(this Type type) where TAttribute : Attribute
		{
			// Try to get it from cache
			var thisTypeAndAttributeTypeCombination = new KeyValuePair<Type, Type>(type, typeof(TAttribute));
			if (SerializedFieldsWithAttributeIncludingBaseTypes.TryGetValue(thisTypeAndAttributeTypeCombination, out var fieldsWithAttributes))
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
			// Try to get it from cache
			var thisTypeAndAttributeTypeCombination = new KeyValuePair<Type, Type>(type, typeof(TAttribute));
			if (SerializedFieldsWithoutAttributeIncludingBaseTypes.TryGetValue(thisTypeAndAttributeTypeCombination, out var fieldsWithoutAttributes))
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

#endif

		#endregion

		#region Cached Type Getters - NonSerialized Fields

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

#if UNITY

		private static Dictionary<Type, FieldInfo[]> NonSerializedFieldsIncludingBaseTypes = new Dictionary<Type, FieldInfo[]>();

		/// <summary>
		/// Gets fields that Unity does not use in serialization (public instance fields and SerializeField tagged instance fields). Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static FieldInfo[] GetNonSerializedFields(this Type type)
		{
			// Try to get it from cache
			if (NonSerializedFieldsIncludingBaseTypes.TryGetValue(type, out var fields))
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

#endif

		#endregion

		#region Cached Type Getters - NonSerialized Fields By Name

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

#if UNITY

		/// <summary>
		/// Key of KeyValuePair: Class type
		/// Value of KeyValuePair: Field name
		/// FieldInfo[]: List of fields with specified field type in specified class.
		/// </summary>
		private static Dictionary<KeyValuePair<Type, string>, FieldInfo> NonSerializedFieldsByNameIncludingBaseTypes = new Dictionary<KeyValuePair<Type, string>, FieldInfo>();

		/// <summary>
		/// Gets fields that Unity does not use in serialization (public instance fields and SerializeField tagged instance fields) with specified field name. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static FieldInfo GetNonSerializedFieldByName(this Type type, string fieldName)
		{
			var key = new KeyValuePair<Type, string>(type, fieldName);

			// Try to get it from cache
			if (NonSerializedFieldsByNameIncludingBaseTypes.TryGetValue(key, out var field))
			{
				LogCTG("Getting Unity-NonSerialized field info from cache for '{0}' and field name '{1}' with field '{2}'", type, fieldName, field.ToString());
				return field;
			}

			var unfilteredField = GetPublicAndPrivateInstanceFieldByName(type, fieldName);
			field = unfilteredField.IsUnityNonSerialized() ? unfilteredField : null;

			// Add to cache
			LogCTG("Adding Unity-NonSerialized field info to cache for '{0}' and field name '{1}' with field '{2}'", type, fieldName, field.ToString());
			NonSerializedFieldsByNameIncludingBaseTypes.Add(key, field);
			return field;
		}

#endif

		#endregion

		#region Cached Type Getters - NonSerialized Fields Of Type

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

#if UNITY

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

			// Try to get it from cache
			if (NonSerializedFieldsOfTypeIncludingBaseTypes.TryGetValue(key, out var fields))
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

#endif

		#endregion

		#region Cached Type Getters - NonSerialized Fields With/Without Attribute

		// --------------------------------------------------------------------------------------------------------
		// ---- NOTE: These codes are duplicated between Cached Type Getters.
		// ---- Make sure you modify them all if anything changes here
		// --------------------------------------------------------------------------------------------------------

#if UNITY

		private static Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]> NonSerializedFieldsWithAttributeIncludingBaseTypes = new Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]>();
		private static Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]> NonSerializedFieldsWithoutAttributeIncludingBaseTypes = new Dictionary<KeyValuePair<Type, Type>, KeyValuePair<FieldInfo, Attribute[]>[]>();

		/// <summary>
		/// Gets fields that Unity does not use in serialization (public instance fields and SerializeField tagged instance fields) that has the specified attribute. Caches the results for faster accesses.
		/// 
		/// This method is developed around the fact that Reflection does not get private fields in base classes, which is stated here: http://stackoverflow.com/a/5911164
		/// </summary>
		public static KeyValuePair<FieldInfo, Attribute[]>[] GetNonSerializedFieldsWithAttribute<TAttribute>(this Type type) where TAttribute : Attribute
		{
			// Try to get it from cache
			var thisTypeAndAttributeTypeCombination = new KeyValuePair<Type, Type>(type, typeof(TAttribute));
			if (NonSerializedFieldsWithAttributeIncludingBaseTypes.TryGetValue(thisTypeAndAttributeTypeCombination, out var fieldsWithAttributes))
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
			// Try to get it from cache
			var thisTypeAndAttributeTypeCombination = new KeyValuePair<Type, Type>(type, typeof(TAttribute));
			if (NonSerializedFieldsWithoutAttributeIncludingBaseTypes.TryGetValue(thisTypeAndAttributeTypeCombination, out var fieldsWithoutAttributes))
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

#endif

		#endregion
	}

}
