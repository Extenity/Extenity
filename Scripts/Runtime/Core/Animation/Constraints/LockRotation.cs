using UnityEngine;

namespace Extenity.AnimationToolbox
{

	public class LockRotation : MonoBehaviour
	{
		private Transform _Transform;
		private Transform Transform
		{
			get
			{
				if (!_Transform)
					_Transform = transform;
				return _Transform;
			}
		}

		private void LateUpdate()
		{
			Transform.rotation = Quaternion.identity;
		}
	}

}
