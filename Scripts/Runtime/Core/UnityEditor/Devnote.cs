#if UNITY

using UnityEngine;

namespace Extenity.UnityEditorToolbox
{

	public class Devnote : MonoBehaviour
	{
		public string Note;

		private Texture2D TEX;
		private bool TEXCreated;
		public Texture2D Asd;

		//private void Awake()
		//{
		//	TEX = TextureToolbox.TextureTools.CreateSimpleTexture(16, 16, Color.green);
		//	Log.Info("TEX: " + TEX);
		//}

		private void OnDrawGizmos()
		{
			//if (TEX == null)
			//{
			//	TEX = TextureToolbox.TextureTools.CreateSimpleTexture(16, 16, Color.green);
			//	Log.Info("TEX: " + TEX);
			//}
			if (!TEXCreated)
			{
				TEXCreated = true;
				TEX = TextureToolbox.TextureTools.CreateSimpleTexture(Color.green);
				Log.Info("TEX: " + TEX);
			}

			var screenPosition = Camera.current.WorldToScreenPoint(transform.position);
			screenPosition.y = Screen.height - screenPosition.y;
			//var width = (float)Screen.width;
			//var height = (float)Screen.height;
			var rect = new Rect(screenPosition.x, screenPosition.y, 50, 50);
			//var rect = new Rect(screenPosition.x / width, screenPosition.y / height, 50 / width, 50 / height);
			//Log.Info("rect :" + rect);
			Gizmos.DrawGUITexture(rect, Asd);
			Gizmos.DrawGUITexture(new Rect(0.25f, 0, 100, 100), Asd);
			//Gizmos.DrawCube(transform.position, Vector3.one);
		}
	}

}

#endif
