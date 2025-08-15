#if UNITY_5_3_OR_NEWER

using UnityEngine;
using UnityEngine.Rendering;

namespace Extenity.RenderingToolbox
{

	public static class MaterialTools
	{
		public static void SetMaterialToTransparentMode(this Material material)
		{
			material.SetFloat("_Mode", 2f);
			material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			material.SetInt("_ZWrite", 0);
			material.DisableKeyword("_ALPHATEST_ON");
			material.EnableKeyword("_ALPHABLEND_ON");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = 3000;
		}

		public static void SetAllTexturesOfMaterial(this Material material, Texture texture)
		{
			var shader = material.shader;
			var propertyCount = shader.GetPropertyCount();
			for (int i = 0; i < propertyCount; i++)
			{
				if (shader.GetPropertyType(i) == ShaderPropertyType.Texture)
				{
					var propertyName = shader.GetPropertyName(i);
					material.SetTexture(propertyName, texture);
				}
			}
		}
	}

}

#endif
