using UnityEngine;

namespace Extenity.DataToolbox
{

	public struct OrientedPosition
	{
		public Vector3 Position;

		/// <summary>
		/// This field can be used as forward vector, up vector or euler angles in the context.
		/// </summary>
		public Vector3 Orientation;
	}

}
