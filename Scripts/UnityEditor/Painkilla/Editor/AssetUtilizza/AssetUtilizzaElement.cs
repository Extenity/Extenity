using System;
using System.Linq;
using Extenity.AssetToolbox.Editor;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.RenderingToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.PainkillaTool.Editor
{

	[Serializable]
	public class AssetUtilizzaElement : TreeElement
	{
		#region Initialization

		public AssetUtilizzaElement(Material material, string sceneName) : base(material.name, 0, material.GetInstanceID())
		{
			Material = material;
			if (Material)
			{
				ShaderName = Material.shader.name;
				var textures = Material.GetAllTextures();
				var largestTexture = textures.Count > 0 ? textures.First(texture => texture.width * texture.height == textures.Max(comp => comp.width * comp.height)) : null;
				TextureCount = textures.Count;
				MaxTextureSize = largestTexture ? new Vector2Int(largestTexture.width, largestTexture.height) : Vector2Int.zero;
				AssetPath = AssetTools.GetAssetPathWithoutRoot(Material);
			}
			FoundInScenes = new[] { sceneName };
		}

		private AssetUtilizzaElement() : base(null, -1, 0)
		{
		}

		public static AssetUtilizzaElement CreateRoot()
		{
			return new AssetUtilizzaElement();
		}

		#endregion

		#region Material

		public Material Material;
		public string ShaderName;
		public int TextureCount;
		public Vector2Int MaxTextureSize;
		public bool IsInstanced { get { return Material ? Material.enableInstancing : false; } }
		public string AssetPath;

		#endregion

		#region Preview

		private Texture2D _Preview;
		public Texture2D Preview
		{
			get
			{
				if (!_Preview)
				{
					_Preview = AssetPreview.GetAssetPreview(Material);
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
