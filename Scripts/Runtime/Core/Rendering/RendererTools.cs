#if UNITY

using System.Collections.Generic;
using UnityEngine;

namespace Extenity.RenderingToolbox
{

	public static class RendererTools
	{
		public static void SetSharedMaterials(this Renderer renderer, List<Material> materials)
		{
			if (materials == null || materials.Count == 0)
			{
				renderer.sharedMaterial = null;
			}
			else if (materials.Count == 1)
			{
				renderer.sharedMaterial = materials[0];
			}
			else
			{
				renderer.sharedMaterials = materials.ToArray();
			}
		}
	}

}

#endif
