using System.Collections.Generic;
using UnityEditor;
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
			var propertyCount = ShaderUtil.GetPropertyCount(shader);
			for (int i = 0; i < propertyCount; i++)
			{
				if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
				{
					var propertyName = ShaderUtil.GetPropertyName(shader, i);
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
