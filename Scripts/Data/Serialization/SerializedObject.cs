using System;
using UnityEngine;

namespace Serialization
{

	public abstract class SerializedObject
	{
		protected abstract void OnSerialize(Serializer serializer);
		protected abstract void OnDeserialize(Serializer serializer);

		#region Invoke Serialization

		internal void InvokeOnSerialize(Serializer serializer)
		{
			OnSerialize(serializer);
		}

		internal void InvokeOnDeserialize(Serializer serializer)
		{
			OnDeserialize(serializer);
		}

		#endregion
	}

}
