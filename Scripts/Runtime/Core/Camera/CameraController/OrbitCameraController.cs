using System;
using UnityEngine;
using Extenity.DebugToolbox;
using Extenity.InputToolbox;

namespace Extenity.CameraToolbox
{

	public class OrbitCameraController : CameraController
	{
		#region Constants

		private const string MouseXAxisName = "Mouse X";
		private const string MouseYAxisName = "Mouse Y";

		#endregion

		#region Initialization

		protected override void OnEnable()
		{
			base.OnEnable();

			ResetDynamics();
		}

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

		[Header("Toggle Input Methods")]
		public bool IsAxisActive = true;
		public bool IsMouseActive = true;
		//public override bool IsTouchActive;

		private void UpdateInput()
		{
			if (IsGUIActiveInCurrentEventSystem)
				return;

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
						InputRotate.y += Input.GetAxis(MouseXAxisName) * RotationMouseSensitivity;
						InputRotate.x -= Input.GetAxis(MouseYAxisName) * RotationMouseSensitivity;
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
							InputShift.x -= Input.GetAxis(MouseXAxisName) * MovementMouseSensitivity;
							InputShift.z -= Input.GetAxis(MouseYAxisName) * MovementMouseSensitivity;
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
					var scrollChange = -Input.mouseScrollDelta.y;
					if (scrollChange != 0f)
					{
						BreakIdle();
						InputZoom += scrollChange;
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
			var camera = Camera;

			TargetOrbitPosition = OrbitCenter.position;
			TargetRotation = camera
				? camera.transform.rotation
				: Quaternion.identity;
			TargetZoom = camera
				? (camera.transform.position - OrbitCenter.position).magnitude
				: MinimumZoom;
			CurrentZoom = TargetZoom;
		}

		private void UpdateDynamics()
		{
			var camera = Camera;
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

			if (camera)
			{
				TargetOrbitPosition += Quaternion.Euler(0f, camera.transform.rotation.eulerAngles.y, 0f) * appliedMovement;
			}
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
			if (camera)
			{
				camera.transform.position += (targetTotalPosition - camera.transform.position) * MovementSmoothingFactor;
				camera.transform.rotation = Quaternion.Slerp(camera.transform.rotation, TargetRotation, RotationSmoothingFactor);
			}
		}

		#endregion

		#region Movement Area

		[Header("Movement Area - Orbit")]
		public Transform OrbitCenter;
		//public Bounds MovementArea;
		public float MaxDistanceFromOrbitCenter = 0.5f;

		#endregion

		#region Debug - Gizmos

#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			DebugDraw.CircleXZ(OrbitCenter.position, MaxDistanceFromOrbitCenter, Color.cyan);

			//Gizmos.DrawWireCube(MovementArea.center, MovementArea.size);
		}

#endif

		#endregion
	}

}
