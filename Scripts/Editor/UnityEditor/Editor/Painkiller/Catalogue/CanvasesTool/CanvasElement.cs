using System;
using Extenity.AssetToolbox.Editor;
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
				AssetPath = AssetTools.GetAssetPathWithoutRoot(Canvas);
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
		public int RenderOrder => Canvas.renderOrder;
		public string AssetPath;

		#endregion
	}

}