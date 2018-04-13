using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Extenity.DataToolbox;

namespace Extenity.GameObjectToolbox
{

	public static class ComponentTools
	{
		#region Get Serialized Fields

		public static List<FieldInfo> GetUnitySerializedFields(this UnityEngine.Object unityObject)
		{
			if (!unityObject)
				throw new ArgumentNullException("unityObject");

			var fields = new List<FieldInfo>();
			var allFields = unityObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

			for (int i = 0; i < allFields.Length; i++)
			{
				var field = allFields[i];
				if (
					(field.IsPublic && !Attribute.IsDefined(field, typeof(NonSerializedAttribute))) || 
					Attribute.IsDefined(field, typeof(SerializeField))
				)
				{
					if (!field.IsReadOnly())
					{
						fields.Add(field);
					}
				}
			}

			return fields;
		}

		#endregion
	}

}
