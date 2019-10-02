using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Extenity.DataToolbox;
using UnityEngine;
using Object = System.Object;

namespace Extenity.DebugToolbox.GraphPlotting
{

	public class TypeInspectors
	{
		#region Configuration

		private const BindingFlags Flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		private static readonly Dictionary<Type, ITypeInspector> Inspectors = new Dictionary<Type, ITypeInspector>
		{
			{ typeof(Quaternion), new QuaternionTypeInspector() }
		};

		#endregion

		public static void AddTypeInspector(Type type, ITypeInspector typeInspector)
		{
			if (Inspectors.ContainsKey(type))
				return; // Ignore.
			Inspectors.Add(type, typeInspector);
		}

		public static ITypeInspector GetTypeInspector(Type type)
		{
			if (!Inspectors.TryGetValue(type, out var inspector))
			{
				inspector = new DefaultTypeInspector(type);
				Inspectors.Add(type, inspector);
			}
			return inspector;
		}

		private class DefaultTypeInspector : ITypeInspector
		{
			private string[] fieldNameStrings;
			private Field[] fields;
			private Dictionary<string, FieldInfo> nameToFieldInfo;

			public DefaultTypeInspector(Type type)
			{
				var fieldInfos = new List<FieldInfo>();

				var currentType = type;
				while (currentType != null && currentType != typeof(UnityEngine.Object))
				{
					fieldInfos.AddRange(currentType.GetFields(Flags).Where(field => TypeInspectors.IsAcceptableType(field.FieldType)));
					currentType = currentType.BaseType;
				}

				fieldNameStrings = new string[fieldInfos.Count];
				fields = new Field[fieldInfos.Count];
				nameToFieldInfo = new Dictionary<string, FieldInfo>();

				for (int i = 0; i < fieldInfos.Count; i++)
				{
					var fieldInfo = fieldInfos[i];

					fieldNameStrings[i] = fieldInfo.Name + " : " + fieldInfo.FieldType.GetPrettyName();

					var field = new Field(fieldInfo.Name, fieldInfo.FieldType);

					nameToFieldInfo.Add(field.name, fieldInfo);

					fields[i] = field;
				}
			}

			public Object GetValue(Object instance, string fieldName)
			{
				if (!nameToFieldInfo.TryGetValue(fieldName, out var fieldInfo))
				{
					return null;
				}

				return fieldInfo.GetValue(instance);
			}

			public string[] FieldNameStrings { get { return fieldNameStrings; } }
			public Field[] Fields { get { return fields; } }
		}

		public class Field
		{
			public string name;
			public Type type;

			public Field(string name, Type type)
			{
				this.name = name;
				this.type = type;
			}
		}

		private class QuaternionTypeInspector : ITypeInspector
		{
			private readonly string[] fieldNameStrings =
			{
				"x (euler) : float",
				"y (euler) : float",
				"z (euler) : float"
			};
			private readonly Field[] fields =
			{
				new Field("x (euler)", typeof(float)),
				new Field("y (euler)", typeof(float)),
				new Field("z (euler)", typeof(float)),
			};

			public string[] FieldNameStrings { get { return fieldNameStrings; } }
			public Field[] Fields { get { return fields; } }

			public Object GetValue(Object instance, string fieldName)
			{
				var quaternion = (Quaternion)instance;
				if (fieldName.Equals("x (euler)", StringComparison.Ordinal))
				{
					return quaternion.eulerAngles.x;
				}
				if (fieldName.Equals("y (euler)", StringComparison.Ordinal))
				{
					return quaternion.eulerAngles.y;
				}
				if (fieldName.Equals("z (euler)", StringComparison.Ordinal))
				{
					return quaternion.eulerAngles.z;
				}
				return null;
			}
		}

		private static readonly HashSet<Type> KnownTypes = new HashSet<Type>
		{
			typeof(Single),
			typeof(Double),
			typeof(Int16),
			typeof(Int32),
			typeof(Int64),
			typeof(UInt16),
			typeof(UInt32),
			typeof(UInt64),
			typeof(Boolean),
			typeof(Byte),
			typeof(SByte),
		};

		public static bool IsKnownType(Type type)
		{
			return KnownTypes.Contains(type);
		}

		public static bool IsAcceptableType(Type type)
		{
			if (IsKnownType(type))
			{
				return true;
			}

			if (type.IsArray)
			{
				return false;
			}

			if (type.Namespace == "UnityEngine" && type.IsSubclassOf(typeof(Component)))
			{
				return false;
			}

			if (type.IsSubclassOf(typeof(Delegate)))
			{
				return false;
			}

			return true;
		}

		public interface ITypeInspector
		{
			Object GetValue(Object instance, string fieldName);
			string[] FieldNameStrings { get; }
			Field[] Fields { get; }
		}
	}

}
