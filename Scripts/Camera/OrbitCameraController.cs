using UnityEngine;
using Extenity.Logging;
using System.Collections;
using System.Collections.Generic;
using SpyQ;
using UnityEngine.EventSystems;

namespace Extenity.CameraManagement
{

	public class OrbitCameraController : CameraController
	{
		#region Initialization

		private void Start()
		{
			InitializeDynamics();
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

		public int MovementMouseButton = 0;
		public int RotationMouseButton = 1;
		public float MouseSensitivity = 1f;
		public float KeyboardSensitivity = 1f;
		private Vector3 InputShift;
		private Vector3 InputRotate;
		private float InputZoom;

		public override bool IsKeyboardAndJoystickActive { get; set; }
		public override bool IsMouseActive { get; set; }

		private void UpdateInput()
		{
			if (IsGUIActive)
				return;

			// Keyboard and joystick
			if (IsKeyboardAndJoystickActive)
			{
				InputShift.x += InputManager.Instance.GetAxis(InputAxis.Roll) * KeyboardSensitivity * Time.deltaTime;
				InputRotate.x += InputManager.Instance.GetAxis(InputAxis.Pitch) * KeyboardSensitivity * Time.deltaTime;
				InputRotate.y += InputManager.Instance.GetAxis(InputAxis.Yaw) * KeyboardSensitivity * Time.deltaTime;
				InputZoom += InputManager.Instance.GetAxis(InputAxis.Altitude) * KeyboardSensitivity * Time.deltaTime;
			}

			// Mouse
			if (IsMouseActive)
			{
				if (Input.GetMouseButton(RotationMouseButton))
				{
					InputRotate.y += Input.GetAxis("Mouse X") * MouseSensitivity;
					InputRotate.x += -Input.GetAxis("Mouse Y") * MouseSensitivity;

					MouseCursor.HideCursor();
				}
				else
				{
					MouseCursor.ShowCursor();

					if (Input.GetMouseButton(MovementMouseButton))
					{
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

		#region Orbit

		public Transform OrbitCenter;

		#endregion

		#region Dynamics

		public float MovementSpeed = 0.05f;
		public float RotationSpeed = 0.3f;
		public float ZoomSpeed = 0.01f;
		public float MovementSmoothingFactor = 0.04f;
		public float RotationSmoothingFactor = 0.04f;
		public float ZoomSmoothingFactor = 0.04f;

		private Vector3 TargetOrbitPosition;
		private Quaternion TargetRotation;
		private float TargetZoom;
		private float CurrentZoom;

		private void InitializeDynamics()
		{
			TargetOrbitPosition = OrbitCenter.position;
			TargetRotation = transform.rotation;
			TargetZoom = (transform.position - OrbitCenter.position).magnitude;
			CurrentZoom = TargetZoom;
		}

		private void UpdateDynamics()
		{
			var appliedMovement = MovementSpeed * InputShift;
			var appliedRotation = RotationSpeed * InputRotate;
			var appliedZoom = ZoomSpeed * InputZoom;

			var targetRotationEuler = TargetRotation.eulerAngles;

			TargetOrbitPosition += Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f) * appliedMovement;
			TargetRotation = Quaternion.Euler(0f, targetRotationEuler.y + appliedRotation.y, 0f) * Quaternion.Euler(targetRotationEuler.x + appliedRotation.x, 0f, 0f);
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
			transform.position += (targetTotalPosition - transform.position) * MovementSmoothingFactor;
			transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation, RotationSmoothingFactor);
		}

		#endregion

		#region Movement Area

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
