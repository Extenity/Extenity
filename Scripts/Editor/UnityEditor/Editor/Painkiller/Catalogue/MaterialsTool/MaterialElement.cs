using System;
using System.Linq;
using Extenity.RenderingToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.PainkillerToolbox.Editor
{

	[Serializable]
	public class MaterialElement : CatalogueElement<MaterialElement>
	{
		#region Initialization

		public MaterialElement(Material material, string sceneName, MaterialElement parentElement)
			: base(material, parentElement.depth + 1)
		{
			Material = material;
			if (Material)
			{
				var textures = Material.GetAllTextures();
				var largestTexture = textures.Count > 0 ? textures.First(texture => texture.width * texture.height == textures.Max(comp => comp.width * comp.height)) : null;
				TextureCount = textures.Count;
				MaxTextureSize = largestTexture ? new Vector2Int(largestTexture.width, largestTexture.height) : Vector2Int.zero;
				AssetPath = AssetDatabase.GetAssetPath(Material);
			}
			FoundInScenes = new[] { sceneName };
		}

		public MaterialElement()
		{
			// Parameterless constructor is only used for creating the tree root element. Don't do anything here, unless you want to do it for the root element.
		}

		#endregion

		#region Material

		public Material Material;
		public string ShaderName { get { return Material && Material.shader ? Material.shader.name : ""; } }
		public int TextureCount;
		public Vector2Int MaxTextureSize;
		public bool IsInstanced { get { return Material ? Material.enableInstancing : false; } }
		public string AssetPath;

		#endregion
	}

}
