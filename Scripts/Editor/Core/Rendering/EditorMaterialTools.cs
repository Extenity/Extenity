using System.Collections.Generic;
using UnityEngine;

namespace Extenity.RenderingToolbox.Editor
{

	public static class EditorMaterialTools
	{
		#region Texture

		public static List<Texture> GetAllTextures(this Material material)
		{
			var result = new List<Texture>();
			var shader = material.shader;
#if UNITY_6000_2_OR_NEWER
			var propertyCount = shader.GetPropertyCount();
#else
			var propertyCount = UnityEditor.ShaderUtil.GetPropertyCount(shader);
#endif
			for (int i = 0; i < propertyCount; i++)
			{
#if UNITY_6000_2_OR_NEWER
				if (shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Texture)
#else
				if (UnityEditor.ShaderUtil.GetPropertyType(shader, i) == UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv)
#endif
				{
#if UNITY_6000_2_OR_NEWER
					var propertyName = shader.GetPropertyName(i);
#else
					var propertyName = UnityEditor.ShaderUtil.GetPropertyName(shader, i);
#endif
					var texture = material.GetTexture(propertyName);
					if (texture)
					{
						result.Add(texture);
					}
				}
			}
			return result;
		}

		#endregion

	}

}
