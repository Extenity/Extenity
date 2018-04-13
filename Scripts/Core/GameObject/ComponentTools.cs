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

		public static List<FieldInfo> GetUnitySerializedFields(this object obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");
			var type = obj as Type;
			if (type == null)
				type = obj.GetType();

			var fields = new List<FieldInfo>();
			var allFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

			for (int i = 0; i < allFields.Length; i++)
			{
				var field = allFields[i];
				var nonSerializedAttributeDefined = Attribute.IsDefined(field, typeof(NonSerializedAttribute));
				var serializeFieldAttributeDefined = Attribute.IsDefined(field, typeof(SerializeField));
				if (nonSerializedAttributeDefined && serializeFieldAttributeDefined)
				{
					throw new Exception(string.Format(
						"Don't know what to do about field '{0}' that has both 'NonSerialized' and 'SerializeField' attributes.",
						field.Name));
				}
				if (!nonSerializedAttributeDefined && // See if the field is specified as not serialized on purpose
					!field.IsReadOnly() && // Unity won't serialize readonly fields
					!field.FieldType.IsDictionary() && // Unity won't serialize Dictionary fields
					(field.IsPublic || serializeFieldAttributeDefined) // See if the field is meant to be serialized
				)
				{
					fields.Add(field);
				}
			}

			return fields;
		}

		#endregion
	}

}
