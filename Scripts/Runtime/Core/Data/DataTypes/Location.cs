using UnityEngine;

namespace Extenity.DataToolbox
{

	public struct Location
	{
		public Vector3 Position;
		public Quaternion Rotation;

		public static readonly Location NaN = new Location(new Vector3(float.NaN, float.NaN, float.NaN), new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN));

		public Location(Vector3 position, Quaternion rotation)
		{
			Position = position;
			Rotation = rotation;
		}

		public void SetUsingTransformWorld(Transform transform)
		{
			Position = transform.position;
			Rotation = transform.rotation;
		}

		public void SetUsingTransformLocal(Transform transform)
		{
			Position = transform.localPosition;
			Rotation = transform.localRotation;
		}
	}

	public static class LocationExt
	{
		public static Location ToWorldLocation(this Transform transform)
		{
			return new Location(transform.position, transform.rotation);
		}

		public static Location ToLocalLocation(this Transform transform)
		{
			return new Location(transform.localPosition, transform.localRotation);
		}

		public static void SetWorldLocation(this Transform transform, Location location)
		{
			transform.position = location.Position;
			transform.rotation = location.Rotation;
		}

		public static void SetWorldLocation(this Transform transform, Vector3 position, Quaternion rotation)
		{
			transform.position = position;
			transform.rotation = rotation;
		}

		public static void SetLocalLocation(this Transform transform, Location location)
		{
			transform.localPosition = location.Position;
			transform.localRotation = location.Rotation;
		}

		public static void SetLocalLocation(this Transform transform, Vector3 position, Quaternion rotation)
		{
			transform.localPosition = position;
			transform.localRotation = rotation;
		}

		public static void SetLocalLocation(this Transform transform, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			transform.localPosition = position;
			transform.localRotation = rotation;
			transform.localScale = scale;
		}
	}

}
