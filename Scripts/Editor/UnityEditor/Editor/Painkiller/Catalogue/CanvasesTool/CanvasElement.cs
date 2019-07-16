using System;
using UnityEditor;
using UnityEngine;

namespace Extenity.PainkillerToolbox.Editor
{

	[Serializable]
	public class CanvasElement : CatalogueElement<CanvasElement>
	{
		#region Initialization

		public CanvasElement(Canvas canvas, string sceneName, CanvasElement parent)
			: base(canvas, parent.depth + 1)
		{
			Canvas = canvas;
			if (Canvas)
			{
				AssetPath = AssetDatabase.GetAssetPath(Canvas);
			}
			FoundInScenes = new[] { sceneName };
		}

		public CanvasElement()
		{
			// Parameterless constructor is only used for creating the tree root element. Don't do anything here, unless you want to do it for the root element.
		}

		#endregion

		#region Canvas

		public Canvas Canvas;
		public int SortingOrder => Canvas ? Canvas.sortingOrder : int.MinValue;
		public string AssetPath;

		#endregion
	}

}
