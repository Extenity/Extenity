using System;
using System.Linq;
using Extenity.AssetToolbox.Editor;
using Extenity.RenderingToolbox.Editor;
using UnityEngine;

namespace Extenity.PainkillaTool.Editor
{

	[Serializable]
	public class MaterialElement : AssetUtilizzaElement<MaterialElement>
	{
		#region Initialization

		public MaterialElement(Material material, string sceneName) 
			: base(material)
		{
			Material = material;
			if (Material)
			{
				var textures = Material.GetAllTextures();
				var largestTexture = textures.Count > 0 ? textures.First(texture => texture.width * texture.height == textures.Max(comp => comp.width * comp.height)) : null;
				TextureCount = textures.Count;
				MaxTextureSize = largestTexture ? new Vector2Int(largestTexture.width, largestTexture.height) : Vector2Int.zero;
				AssetPath = AssetTools.GetAssetPathWithoutRoot(Material);
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
