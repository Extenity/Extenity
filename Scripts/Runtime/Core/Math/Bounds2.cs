using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Extenity.MathToolbox
{

	/// <summary>
	///   <para>Represents an axis aligned 2D bounding box.</para>
	/// </summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Bounds2
	{
#if UNITY
		[UnityEngine.SerializeField]
#endif
		private float2 m_Center;
#if UNITY
		[UnityEngine.SerializeField]
#endif
		private float2 m_Extents;

		/// <summary>
		///   <para>Creates a new Bounds.</para>
		/// </summary>
		/// <param name="center">The location of the origin of the Bounds.</param>
		/// <param name="size">The dimensions of the Bounds.</param>
		public Bounds2(float2 center, float2 size)
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
		public float2 center { get { return m_Center; } set { m_Center = value; } }

		/// <summary>
		///   <para>The total size of the box. This is always twice as large as the extents.</para>
		/// </summary>
		public float2 size { get { return m_Extents * 2.0F; } set { m_Extents = value * 0.5F; } }

		/// <summary>
		///   <para>The extents of the Bounding Box. This is always half of the size of the Bounds.</para>
		/// </summary>
		public float2 extents { get { return m_Extents; } set { m_Extents = value; } }

		/// <summary>
		///   <para>The minimal point of the box. This is always equal to center-extents.</para>
		/// </summary>
		public float2 min { get { return center - extents; } set { SetMinMax(value, max); } }

		/// <summary>
		///   <para>The maximal point of the box. This is always equal to center+extents.</para>
		/// </summary>
		public float2 max { get { return center + extents; } set { SetMinMax(min, value); } }

		public static bool operator ==(Bounds2 lhs, Bounds2 rhs)
		{
			// Returns false in the presence of NaN values.
			return all(lhs.center == rhs.center) && all(lhs.extents == rhs.extents);
		}

		public static bool operator !=(Bounds2 lhs, Bounds2 rhs)
		{
			// Returns true in the presence of NaN values.
			return !(lhs == rhs);
		}

		/// <summary>
		///   <para>Sets the bounds to the min and max value of the box.</para>
		/// </summary>
		public void SetMinMax(float2 min, float2 max)
		{
			extents = (max - min) * 0.5F;
			center = min + extents;
		}

		/// <summary>
		///   <para>Grows the Bounds to include the point.</para>
		/// </summary>
		public void Encapsulate(float2 point)
		{
			SetMinMax(min(min, point), max(max, point));
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
			extents += new float2(amount, amount);
		}

		/// <summary>
		///   <para>Expand the bounds by increasing its size by amount along each side.</para>
		/// </summary>
		public void Expand(float2 amount)
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
        ///   <para>Does ray intersect this bounding box?</para>
		/// </summary>
		public bool IntersectRay(UnityEngine.Ray ray)
		{
			throw new NotImplementedException();
			//float dist;
			//return IntersectRayAABB(ray, this, out dist);
		}

		/// <summary>
		///   <para>Does ray intersect this bounding box?</para>
		/// </summary>
		public bool IntersectRay(UnityEngine.Ray ray, out float distance)
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
		public string ToString(string format, IFormatProvider formatProvider)
		{
			return $"Center: {m_Center.ToString(format, formatProvider)}, Extents: {m_Extents.ToString(format, formatProvider)}";
		}

		/// <summary>
		///   <para>Is point contained in the bounding box?</para>
		/// </summary>
		public bool Contains(float2 point, float tolerance = 0.0001f)
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
		public float SqrDistance(float2 point)
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
		public float2 ClosestPoint(float2 point)
		{
			throw new NotImplementedException();
			//float2 ret;
			//Bounds.ClosestPoint_Injected(ref this, ref point, out ret);
			//return ret;
		}
	}

}
