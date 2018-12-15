using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = System.Object;

namespace Extenity.DebugFlowTool.GraphPlotting
{

	public class TypeInspectors
	{
		#region Singleton

		private static TypeInspectors _Instance = null;
		public static TypeInspectors Instance
		{
			get
			{
				if (_Instance == null)
				{
					_Instance = new TypeInspectors();
				}
				return _Instance;
			}
		}

		#endregion

		#region Configuration

		private const BindingFlags Flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		private readonly Dictionary<Type, string> PrettyTypeNames = new Dictionary<Type, string>
		{
			{ typeof(Single), "float" },
			{ typeof(Double), "double" },
			{ typeof(Int16), "short" },
			{ typeof(Int32), "int" },
			{ typeof(Int64), "long" },
			{ typeof(UInt16), "ushort" },
			{ typeof(UInt32), "uint" },
			{ typeof(UInt64), "ulong" },
			{ typeof(Boolean), "bool" },
			{ typeof(Byte), "byte" },
			{ typeof(SByte), "sbyte" },
		};

		private readonly Dictionary<Type, ITypeInspector> Inspectors = new Dictionary<Type, ITypeInspector>
		{
			{ typeof(Quaternion), new QuaternionTypeInspector() }
		};

		#endregion

		public void AddTypeInspector(Type type, ITypeInspector typeInspector)
		{
			if (Inspectors.ContainsKey(type))
				return; // Ignore.
			Inspectors.Add(type, typeInspector);
		}

		public ITypeInspector GetTypeInspector(Type type)
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
					fieldInfos.AddRange(currentType.GetFields(Flags).Where(field => Instance.IsAcceptableType(field.FieldType)));
					currentType = currentType.BaseType;
				}

				fieldNameStrings = new string[fieldInfos.Count];
				fields = new Field[fieldInfos.Count];
				nameToFieldInfo = new Dictionary<string, FieldInfo>();

				for (int i = 0; i < fieldInfos.Count; i++)
				{
					var fieldInfo = fieldInfos[i];

					fieldNameStrings[i] = fieldInfo.Name + " : " + Instance.GetPrettyName(fieldInfo.FieldType);

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
				if (fieldName == "x (euler)")
				{
					return quaternion.eulerAngles.x;
				}
				if (fieldName == "y (euler)")
				{
					return quaternion.eulerAngles.y;
				}
				if (fieldName == "z (euler)")
				{
					return quaternion.eulerAngles.z;
				}
				return null;
			}
		}

		public bool IsKnownType(Type type)
		{
			return PrettyTypeNames.ContainsKey(type);
		}

		public bool IsAcceptableType(Type type)
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

		private string GetPrettyName(Type type)
		{
			if (!PrettyTypeNames.TryGetValue(type, out var readable))
			{
				readable = type.Name;
			}
			return readable;
		}

		public string GetPrettyName(string typeName)
		{
			var type = Type.GetType(typeName);
			return GetPrettyName(type);
		}

		public interface ITypeInspector
		{
			Object GetValue(Object instance, string fieldName);
			string[] FieldNameStrings { get; }
			Field[] Fields { get; }
		}
	}

}
