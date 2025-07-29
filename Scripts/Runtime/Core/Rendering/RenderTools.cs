#if UNITY_5_3_OR_NEWER

using UnityEngine;

namespace Extenity.RenderingToolbox
{

	public static class RenderTools
	{
		#region Projection

		public static void SetScissorRect(this Camera cam, Rect r)
		{
			if (r.x < 0)
			{
				r.width += r.x;
				r.x = 0;
			}

			if (r.y < 0)
			{
				r.height += r.y;
				r.y = 0;
			}

			r.width = Mathf.Min(1 - r.x, r.width);
			r.height = Mathf.Min(1 - r.y, r.height);
			//print( r );

			cam.rect = new Rect(0, 0, 1, 1);
			cam.ResetProjectionMatrix();
			var m = cam.projectionMatrix;
			//print( cam.projectionMatrix );
			//print( Mathf.Rad2Deg * Mathf.Atan( 1 / cam.projectionMatrix[ 0 ] ) * 2 );
			cam.rect = r;
			//cam.projectionMatrix = m;
			//print( cam.projectionMatrix );		
			//print( Mathf.Rad2Deg * Mathf.Atan( 1 / cam.projectionMatrix[ 0 ] ) * 2 );
			//print( cam.fieldOfView );
			//print( Mathf.Tan( cam.projectionMatrix[ 1, 1 ] ) * 2 );
			//cam.pixelRect = new Rect( 0, 0, Screen.width / 2, Screen.height );
			//Matrix4x4 m1 = Matrix4x4.TRS(new Vector3(r.x, r.y, 0), Quaternion.identity, new Vector3(r.width, r.height, 1));
			//Matrix4x4 m1 = Matrix4x4.TRS( Vector3.zero, Quaternion.identity, new Vector3( r.width, r.height, 1 ) );
			//Matrix4x4 m2 = m1.inverse;
			//print( m2 );
			var m2 = Matrix4x4.TRS(new Vector3((1 / r.width - 1), (1 / r.height - 1), 0), Quaternion.identity, new Vector3(1 / r.width, 1 / r.height, 1));
			var m3 = Matrix4x4.TRS(new Vector3(-r.x * 2 / r.width, -r.y * 2 / r.height, 0), Quaternion.identity, Vector3.one);
			//m2[ 0, 3 ] = r.x;
			//m2[ 1, 3 ] = r.y;
			//print( m3 );
			//print( cam.projectionMatrix );
			cam.projectionMatrix = m3 * m2 * m;
			//print( cam.projectionMatrix );		
		}

		#endregion
	}

}

#endif
