using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Extenity.Meshing
{

	[Serializable]
	public class TriangleGroup
	{
		#region Initialization

		public TriangleGroup()
		{
			Id = -1;
		}

		public TriangleGroup(int id)
		{
			Id = id;
		}

		#endregion

		#region Deinitialization

		public void Release()
		{
			Id = -1;

			if (TriangleStartingIndices != null)
			{
				TriangleStartingIndices.Clear();
				TriangleStartingIndices = null;
			}

			CustomData = null;
		}

		#endregion

		#region Id

		public int Id;

		#endregion

		#region Triangle Indices

		/// <summary>
		/// Group of triangles that keeps track of which triangles of the mesh is related with this group. A triangle can be a part of multiple groups.
		/// </summary>
		public List<int> TriangleStartingIndices;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="triangleStartingIndex"></param>
		/// <returns>True if succeeds</returns>
		public bool AddTriangleStartingIndex(int triangleStartingIndex)
		{
			if (TriangleStartingIndices == null)
			{
				TriangleStartingIndices = new List<int>();
			}

			// Silently ignore nonvalid index
			if (triangleStartingIndex < 0)
				return false;

			TriangleStartingIndices.Add(triangleStartingIndex);
			return true;
		}

		#endregion

		#region Custom Data

		/// <summary>
		/// Custom user data in case the user needs to associate some data with this triangle group.
		/// </summary>
		public object CustomData;

		public void SetCustomDataEnsuredNull(object data)
		{
			if (CustomData != null)
			{
				throw new Exception("Custom data was not null for triangle group.");
			}

			CustomData = data;
		}

		#endregion

		#region Get

		/// <summary>
		/// For performance reasons, this method does not check triangle index consistency like array bound checks or multiple of 3 checks.
		/// </summary>
		/// <param name="triangleIndex">Should be multiples of 3.</param>
		/// <returns></returns>
		public bool HasTriangle(int triangleIndex)
		{
			for (int i = 0; i < TriangleStartingIndices.Count; i++)
			{
				if (triangleIndex == TriangleStartingIndices[i])
					return true;
			}
			return false;
		}

		#endregion
	}

}
