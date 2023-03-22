#if UNITY

#if !DisableUnityAI

using System;
using System.Collections.Generic;
using Extenity.MathToolbox;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Extenity.NavigationToolbox
{

	public static class NavigationTools
	{
		public static int DrawPath(this NavMeshPath path, LineRenderer lineRenderer, ref Vector3[] buffer, int maxBufferExtensionSize = 100)
		{
			if (!lineRenderer)
				throw new ArgumentNullException(nameof(lineRenderer));

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
				//Log.Info($"Extending the buffer size from: {size}");
				while (size < maxBufferExtensionSize)
				{
					size = size == 0 ? 8 : Mathf.Min(size * 2, maxBufferExtensionSize);
					//Log.Info($"New buffer size: {size}");
					buffer = new Vector3[size];
					count = path.GetCornersNonAlloc(buffer);
					if (count < buffer.Length)
						return count;
				}
			}
			//Log.Info($"Buffer size: {buffer.Length}    count: {count}");
			return count;
		}

		public static int GetCornersNonAllocDynamic(this NavMeshPath path, ref Vector3[] buffer, ref Vector2[] bufferXZ, int maxBufferExtensionSize = 100)
		{
			var count = path.GetCornersNonAlloc(buffer);
			if (count == buffer.Length)
			{
				var size = buffer.Length;
				//Log.Info($"Extending the buffer size from: {size}");
				while (size < maxBufferExtensionSize)
				{
					size = size == 0 ? 8 : Mathf.Min(size * 2, maxBufferExtensionSize);
					//Log.Info($"New buffer size: {size}");
					buffer = new Vector3[size];
					count = path.GetCornersNonAlloc(buffer);
					if (count < buffer.Length)
					{
						// Copy to 2D buffer
						bufferXZ = new Vector2[size];
						for (var i = 0; i < count; i++)
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
			for (var i = 0; i < count; i++)
			{
				var item = buffer[i];
				bufferXZ[i].x = item.x;
				bufferXZ[i].y = item.z;
			}

			//Log.Info($"Buffer size: {buffer.Length}    count: {count}");
			return count;
		}

		#endregion

		#region Length

		/// <summary>
		/// Calculates total length of the path. Consider using 'buffer.CalculateLineStripLength' if you already have the path corners.
		/// </summary>
		public static float CalculateTotalLength(this NavMeshPath path, ref Vector3[] buffer, int maxBufferExtensionSize = 100)
		{
			// Reimplement this whenever needed. Need a way to cast Vector3[] to IList<float3>.
			throw new NotImplementedException();
			// var count = path.GetCornersNonAllocDynamic(ref buffer, maxBufferExtensionSize);
			// return buffer.CalculateLineStripLength(0, count);
		}

		#endregion

		#region Layers

		public static LayerMask GetAreaLayerMask(string areaName)
		{
			var index = NavMesh.GetAreaFromName(areaName);
			if (index < 0)
			{
				Log.Fatal($"Navigation area '{areaName}' does not exist.");
				return 0;
			}
			return 1 << index;
		}

		#endregion

		#region Agent

		public static int GetAgentTypeID(string agentTypeName)
		{
			var settingsCount = NavMesh.GetSettingsCount();
			for (int i = 0; i < settingsCount; ++i)
			{
				var settings = NavMesh.GetSettingsByIndex(i);

				var agentTypeID = settings.agentTypeID;
				var settingsName = NavMesh.GetSettingsNameFromID(agentTypeID);

				if (settingsName.Equals(agentTypeName, StringComparison.Ordinal))
				{
					return agentTypeID;
				}
			}
			Log.Fatal($"Agent type '{agentTypeName}' does not exist.");
			return -1;
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(NavigationTools));

		#endregion
	}

}

#endif

#endif
