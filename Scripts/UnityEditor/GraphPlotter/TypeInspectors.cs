using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
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

		private readonly Dictionary<Type, string> valueTypes = new Dictionary<Type, string>
		{
			{ typeof(Single), "float" },
			{ typeof(Int32), "int" },
			{ typeof(Boolean), "bool" },
			{ typeof(Double), "double" } ,
		};

		private readonly Dictionary<Type, ITypeInspector> inspectors = new Dictionary<Type, ITypeInspector>
		{
			{ typeof(Quaternion), new QuaternionTypeInspector() }
		};

		#endregion

		public ITypeInspector GetTypeInspector(Type type)
		{
			ITypeInspector inspector = null;
			if (!inspectors.TryGetValue(type, out inspector))
			{
				inspector = new DefaultTypeInspector(this, type);
				inspectors.Add(type, inspector);
			}
			return inspector;
		}

		private class DefaultTypeInspector : ITypeInspector
		{
			private string[] fieldNameStrings;
			private Field[] fields;
			private Dictionary<string, FieldInfo> nameToFieldInfo;

			public DefaultTypeInspector(TypeInspectors typeInspectors, Type type)
			{
				var fieldInfos = new List<FieldInfo>();

				var currentType = type;
				while (currentType != null && currentType != typeof(UnityEngine.Object))
				{
					fieldInfos.AddRange(currentType.GetFields(Flags).Where(field => typeInspectors.IsAcceptableType(field.FieldType)));
					currentType = currentType.BaseType;
				}

				fieldNameStrings = new string[fieldInfos.Count];
				fields = new Field[fieldInfos.Count];
				nameToFieldInfo = new Dictionary<string, FieldInfo>();

				for (int i = 0; i < fieldInfos.Count; i++)
				{
					var fieldInfo = fieldInfos[i];

					fieldNameStrings[i] = fieldInfo.Name + " : " + typeInspectors.GetReadableName(fieldInfo.FieldType);

					var field = new Field();
					field.name = fieldInfo.Name;
					field.type = fieldInfo.FieldType;

					nameToFieldInfo.Add(field.name, fieldInfo);

					fields[i] = field;
				}
			}

			public System.Object GetValue(System.Object instance, string fieldName)
			{
				FieldInfo fieldInfo;
				if (!nameToFieldInfo.TryGetValue(fieldName, out fieldInfo))
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
		}

		private class QuaternionTypeInspector : ITypeInspector
		{
			private readonly string[] _FieldNameStrings =
			{
				"x (euler) : float",
				"y (euler) : float",
				"z (euler) : float"
			};
			private readonly Field[] _Fields =
			{
				new Field() { name = "x (euler)", type = typeof(float) },
				new Field() { name = "y (euler)", type = typeof(float) },
				new Field() { name = "z (euler)", type = typeof(float) },
			};

			public string[] FieldNameStrings { get { return _FieldNameStrings; } }
			public Field[] Fields { get { return _Fields; } }

			public System.Object GetValue(System.Object instance, string fieldName)
			{
				var quaternion = (Quaternion)instance;
				if (fieldName == "x (euler)")
				{
					return quaternion.eulerAngles.x;
				}
				else if (fieldName == "y (euler)")
				{
					return quaternion.eulerAngles.y;
				}
				else if (fieldName == "z (euler)")
				{
					return quaternion.eulerAngles.z;
				}
				else
				{
					return null;
				}
			}
		}

		public bool IsSampleType(Type type)
		{
			return valueTypes.ContainsKey(type);
		}

		public bool IsAcceptableType(Type type)
		{
			if (IsSampleType(type))
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

		private string GetReadableName(Type type)
		{
			string readable;
			if (!valueTypes.TryGetValue(type, out readable))
			{
				readable = type.Name;
			}
			return readable;
		}

		public string GetReadableName(string typeName)
		{
			var type = Type.GetType(typeName);
			return GetReadableName(type);
		}

		public interface ITypeInspector
		{
			System.Object GetValue(System.Object instance, string fieldName);
			string[] FieldNameStrings { get; }
			Field[] Fields { get; }
		}
	}

}
