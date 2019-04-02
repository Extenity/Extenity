using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.PainkillerToolbox.Editor
{

	public abstract class CatalogueElement<TElement> : TreeElement
		where TElement : CatalogueElement<TElement>, new()
	{
		#region Initialization

		public CatalogueElement(Object asset)
			: base(asset.name, 0, asset.GetInstanceID())
		{
			Asset = asset;
		}

		public CatalogueElement()
			: base(null, -1, 0)
		{
			Asset = null;
		}

		public static TElement CreateRoot()
		{
			return new TElement();
		}

		#endregion

		#region Asset

		public readonly Object Asset;

		#endregion

		#region Preview

		private Texture2D _Preview;
		public Texture2D Preview
		{
			get
			{
				if (!_Preview)
				{
					_Preview = AssetPreview.GetAssetPreview(Asset);
				}
				return _Preview;
			}
		}

		#endregion

		#region Found In Scenes

		public string[] FoundInScenes;

		private string _FoundInScenesCombined;
		public string FoundInScenesCombined
		{
			get
			{
				if (string.IsNullOrEmpty(_FoundInScenesCombined) && FoundInScenes.Length > 0)
				{
					_FoundInScenesCombined = string.Join(", ", FoundInScenes);
				}
				return _FoundInScenesCombined;
			}
		}

		public void AddScene(string sceneName)
		{
			FoundInScenes = FoundInScenes.Add(sceneName);
		}

		#endregion
	}

}
