using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Extenity.MathToolbox
{

	/// <summary>
	///   <para>Represents an axis aligned 2D bounding box.</para>
	/// </summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Bounds2
	{
		[SerializeField]
		private Vector2 m_Center;
		[SerializeField]
		private Vector2 m_Extents;

		/// <summary>
		///   <para>Creates a new Bounds.</para>
		/// </summary>
		/// <param name="center">The location of the origin of the Bounds.</param>
		/// <param name="size">The dimensions of the Bounds.</param>
		public Bounds2(Vector2 center, Vector2 size)
		{
			m_Center = center;
			m_Extents = size * 0.5F;
		}

		public override int GetHashCode()
		{
			return center.GetHashCode() ^ (extents.GetHashCode() << 2);
		}

		public override bool Equals(object other)
		{
			if (!(other is Bounds2))
				return false;
			Bounds2 rhs = (Bounds2)other;
			return center.Equals(rhs.center) && extents.Equals(rhs.extents);
		}

		/// <summary>
		///   <para>The center of the bounding box.</para>
		/// </summary>
		public Vector2 center { get { return m_Center; } set { m_Center = value; } }

		/// <summary>
		///   <para>The total size of the box. This is always twice as large as the extents.</para>
		/// </summary>
		public Vector2 size { get { return m_Extents * 2.0F; } set { m_Extents = value * 0.5F; } }

		/// <summary>
		///   <para>The extents of the Bounding Box. This is always half of the size of the Bounds.</para>
		/// </summary>
		public Vector2 extents { get { return m_Extents; } set { m_Extents = value; } }

		/// <summary>
		///   <para>The minimal point of the box. This is always equal to center-extents.</para>
		/// </summary>
		public Vector2 min { get { return center - extents; } set { SetMinMax(value, max); } }

		/// <summary>
		///   <para>The maximal point of the box. This is always equal to center+extents.</para>
		/// </summary>
		public Vector2 max { get { return center + extents; } set { SetMinMax(min, value); } }

		public static bool operator ==(Bounds2 lhs, Bounds2 rhs)
		{
			// Returns false in the presence of NaN values.
			return (lhs.center == rhs.center && lhs.extents == rhs.extents);
		}

		public static bool operator !=(Bounds2 lhs, Bounds2 rhs)
		{
			// Returns true in the presence of NaN values.
			return !(lhs == rhs);
		}

		/// <summary>
		///   <para>Sets the bounds to the min and max value of the box.</para>
		/// </summary>
		public void SetMinMax(Vector2 min, Vector2 max)
		{
			extents = (max - min) * 0.5F;
			center = min + extents;
		}

		/// <summary>
		///   <para>Grows the Bounds to include the point.</para>
		/// </summary>
		public void Encapsulate(Vector2 point)
		{
			SetMinMax(Vector2.Min(min, point), Vector2.Max(max, point));
		}

		/// <summary>
		///   <para>Grow the bounds to encapsulate the bounds.</para>
		/// </summary>
		public void Encapsulate(Bounds2 bounds)
		{
			Encapsulate(bounds.center - bounds.extents);
			Encapsulate(bounds.center + bounds.extents);
		}

		/// <summary>
		///   <para>Expand the bounds by increasing its size by amount along each side.</para>
		/// </summary>
		public void Expand(float amount)
		{
			amount *= .5f;
			extents += new Vector2(amount, amount);
		}

		/// <summary>
		///   <para>Expand the bounds by increasing its size by amount along each side.</para>
		/// </summary>
		public void Expand(Vector2 amount)
		{
			extents += amount * .5f;
		}

		/// <summary>
		///   <para>Does another bounding box intersect with this bounding box?</para>
		/// </summary>
		public bool Intersects(Bounds2 bounds)
		{
			return (min.x <= bounds.max.x) && (max.x >= bounds.min.x) &&
				(min.y <= bounds.max.y) && (max.y >= bounds.min.y);
		}

		/// <summary>
		///   <para>Does another bounding box intersect with this bounding box?</para>
		/// </summary>
		public bool IntersectRay(Ray ray)
		{
			throw new NotImplementedException();
			//float dist;
			//return IntersectRayAABB(ray, this, out dist);
		}

		/// <summary>
		///   <para>Does ray intersect this bounding box?</para>
		/// </summary>
		public bool IntersectRay(Ray ray, out float distance)
		{
			throw new NotImplementedException();
			//return IntersectRayAABB(ray, this, out distance);
		}


		/// <summary>
		///   <para>Returns a nicely formatted string for the bounds.</para>
		/// </summary>
		public override string ToString()
		{
			return $"Center: {m_Center}, Extents: {m_Extents}";
		}

		/// <summary>
		///   <para>Returns a nicely formatted string for the bounds.</para>
		/// </summary>
		public string ToString(string format)
		{
			return $"Center: {m_Center.ToString(format)}, Extents: {m_Extents.ToString(format)}";
		}

		/// <summary>
		///   <para>Is point contained in the bounding box?</para>
		/// </summary>
		public bool Contains(Vector2 point, float tolerance = 0.0001f)
		{
			return point.x > center.x - extents.x - tolerance &&
			       point.x < center.x + extents.x + tolerance &&
			       point.y > center.y - extents.y - tolerance &&
			       point.y < center.y + extents.y + tolerance;
			//return Bounds.Contains_Injected(ref this, ref point);
		}
		
		/// <summary>
		///   <para>The smallest squared distance between the point and this bounding box.</para>
		/// </summary>
		public float SqrDistance(Vector2 point)
		{
			throw new NotImplementedException();
			//return Bounds.SqrDistance_Injected(ref this, ref point);
		}

		/// <summary>
		///   <para>The closest point on the bounding box.</para>
		/// </summary>
		/// <param name="point">Arbitrary point.</param>
		/// <returns>
		///   <para>The point on the bounding box or inside the bounding box.</para>
		/// </returns>
		public Vector2 ClosestPoint(Vector2 point)
		{
			throw new NotImplementedException();
			//Vector2 ret;
			//Bounds.ClosestPoint_Injected(ref this, ref point, out ret);
			//return ret;
		}
	}

}
