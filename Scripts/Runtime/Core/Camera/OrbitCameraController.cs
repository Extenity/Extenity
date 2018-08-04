using System;
using UnityEngine;
using Extenity.DebugToolbox;
using Extenity.InputToolbox;

namespace Extenity.CameraToolbox
{

	public class OrbitCameraController : CameraController
	{
		#region Initialization

		protected override void OnEnable()
		{
			base.OnEnable();

			ResetDynamics();
		}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region Update

		private void Update()
		{
			UpdateInput();
		}

		private void FixedUpdate()
		{
			UpdateDynamics();
			ResetInput();
		}

		#endregion

		#region Input

		[Header("Input")]
		public int MovementMouseButton = 0;
		public int RotationMouseButton = 1;
		public float MouseSensitivity = 1f;
		public float AxisSensitivity = 1f;
		[NonSerialized]
		public Vector3 InputShift;
		[NonSerialized]
		public Vector3 InputRotate;
		[NonSerialized]
		public float InputZoom;

		public override bool IsAxisActive { get; set; }
		public override bool IsMouseActive { get; set; }

		private void UpdateInput()
		{
			// TODO: Find a way to do this check without requiring UI DLL.
			//if (IsGUIActive)
			//	return;

			// TODO: Find a way to properly do this.
			//// Keyboard and joystick
			//if (IsKeyboardAndJoystickActive)
			//{
			//	InputShift.x += InputManager.Instance.GetAxis(InputAxis.Roll) * KeyboardSensitivity * Time.deltaTime;
			//	InputRotate.x += InputManager.Instance.GetAxis(InputAxis.Pitch) * KeyboardSensitivity * Time.deltaTime;
			//	InputRotate.y += InputManager.Instance.GetAxis(InputAxis.Yaw) * KeyboardSensitivity * Time.deltaTime;
			//	InputZoom += InputManager.Instance.GetAxis(InputAxis.Altitude) * KeyboardSensitivity * Time.deltaTime;
			//}

			// Mouse
			if (IsMouseActive)
			{
				if (Input.GetMouseButton(RotationMouseButton))
				{
					BreakIdle();
					InputRotate.y += Input.GetAxis("Mouse X") * MouseSensitivity;
					InputRotate.x += -Input.GetAxis("Mouse Y") * MouseSensitivity;

					MouseCursor.HideCursor();
				}
				else
				{
					MouseCursor.ShowCursor();

					if (Input.GetMouseButton(MovementMouseButton))
					{
						BreakIdle();
						InputShift.x += -Input.GetAxis("Mouse X") * MouseSensitivity;
						InputShift.z += -Input.GetAxis("Mouse Y") * MouseSensitivity;
					}
				}
				InputZoom += -Input.mouseScrollDelta.y;
			}
		}

		private void ResetInput()
		{
			InputShift = Vector3.zero;
			InputRotate = Vector3.zero;
			InputZoom = 0f;
		}

		#endregion

		#region Dynamics

		[Header("Dynamics")]
		public float MovementSpeed = 0.05f;
		public float RotationSpeed = 0.3f;
		public float ZoomSpeed = 0.01f;
		public float MovementSmoothingFactor = 0.04f;
		public float RotationSmoothingFactor = 0.04f;
		public float ZoomSmoothingFactor = 0.04f;

		[NonSerialized]
		public Vector3 TargetOrbitPosition;
		[NonSerialized]
		public Quaternion TargetRotation;
		[NonSerialized]
		public float TargetZoom;
		[NonSerialized]
		public float CurrentZoom;

		private void ResetDynamics()
		{
			TargetOrbitPosition = OrbitCenter.position;
			TargetRotation = Camera.transform.rotation;
			TargetZoom = (Camera.transform.position - OrbitCenter.position).magnitude;
			CurrentZoom = TargetZoom;
		}

		private void UpdateDynamics()
		{
			var appliedMovement = MovementSpeed * InputShift;
			var appliedRotation = RotationSpeed * InputRotate;
			var appliedZoom = ZoomSpeed * InputZoom;

			var targetRotationEuler = TargetRotation.eulerAngles;

			var newRotationY = targetRotationEuler.y + appliedRotation.y;
			var newRotationX = targetRotationEuler.x + appliedRotation.x;
			// Clip rotation to prevent gimbal locks
			if (newRotationX > 180f)
			{
				if (newRotationX < 270.1f) // 270 + tolerance
					newRotationX = 270.1f;
			}
			else
			{
				if (newRotationX > 89.9f) // 90 + tolerance
					newRotationX = 89.9f;
			}

			TargetOrbitPosition += Quaternion.Euler(0f, Camera.transform.rotation.eulerAngles.y, 0f) * appliedMovement;
			TargetRotation = Quaternion.Euler(0f, newRotationY, 0f) * Quaternion.Euler(newRotationX, 0f, 0f);
			TargetZoom += appliedZoom;

			var differenceToOrbitCenter = TargetOrbitPosition - OrbitCenter.position;
			if (differenceToOrbitCenter.magnitude > MaxDistanceFromOrbitCenter)
			{
				TargetOrbitPosition = OrbitCenter.position + (differenceToOrbitCenter.normalized * MaxDistanceFromOrbitCenter);
			}

			// Clamp target position to movement area
			//TargetPosition.x = Mathf.Clamp(TargetPosition.x, MovementArea.min.x, MovementArea.max.x);
			//TargetPosition.y = Mathf.Clamp(TargetPosition.y, MovementArea.min.y, MovementArea.max.y);
			//TargetPosition.z = Mathf.Clamp(TargetPosition.z, MovementArea.min.z, MovementArea.max.z);

			var targetTotalPosition = TargetOrbitPosition - (TargetRotation * (Vector3.forward * CurrentZoom));

			CurrentZoom += (TargetZoom - CurrentZoom) * ZoomSmoothingFactor;
			Camera.transform.position += (targetTotalPosition - Camera.transform.position) * MovementSmoothingFactor;
			Camera.transform.rotation = Quaternion.Slerp(Camera.transform.rotation, TargetRotation, RotationSmoothingFactor);
		}

		#endregion

		#region Movement Area

		[Header("Movement Area")]
		public Transform OrbitCenter;
		//public Bounds MovementArea;
		public float MaxDistanceFromOrbitCenter = 0.5f;

		#endregion

		#region Debug - Gizmos

		private void OnDrawGizmosSelected()
		{
			DebugDraw.CircleXZ(OrbitCenter.position, MaxDistanceFromOrbitCenter, Color.cyan);

			//Gizmos.DrawWireCube(MovementArea.center, MovementArea.size);
		}

		#endregion
	}

}
