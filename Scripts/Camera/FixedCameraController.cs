using UnityEngine;
using Extenity.Logging;
using System.Collections;
using System.Collections.Generic;
using SpyQ;
using UnityEngine.EventSystems;

namespace Extenity.CameraManagement
{

	public class FixedCameraController : CameraController
	{
		#region Initialization

		//private void Start()
		//{
		//}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region Update

		//private void Update()
		//{
		//}

		#endregion

		#region Input

		public override bool IsKeyboardAndJoystickActive { get; set; }
		public override bool IsMouseActive { get; set; }

		#endregion

		#region Debug - Gizmos

		//private void OnDrawGizmosSelected()
		//{
		//}

		#endregion
	}

}
