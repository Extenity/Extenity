using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Xml.Linq;

namespace Serialization
{

	public sealed class Serializer : IDisposable
	{
		#region Initialization

		internal Serializer()
		{
			Document = new XDocument();

			CurrentElement = null;
		}

		#endregion

		#region Deinitialization

		public void Dispose()
		{
			Document = null;
			CurrentElement = null;
		}

		#endregion

		#region XML

		private XDocument Document;
		private XElement CurrentElement;

		#endregion

		#region Write Data

		public void Write(string fieldName, string fieldValue)
		{
			CurrentElement.Add(new XElement(fieldName, fieldValue));
		}

		public void Write(string fieldName, bool fieldValue)
		{
			CurrentElement.Add(new XElement(fieldName, fieldValue.ToString()));
		}

		public void Write(string fieldName, int fieldValue)
		{
			CurrentElement.Add(new XElement(fieldName, fieldValue.ToString()));
		}

		public void Write(string fieldName, float fieldValue)
		{
			CurrentElement.Add(new XElement(fieldName, fieldValue.ToString()));
		}

		public void Write(string fieldName, Vector2 fieldValue)
		{
			CurrentElement.Add(new XElement(fieldName, fieldValue.x + " " + fieldValue.y));
		}

		public void Write(string fieldName, Vector3 fieldValue)
		{
			CurrentElement.Add(new XElement(fieldName, fieldValue.x + " " + fieldValue.y + " " + fieldValue.z));
		}

		public void Write(string fieldName, Vector4 fieldValue)
		{
			CurrentElement.Add(new XElement(fieldName, fieldValue.x + " " + fieldValue.y + " " + fieldValue.z + " " + fieldValue.w));
		}

		public void Write(string fieldName, Quaternion fieldValue)
		{
			Write(fieldName, fieldValue.eulerAngles);
		}

		public void Write(string fieldName, Guid fieldValue)
		{
			CurrentElement.Add(new XElement(fieldName, fieldValue.ToString()));
		}

		#endregion

		#region Read Data

		public string ReadString(string fieldName, string defaultValue)
		{
			var element = CurrentElement.Element(fieldName);
			if (element != null)
			{
				return element.Value;
			}
			return defaultValue;
		}

		public bool ReadBool(string fieldName, bool defaultValue)
		{
			bool value;
			var element = CurrentElement.Element(fieldName);
			if (element != null && bool.TryParse(element.Value, out value))
			{
				return value;
			}
			return defaultValue;
		}

		public float ReadFloat(string fieldName, float defaultValue)
		{
			float value;
			var element = CurrentElement.Element(fieldName);
			if (element != null && float.TryParse(element.Value, out value))
			{
				return value;
			}
			return defaultValue;
		}

		public int ReadInt(string fieldName, int defaultValue)
		{
			int value;
			var element = CurrentElement.Element(fieldName);
			if (element != null && int.TryParse(element.Value, out value))
			{
				return value;
			}
			return defaultValue;
		}

		public Vector2 ReadVector2(string fieldName, Vector2 defaultValue)
		{
			Vector2 value = Vector2.zero;
			var element = CurrentElement.Element(fieldName);
			if (element != null && !string.IsNullOrEmpty(element.Value))
			{
				var split = element.Value.Split(' ');
				if (split.Length == 2)
				{
					if (float.TryParse(split[0], out value.x) &&
						float.TryParse(split[1], out value.y))
					{
						return value;
					}
				}
			}
			return defaultValue;
		}

		public Vector3 ReadVector3(string fieldName, Vector3 defaultValue)
		{
			Vector3 value = Vector3.zero;
			var element = CurrentElement.Element(fieldName);
			if (element != null && !string.IsNullOrEmpty(element.Value))
			{
				var split = element.Value.Split(' ');
				if (split.Length == 3)
				{
					if (float.TryParse(split[0], out value.x) &&
						float.TryParse(split[1], out value.y) &&
						float.TryParse(split[2], out value.z))
					{
						return value;
					}
				}
			}
			return defaultValue;
		}

		public Vector4 ReadVector4(string fieldName, Vector4 defaultValue)
		{
			Vector4 value = Vector4.zero;
			var element = CurrentElement.Element(fieldName);
			if (element != null && !string.IsNullOrEmpty(element.Value))
			{
				var split = element.Value.Split(' ');
				if (split.Length == 4)
				{
					if (float.TryParse(split[0], out value.x) &&
						float.TryParse(split[1], out value.y) &&
						float.TryParse(split[2], out value.z) &&
						float.TryParse(split[3], out value.w))
					{
						return value;
					}
				}
			}
			return defaultValue;
		}

		public Quaternion ReadQuaternionAsEuler(string fieldName, Vector3 defaultValue)
		{
			Vector3 value = Vector3.zero;
			var element = CurrentElement.Element(fieldName);
			if (element != null && !string.IsNullOrEmpty(element.Value))
			{
				var split = element.Value.Split(' ');
				if (split.Length == 3)
				{
					if (float.TryParse(split[0], out value.x) &&
						float.TryParse(split[1], out value.y) &&
						float.TryParse(split[2], out value.z))
					{
						return Quaternion.Euler(value);
					}
				}
			}
			return Quaternion.Euler(defaultValue);
		}

		public T ReadEnum<T>(string fieldName, T defaultValue)
		{
			var element = CurrentElement.Element(fieldName);
			if (element != null)
			{
				try
				{
					return (T)Enum.Parse(typeof(T), element.Value);
				}
				catch (Exception)
				{
					// ignored
				}
			}
			return defaultValue;
		}

		public Guid ReadGuid(string fieldName, Guid defaultValue)
		{
			var element = CurrentElement.Element(fieldName);
			if (element != null)
			{
				return new Guid(element.Value);
			}
			return defaultValue;
		}

		#endregion

		#region Group

		/// <summary>
		/// Creates new group with specified name. Don't forget to call PopGroup for each Push and 
		/// successful Peak operations.
		/// </summary>
		public void PushGroup(string groupName)
		{
			var child = new XElement(groupName);

			if (CurrentElement != null)
			{
				CurrentElement.Add(child);
			}
			else
			{
				Document.Add(child);
			}

			CurrentElement = child;
		}

		/// <summary>
		/// Gets the first group with specified name. If there are multiple groups with the same name,
		/// you may want to use other PeakGroup overrides and GetGroupCount. Don't forget to call 
		/// PopGroup for each Push and successful Peak operations.
		/// </summary>
		/// <returns>
		/// True if there is a group with the specified name. You should only call PopGroup when
		/// peak is successful. 
		/// </returns>
		public bool PeakGroup(string groupName)
		{
			var child = CurrentElement != null
				? CurrentElement.Element(groupName)
				: Document.Element(groupName);

			if (child != null)
			{
				CurrentElement = child;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Allows to get group with specified name at specified index if there are multiple groups with 
		/// the same name. Don't forget to call PopGroup for each Push and successful Peak operations.
		/// </summary>
		/// <returns>
		/// True if there is a group with the specified name. You should only call PopGroup when
		/// peak is successful. 
		/// </returns>
		public bool PeakGroup(string groupName, int groupIndex)
		{
			var child = CurrentElement != null
				? CurrentElement.Elements(groupName).Skip(groupIndex).First()
				: Document.Elements(groupName).Skip(groupIndex).First();

			if (child != null)
			{
				CurrentElement = child;
				return true;
			}
			return false;
		}

		/// <summary>
		/// See PushGroup or PeakGroup for more information.
		/// </summary>
		public void PopGroup()
		{
			CurrentElement = CurrentElement.Parent;
		}

		public int GetGroupCount(string groupName)
		{
			return CurrentElement != null
				? CurrentElement.Elements(groupName).Count()
				: Document.Elements(groupName).Count();
		}

		#endregion

		#region Read Data

		// TODO:

		#endregion

		#region Serialize/Deserialize Object

		public void SerializeObject(SerializedMonoBehaviour theObject)
		{
			if (theObject == null)
				throw new ArgumentNullException("theObject");

			theObject.InvokeOnSerialize(this);
		}

		public void SerializeObject(SerializedObject theObject)
		{
			if (theObject == null)
				throw new ArgumentNullException("theObject");

			theObject.InvokeOnSerialize(this);
		}

		public void DeserializeObject(SerializedMonoBehaviour theObject)
		{
			if (theObject == null)
				throw new ArgumentNullException("theObject");

			theObject.InvokeOnDeserialize(this);
		}

		public void DeserializeObject(SerializedObject theObject)
		{
			if (theObject == null)
				throw new ArgumentNullException("theObject");

			theObject.InvokeOnDeserialize(this);
		}

		#endregion

		#region Serialize/Deserialize Sub Object

		public void SerializeSubObject(SerializedMonoBehaviour subObject, string containerFieldName)
		{
			if (subObject == null)
				throw new ArgumentNullException("subObject");
			if (string.IsNullOrEmpty(containerFieldName))
				throw new ArgumentNullException("containerFieldName");

			PushGroup(containerFieldName);
			{
				subObject.InvokeOnSerialize(this);
			}
			PopGroup();
		}

		public void SerializeSubObject(SerializedObject subObject, string containerFieldName)
		{
			if (subObject == null)
				throw new ArgumentNullException("subObject");
			if (string.IsNullOrEmpty(containerFieldName))
				throw new ArgumentNullException("containerFieldName");

			PushGroup(containerFieldName);
			{
				subObject.InvokeOnSerialize(this);
			}
			PopGroup();
		}

		public void DeserializeSubObject(SerializedMonoBehaviour subObject, string containerFieldName)
		{
			if (subObject == null)
				throw new ArgumentNullException("subObject");
			if (string.IsNullOrEmpty(containerFieldName))
				throw new ArgumentNullException("containerFieldName");

			if (PeakGroup(containerFieldName))
			{
				subObject.InvokeOnDeserialize(this);
				PopGroup();
			}
		}

		public void DeserializeSubObject(SerializedObject subObject, string containerFieldName)
		{
			if (subObject == null)
				throw new ArgumentNullException("subObject");
			if (string.IsNullOrEmpty(containerFieldName))
				throw new ArgumentNullException("containerFieldName");

			if (PeakGroup(containerFieldName))
			{
				subObject.InvokeOnDeserialize(this);
				PopGroup();
			}
		}

		#endregion

		#region Save To File

		public void SaveToFile(string filePath)
		{
			Document.Save(filePath);
		}

		#endregion

		#region Load From File

		public void LoadFromFile(string filePath)
		{
			Document = XDocument.Load(filePath);
		}

		#endregion
	}

}
