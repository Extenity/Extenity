using UnityEngine;

namespace Extenity.Coloring
{

	public interface IColorScale
	{
		float MinimumValue { get; }
		float MaximumValue { get; }

		Color32 GetColor32(float scalePoint);
	}

}
