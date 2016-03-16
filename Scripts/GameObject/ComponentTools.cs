using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public static class ComponentTools
{
	#region Get Serialized Fields

	public static List<FieldInfo> GetUnitySerializedFields(this Component component)
	{
		var fields = new List<FieldInfo>();

		var allFields = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		for (int i = 0; i < allFields.Length; i++)
		{
			var field = allFields[i];
			if ((field.IsPublic && !Attribute.IsDefined(field, typeof(NonSerializedAttribute))) || Attribute.IsDefined(field, typeof(SerializeField)))
			{
				fields.Add(field);
			}
		}

		return fields;
	}

	#endregion
}
