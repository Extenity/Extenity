using Extenity.MathToolbox;
using UnityEditor;
using UnityEngine;

namespace ExtenityExamples.MathToolbox
{

	public class Example_PlaneLinecast : MonoBehaviour
	{
		public Transform Line1;
		public Transform Line2;

		public Transform PlaneCenter;
		public Transform PlaneNormalTip;

		public Transform ProximityCheckingPoint;
		public float ProximityCheckingRadius = 2f;

		public Transform RaycastResult;

		public Plane Plane;

		public bool StickProximityCheckingPointToPlane = true;

		public Color InsideProximityColor = Color.green;
		public Color OutsideProximityColor = Color.red;

		private void OnDrawGizmos()
		{
			var planeCenter = PlaneCenter.position;
			var planeNormal = PlaneNormalTip.position - PlaneCenter.position;
			Plane = new Plane(planeNormal.normalized, planeCenter);
			PlaneCenter.LookAt(PlaneNormalTip, Vector3.up);

			if (StickProximityCheckingPointToPlane)
			{
				ProximityCheckingPoint.position = Plane.ClosestPointOnPlane(ProximityCheckingPoint.position);
			}

			Handles.color = Color.gray;
			Gizmos.color = Color.gray;
			for (float radius = 1f; radius < 10f; radius += 1f)
			{
				Handles.DrawWireDisc(PlaneCenter.position, Plane.normal, radius);
			}
			Gizmos.DrawLine(PlaneCenter.position, PlaneNormalTip.position);

			// Proximity circle
			var isInsideProximity = Plane.LinecastWithProximity(
				Line1.position, Line2.position,
				ProximityCheckingPoint.position, ProximityCheckingRadius);
			Handles.color = isInsideProximity ? InsideProximityColor : OutsideProximityColor;
			Handles.DrawWireDisc(ProximityCheckingPoint.position, Plane.normal, ProximityCheckingRadius);

			// Line
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(Line1.position, Line2.position);

			// Raycast result
			var ray = new Ray(Line1.position, Line2.position - Line1.position);
			float distance;
			if (Plane.Raycast(ray, out distance))
			{
				RaycastResult.position = ray.GetPoint(distance);
				RaycastResult.rotation = PlaneCenter.rotation;
				RaycastResult.gameObject.SetActive(true);
			}
			else
			{
				RaycastResult.gameObject.SetActive(false);
			}
		}

	}

}
