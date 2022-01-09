#if UNITY

using UnityEngine;
using static Unity.Mathematics.math;

namespace Extenity.AnimationToolbox
{

	public class ColorBlinker : MonoBehaviour
	{
		public float FadeDuration = 0.5f;
		public Color Color1 = Color.gray;
		public Color Color2 = Color.white;

		private Color startColor;
		private Color endColor;
		private float lastColorChangeTime;

		private Material material;

		protected void Start()
		{
			material = GetComponent<Renderer>().material;
			startColor = Color1;
			endColor = Color2;
		}

		protected void Update()
		{
			var ratio = (Time.time - lastColorChangeTime) / FadeDuration;
			ratio = Mathf.Clamp01(ratio);
			material.color = Color.Lerp(startColor, endColor, sqrt(ratio));
			//material.color = Color.Lerp(startColor, endColor, ratio * ratio);
			//material.color = Color.Lerp(startColor, endColor, ratio);

			if (ratio == 1f)
			{
				lastColorChangeTime = Time.time;

				// Switch colors
				var temp = startColor;
				startColor = endColor;
				endColor = temp;
			}
		}
	}

}

#endif
