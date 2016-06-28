using UnityEngine;
using System.Collections;
using AdvancedInspector;
using SmartData;
using UnityEngine.Events;

namespace CustomPhysics.Inverse
{

	public class InversePhysics : MonoBehaviour
	{
		#region Initialization

		protected void Awake()
		{
			Location.Reference = transform;
			Location.Tag = this; // Note that this can be overriden elsewhere. Setting Tag here does not mean the tag will always stay as reference to this object.
		}

		#endregion

		#region Update

		protected void FixedUpdate()
		{
			FixedUpdateVelocityDetector();
			Location.CheckForChanges();
		}

		#endregion

		#region Events

		public class InversePhysicsEvent : UnityEvent<InversePhysics> { }

		#endregion

		#region Location Change Detector

		private SmartTransform _Location = new SmartTransform(false);
		[Inspect, ReadOnly, Group("Location")]
		public SmartTransform Location { get { return _Location; } }

		#endregion

		#region Velocity Detector

		[Inspect, ReadOnly, Group("Velocity")]
		public Vector3 Velocity { get; private set; }
		[Inspect, ReadOnly, Group("Velocity")]
		public Vector3 LocalVelocity { get; private set; }
		[Inspect, ReadOnly, Group("Velocity")]
		public Vector3 PreviousVelocity { get; private set; }
		public int VelocityCalculationPauseCounter { get; private set; }
		[Inspect, ReadOnly, Group("Velocity")]
		public bool IsVelocityCalculationPaused { get { return VelocityCalculationPauseCounter > 0; } }

		private void FixedUpdateVelocityDetector()
		{
			PreviousVelocity = Velocity;

			if (!IsVelocityCalculationPaused)
			{
				Velocity = Location.positionDelta / Time.fixedDeltaTime;

				// Prevent too much speed changes
				if (Velocity.magnitude > 100f) // 100 m/s
				{
					Velocity = Vector3.zero;
				}

				LocalVelocity = transform.InverseTransformDirection(Velocity);
			}
			else
			{
				Velocity = Vector3.zero;
				LocalVelocity = Vector3.zero;
			}
		}

		public void PauseVelocityCalculation()
		{
			VelocityCalculationPauseCounter++;
		}

		public void ResumeVelocityCalculation()
		{
			VelocityCalculationPauseCounter--;
		}

		#endregion
	}

}
