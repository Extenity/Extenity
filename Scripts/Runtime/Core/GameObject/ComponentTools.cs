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
				throw new ArgumentNullException(nameof(obj));
			var type = obj as Type;
			if (type != null)
			{
				throw new InternalException(723659); // Encountered an unexpected behaviour. Developer attention is needed. See below.
			}
			type = obj.GetType();
			// This was the old implementation, which I'm not sure if it was a faulty copy-paste mistake as some
			// similar usages exist in some other methods. There is a chance it really means something.
			// If you encounter, figure out what should be done. Trying to get serialized fields of a Type
			// seems to be a dull operation, but maybe I'm wrong.
			//
			// Delete the lines after a year if we don't encounter the error above.
			//if (type == null)
			//	type = obj.GetType();

			var fields = new List<FieldInfo>();
			var allFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

			for (int i = 0; i < allFields.Length; i++)
			{
				var field = allFields[i];
				var nonSerializedAttributeDefined = Attribute.IsDefined(field, typeof(NonSerializedAttribute));
				var serializeFieldAttributeDefined = Attribute.IsDefined(field, typeof(SerializeField));
				if (nonSerializedAttributeDefined && serializeFieldAttributeDefined)
				{
					throw new Exception($"Don't know what to do about field '{field.Name}' that has both 'NonSerialized' and 'SerializeField' attributes.");
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
