using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extenity.GridPaintTool
{

	public class GridPaintBase : MonoBehaviour
	{
		#region Initialization

		//protected void Awake()
		//{
		//}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region Update

		//protected void Update()
		//{
		//}

		#endregion

		Vector3 GizmoPosition;
		Quaternion GizmoRotation;

		private void OnDrawGizmos()
		{
			var texture = GridPaintIcons.Texture_ArrowBent;
			Debug.Log("###");
			Gizmos.matrix = Matrix4x4.TRS(GizmoPosition, GizmoRotation, Vector3.one);
			Gizmos.DrawGUITexture(new Rect(0, 0, texture.width, texture.height), texture);
		}


	}

}
