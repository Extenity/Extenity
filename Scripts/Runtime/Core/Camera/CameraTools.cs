#if UNITY

using UnityEngine;

namespace Extenity.CameraToolbox
{

	public static class CameraTools
	{
		public static Vector3? WorldToScreenPointWithReverseCheck(this Camera camera, Vector3 position)
		{
			if (Vector3.Dot(camera.transform.forward, position - camera.transform.position) > 0)
			{
				return camera.WorldToScreenPoint(position);
			}
			return null;
		}

		#region IsVisibleFrom

		public static bool IsCollidersVisibleFrom(this Transform transform, Vector3 eyePosition)
		{
			var colliders = transform.GetComponents<Collider>();
			for (int i = 0; i < colliders.Length; i++)
			{
				if (colliders[i].IsVisibleFrom(eyePosition))
					return true;
			}

			for (int iChild = 0; iChild < transform.childCount; iChild++)
			{
				if (transform.GetChild(iChild).IsCollidersVisibleFrom(eyePosition))
					return true;
			}

			return false;
		}

		public static bool IsVisibleFrom(this Collider collider, Vector3 eyePosition)
		{
			return IsVisibleFrom(collider.bounds, eyePosition);
		}

		public static bool IsRenderersVisibleFrom(this Transform transform, Vector3 eyePosition)
		{
			var renderer = transform.GetComponent<Renderer>();
			if (renderer != null)
				if (renderer.IsVisibleFrom(eyePosition))
					return true;

			for (int iChild = 0; iChild < transform.childCount; iChild++)
			{
				if (transform.GetChild(iChild).IsRenderersVisibleFrom(eyePosition))
					return true;
			}

			return false;
		}

		public static bool IsVisibleFrom(this Renderer renderer, Vector3 eyePosition)
		{
			return IsVisibleFrom(renderer.bounds, eyePosition);
		}

		public static bool IsVisibleFrom(this Bounds bounds, Vector3 eyePosition)
		{
			var center = bounds.center;
			var extents = bounds.extents;

			if (!Physics.Linecast(center, eyePosition))
				return true;

			Vector3 corner;
			// Front top left corner
			corner.x = center.x - extents.x; corner.y = center.y + extents.y; corner.z = center.z - extents.z;
			if (!Physics.Linecast(corner, eyePosition))
				return true;

			// Front top right corner
			corner.x = center.x + extents.x; /*corner.y = center.y + extents.y;*/ /*corner.z = center.z - extents.z;*/
			if (!Physics.Linecast(corner, eyePosition))
				return true;

			// Front bottom left corner
			corner.x = center.x - extents.x; corner.y = center.y - extents.y; /*corner.z = center.z - extents.z;*/
			if (!Physics.Linecast(corner, eyePosition))
				return true;

			// Front bottom right corner
			corner.x = center.x + extents.x; /*corner.y = center.y - extents.y;*/ /*corner.z = center.z - extents.z;*/
			if (!Physics.Linecast(corner, eyePosition))
				return true;

			// Back top left corner
			corner.x = center.x - extents.x; corner.y = center.y + extents.y; corner.z = center.z + extents.z;
			if (!Physics.Linecast(corner, eyePosition))
				return true;

			// Back top right corner
			corner.x = center.x + extents.x; /*corner.y = center.y + extents.y;*/ /*corner.z = center.z + extents.z;*/
			if (!Physics.Linecast(corner, eyePosition))
				return true;

			// Back bottom left corner
			corner.x = center.x - extents.x; corner.y = center.y - extents.y; /*corner.z = center.z + extents.z;*/
			if (!Physics.Linecast(corner, eyePosition))
				return true;

			// Back bottom right corner
			corner.x = center.x + extents.x; /*corner.y = center.y - extents.y;*/ /*corner.z = center.z + extents.z;*/
			if (!Physics.Linecast(corner, eyePosition))
				return true;

			return false;
		}

		#endregion

		#region Frustum Check

		private static Plane[] FrustumCheckPlanes;
		private static Camera LastCalculatedCameraOfFrustumCheckPlanes;
		private static int LastCalculatedFrameOfFrustumCheckPlanes = -1;

		public static bool IsInsideFrustum(this Camera camera, Bounds bounds)
		{
			if (FrustumCheckPlanes == null)
				FrustumCheckPlanes = new Plane[6];

			// Calculate frustum planes if required.
			// Assumes camera won't move in the same frame and that allows
			// calculating only once if called multiple times in the same frame.
			var currentFrame = Time.frameCount;
			if (LastCalculatedFrameOfFrustumCheckPlanes != currentFrame ||
				LastCalculatedCameraOfFrustumCheckPlanes != camera)
			{
				GeometryUtility.CalculateFrustumPlanes(camera, FrustumCheckPlanes);
				LastCalculatedFrameOfFrustumCheckPlanes = currentFrame;
				LastCalculatedCameraOfFrustumCheckPlanes = camera;
			}

			return GeometryUtility.TestPlanesAABB(FrustumCheckPlanes, bounds);
		}

		public static bool IsInsideFrustum(this Camera camera, Bounds bounds, Plane[] preCalculatedPlanes)
		{
			return GeometryUtility.TestPlanesAABB(preCalculatedPlanes, bounds);
		}

		#endregion
	}

}

#endif
