#if UNITY_5_3_OR_NEWER

using UnityEngine;

namespace Extenity.CameraToolbox
{

	/// <summary>
	/// Fix for the problem described below. Also make sure all cameras have GUILayer component.
	/// https://forum.unity.com/threads/how-to-turn-off-sendmouseevents.160372/
	/// </summary>
	public class CameraEventMaskDisabler : MonoBehaviour
	{
		public Camera Camera;

		private void Start()
		{
			Camera.eventMask = 0;
		}
	}

}

#endif
