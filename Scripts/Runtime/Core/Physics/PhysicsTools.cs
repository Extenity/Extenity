#if UNITY_5_3_OR_NEWER && PACKAGE_PHYSICS

using System;
using UnityEngine;

namespace Extenity.PhysicsToolbox
{

	public static class PhysicsTools
	{
		#region Raycast Smart

		private static RaycastHit[] HitInfos = new RaycastHit[30];

		public static bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance, int layerMask, Transform excludedObject)
		{
			return Raycast(Physics.defaultPhysicsScene, ray, out hitInfo, maxDistance, layerMask, excludedObject);
		}

		public static bool Raycast(PhysicsScene scene, Ray ray, out RaycastHit hitInfo, float maxDistance, int layerMask, Transform excludedObject)
		{
			var hitCount = scene.Raycast(ray.origin, ray.direction, HitInfos, maxDistance, layerMask);

			// Enlarge the buffer if there are lots of objects around. It requires rerunning the raycast unfortunately.
			while (hitCount >= HitInfos.Length)
			{
				Array.Resize(ref HitInfos, HitInfos.Length * 2);
				hitCount = scene.Raycast(ray.origin, ray.direction, HitInfos, maxDistance, layerMask);
			}

			if (hitCount <= 0)
			{
				hitInfo = default;
				return false;
			}

			// Find the closest hit but skip hits to 'excludedObject'.
			var minDistance = float.MaxValue;
			var minDistanceIndex = -1;
			for (int i = 0; i < hitCount; i++)
			{
				var distance = HitInfos[i].distance;
				if (minDistance > distance &&
				    HitInfos[i].transform != excludedObject) // Skip hits to self
				{
					minDistance = distance;
					minDistanceIndex = i;
				}
			}

			if (minDistanceIndex >= 0)
			{
				hitInfo = HitInfos[minDistanceIndex];
				return true;
			}
			else
			{
				hitInfo = default;
				return false;
			}
		}

		#endregion
	}

}

#endif
