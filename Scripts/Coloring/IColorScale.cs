using UnityEngine;

namespace Extenity.ColoringToolbox
{

	public interface IColorScale
	{
		float MinimumValue { get; }
		float MaximumValue { get; }

		Color32 GetColor32(float scalePoint);
	}

}
