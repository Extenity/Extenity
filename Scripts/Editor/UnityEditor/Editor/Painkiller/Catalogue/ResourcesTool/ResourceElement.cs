using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Extenity.PainkillerToolbox.Editor
{

	[Serializable]
	public class ResourceElement : CatalogueElement<ResourceElement>
	{
		#region Initialization

		public ResourceElement(Object asset, string assetPath, string resourcePath, ResourceElement parentElement)
			: base(asset, parentElement.depth + 1)
		{
			//Asset = asset; Already exists in CatalogueElement
			if (Asset)
			{
				FullPath = assetPath;
				ResourcePath = resourcePath;
				AssetType = AssetDatabase.GetMainAssetTypeAtPath(FullPath).Name;
			}
		}

		public ResourceElement()
		{
			// Parameterless constructor is only used for creating the tree root element. Don't do anything here, unless you want to do it for the root element.
		}

		#endregion

		#region Resource

		//public readonly Object Asset; Already exists in CatalogueElement
		public readonly string AssetType;
		public readonly string ResourcePath;
		public readonly string FullPath;

		#endregion
	}

}
