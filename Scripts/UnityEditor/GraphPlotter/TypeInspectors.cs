// ============================================================================
//   Monitor Components v. 1.04 - written by Peter Bruun (twitter.com/ptrbrn)
//   More info on Asset Store: http://u3d.as/9MW
// ============================================================================

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MonitorComponents 
{
	public class TypeInspectors 
	{
		private HashSet<Type> sampleTypes;
		
		private static BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		private static TypeInspectors instance = null;

		private Dictionary<Type, ITypeInspector> inspectors = new Dictionary<Type, ITypeInspector>();

		public TypeInspectors()
		{
			sampleTypes = new HashSet<Type>() {
				typeof(int), 
				typeof(float), 
				typeof(double), 
				typeof(bool)
			};

			inspectors.Add(typeof(Quaternion), new QuaternionTypeInspector());
		}

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
				List<FieldInfo> fieldInfos = new List<FieldInfo>();

				Type currentType = type;
				while (currentType != null && currentType != typeof(UnityEngine.Object))
				{
					fieldInfos.AddRange(currentType.GetFields(flags));
					currentType = currentType.BaseType;
				}
				
				fieldInfos.RemoveAll(f => !typeInspectors.IsAcceptableType(f.FieldType));

				fieldNameStrings = new string[fieldInfos.Count];
				fields = new Field[fieldInfos.Count];
				nameToFieldInfo = new Dictionary<string, FieldInfo>();

				for (int i = 0; i < fieldInfos.Count; i++)
				{
					FieldInfo fieldInfo = fieldInfos[i];
					
					fieldNameStrings[i] = fieldInfo.Name + " : " + GetReadableName(fieldInfo.FieldType);

					Field field = new Field();
					field.name = fieldInfo.Name;
					field.type = fieldInfo.FieldType;
					
					nameToFieldInfo.Add(field.name, fieldInfo);

					fields[i] = field;
				}
			}

			public System.Object GetValue(System.Object instance, string fieldName) 
			{
				FieldInfo fieldInfo;
				if(!nameToFieldInfo.TryGetValue(fieldName, out fieldInfo))
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
			private string[] fieldNameStrings = new string[] { "x (euler) : float", "y (euler) : float", "z (euler) : float" };
			private Field[] fields = new Field[] { 
				new Field() { name = "x (euler)", type = typeof(float) },
				new Field() { name = "y (euler)", type = typeof(float) },
				new Field() { name = "z (euler)", type = typeof(float) },
			};

			public string[] FieldNameStrings { get { return fieldNameStrings; } }
			public Field[] Fields { get { return fields; } }

			public System.Object GetValue(System.Object instance, string fieldName)
			{
				Quaternion quaternion = (Quaternion) instance;
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
			return sampleTypes.Contains(type);
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

		private static string GetReadableName(Type type)
	    {
	    	string readable;
	    	if (!valueTypes.TryGetValue(type, out readable))
	    	{
	    		readable = type.Name;
	    	}

	    	return readable;
	    }

	    public static string GetReadableName(string typeName)
	    {
	    	Type type = Type.GetType(typeName);
	    	return GetReadableName(type);	
	    }

		private static Dictionary<Type,string> valueTypes = new Dictionary<Type, string>() {
			{ typeof(System.Single), "float" },
			{ typeof(System.Int32), "int" },
			{ typeof(System.Boolean), "bool" },
			{ typeof(System.Double), "double" } ,
		};


		// Singleton stuff.

		public static TypeInspectors Instance
		{
			get 
			{
				if (instance == null)
				{
					instance = new TypeInspectors();
				}

				return instance;
			}
		}

		public interface ITypeInspector 
		{
			System.Object GetValue(System.Object instance, string fieldName);
			string[] FieldNameStrings { get; }
			Field[] Fields { get; }
		}
	}
}
