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

		[Header("Input - Orbit")]
		public bool EnableUserMovement = true;
		public int MovementMouseButton = 0;
		public float MovementMouseSensitivity = 1f;
		//public float MovementTouchSensitivity = 1f;

		public bool EnableUserRotation = true;
		public int RotationMouseButton = 1;
		public float RotationMouseSensitivity = 1f;
		//public float RotationTouchSensitivity = 1f;

		public bool EnableUserZoom = true;
		//public float ZoomTouchSensitivity = 1f;

		[NonSerialized]
		public Vector3 InputShift;
		[NonSerialized]
		public Vector3 InputRotate;
		[NonSerialized]
		public float InputZoom;

		public override bool IsAxisActive { get; set; }
		public override bool IsMouseActive { get; set; }
		//public override bool IsTouchActive { get; set; }

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
				if (EnableUserRotation && Input.GetMouseButton(RotationMouseButton))
				{
					BreakIdle();
					if (!IgnoreInitialDragOnClick || IsDragging)
					{
						InputRotate.y += Input.GetAxis("Mouse X") * RotationMouseSensitivity;
						InputRotate.x -= Input.GetAxis("Mouse Y") * RotationMouseSensitivity;
					}
					IsDragging = true;
					MouseCursor.HideCursor();
				}
				else
				{
					MouseCursor.ShowCursor();

					if (EnableUserMovement && Input.GetMouseButton(MovementMouseButton))
					{
						BreakIdle();
						if (!IgnoreInitialDragOnClick || IsDragging)
						{
							InputShift.x -= Input.GetAxis("Mouse X") * MovementMouseSensitivity;
							InputShift.z -= Input.GetAxis("Mouse Y") * MovementMouseSensitivity;
						}
						IsDragging = true;
					}
					else
					{
						IsDragging = false;
					}
				}

				if (EnableUserZoom)
				{
					InputZoom += -Input.mouseScrollDelta.y;
					if (InputZoom != 0f)
					{
						if (ZoomOnlyBreaksIdlingWhenNotIdle && !IsIdle)
						{
							BreakIdle();
						}
					}
				}
			}
		}

		private void ResetInput()
		{
			InputShift = Vector3.zero;
			InputRotate = Vector3.zero;
			InputZoom = 0f;
		}

		#endregion

		#region Idle

		[Header("Idle - Orbit")]
		public bool ZoomOnlyBreaksIdlingWhenNotIdle = true;

		#endregion

		#region Dynamics

		[Header("Dynamics - Orbit")]
		public float MovementSpeed = 0.05f;
		public float RotationSpeed = 0.3f;
		[Range(-90f, 90f)]
		public float MinimumPitch = -90f;
		[Range(-90f, 90f)]
		public float MaximumPitch = 90f;
		public float ZoomSpeed = 0.01f;
		public float MinimumZoom = 2f;
		public float MaximumZoom = 30f;
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
			if (newRotationX > 180f)
				newRotationX -= 360f;
			// Clip rotation to prevent gimbal locks
			const float tolerance = 0.01f;
			if (newRotationX < MinimumPitch + tolerance)
				newRotationX = MinimumPitch + tolerance;
			if (newRotationX > MaximumPitch - tolerance)
				newRotationX = MaximumPitch - tolerance;

			TargetOrbitPosition += Quaternion.Euler(0f, Camera.transform.rotation.eulerAngles.y, 0f) * appliedMovement;
			TargetRotation = Quaternion.Euler(0f, newRotationY, 0f) * Quaternion.Euler(newRotationX, 0f, 0f);
			TargetZoom += appliedZoom;
			if (TargetZoom > MaximumZoom)
				TargetZoom = MaximumZoom;
			if (TargetZoom < MinimumZoom)
				TargetZoom = MinimumZoom;

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

		[Header("Movement Area - Orbit")]
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
