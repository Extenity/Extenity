#if UNITY_5_3_OR_NEWER

using System;
using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	[Serializable]
	public struct Target
	{
		public Transform Transform;
		public Vector3 Position;

		public void RefreshPosition()
		{
			if (Transform != null)
			{
				Position = Transform.position;
			}
		}
	}

}

#endif
