using System;
using UnityEngine;

namespace Extenity.DataToolbox
{

	public abstract class SerializedMonoBehaviour : MonoBehaviour
	{
		protected abstract void OnSerialize(Serializer serializer);
		protected abstract void OnDeserialize(Serializer serializer);

		#region Invoke Serialization

		internal void InvokeOnSerialize(Serializer serializer)
		{
			if (gameObject == null)
			{
				throw new Exception("Tried to serialize an object but it's reference was lost.");
			}
			if (serializer == null)
			{
				throw new Exception(string.Format("Tried to serialize object '{0}' but serializer is null.", gameObject.name));
			}

			OnSerialize(serializer);
		}

		internal void InvokeOnDeserialize(Serializer serializer)
		{
			OnDeserialize(serializer);
		}

		#endregion
	}

}
