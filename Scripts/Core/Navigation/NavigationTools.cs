using System;
using Extenity.MathToolbox;
using UnityEngine;
using UnityEngine.AI;

namespace Extenity.NavigationToolbox
{

	public static class NavigationTools
	{
		public static int DrawPath(this NavMeshPath path, LineRenderer lineRenderer, ref Vector3[] buffer, int maxBufferExtensionSize = 100)
		{
			if (!lineRenderer)
				throw new ArgumentNullException("lineRenderer");

			var count = path.GetCornersNonAllocDynamic(ref buffer, maxBufferExtensionSize);

			if (count < 2)
			{
				lineRenderer.positionCount = 0;
			}
			else
			{
				lineRenderer.positionCount = count;
				lineRenderer.SetPositions(buffer);
			}

			return count;
		}

		public static void DrawDebugPath(this NavMeshPath path, Color color, float duration = 0f, bool depthTest = false)
		{
			var corners = path.corners;
			for (int i = 0; i < corners.Length - 1; i++)
				Debug.DrawLine(corners[i], corners[i + 1], color, duration, depthTest);
		}

		#region Get Corners

		public static int GetCornersNonAllocDynamic(this NavMeshPath path, ref Vector3[] buffer, int maxBufferExtensionSize = 100)
		{
			var count = path.GetCornersNonAlloc(buffer);
			if (count == buffer.Length)
			{
				var size = buffer.Length;
				//Debug.LogFormat("Extending the buffer size from: {0}", size);
				while (size < maxBufferExtensionSize)
				{
					size = size == 0 ? 8 : Mathf.Min(size * 2, maxBufferExtensionSize);
					//Debug.LogFormat("New buffer size: {0}", size);
					buffer = new Vector3[size];
					count = path.GetCornersNonAlloc(buffer);
					if (count < buffer.Length)
						return count;
				}
			}
			//Debug.LogFormat("Buffer size: {0}    count: {1}", buffer.Length, count);
			return count;
		}

		public static int GetCornersNonAllocDynamic(this NavMeshPath path, ref Vector3[] buffer, ref Vector2[] bufferXZ, int maxBufferExtensionSize = 100)
		{
			var count = path.GetCornersNonAlloc(buffer);
			if (count == buffer.Length)
			{
				var size = buffer.Length;
				//Debug.LogFormat("Extending the buffer size from: {0}", size);
				while (size < maxBufferExtensionSize)
				{
					size = size == 0 ? 8 : Mathf.Min(size * 2, maxBufferExtensionSize);
					//Debug.LogFormat("New buffer size: {0}", size);
					buffer = new Vector3[size];
					count = path.GetCornersNonAlloc(buffer);
					if (count < buffer.Length)
					{
						// Copy to 2D buffer
						bufferXZ = new Vector2[size];
						for (var i = 0; i < buffer.Length; i++)
						{
							var item = buffer[i];
							bufferXZ[i].x = item.x;
							bufferXZ[i].y = item.z;
						}

						return count;
					}
				}
			}

			// Copy to 2D buffer
			bufferXZ = new Vector2[count];
			for (var i = 0; i < buffer.Length; i++)
			{
				var item = buffer[i];
				bufferXZ[i].x = item.x;
				bufferXZ[i].y = item.z;
			}

			//Debug.LogFormat("Buffer size: {0}    count: {1}", buffer.Length, count);
			return count;
		}

		#endregion

		#region Length

		/// <summary>
		/// Calculates total length of the path. Consider using 'buffer.CalculateLineStripLength' if you already have the path corners.
		/// </summary>
		public static float CalculateTotalLength(this NavMeshPath path, ref Vector3[] buffer, int maxBufferExtensionSize = 100)
		{
			var count = path.GetCornersNonAllocDynamic(ref buffer, maxBufferExtensionSize);
			return buffer.CalculateLineStripLength(0, count);
		}

		#endregion
	}

}
