#if UNITY_5_3_OR_NEWER

using UnityEngine;
using static Unity.Mathematics.math;

namespace Extenity.AnimationToolbox
{

	public class ColorBlinker : MonoBehaviour
	{
		public float FadeDuration = 0.5f;
		public Color Color1 = Color.gray;
		public Color Color2 = Color.white;

		private Color StartColor;
		private Color EndColor;
		private float LastColorChangeTime;

		private Material Material;

		protected void Start()
		{
			Material = GetComponent<Renderer>().material;
			StartColor = Color1;
			EndColor = Color2;
		}

		protected void Update()
		{
			var ratio = (Time.time - LastColorChangeTime) / FadeDuration;
			ratio = Mathf.Clamp01(ratio);
			Material.color = Color.Lerp(StartColor, EndColor, sqrt(ratio));
			//material.color = Color.Lerp(startColor, endColor, ratio * ratio);
			//material.color = Color.Lerp(startColor, endColor, ratio);

			if (ratio == 1f)
			{
				LastColorChangeTime = Time.time;

				// Switch colors
				var temp = StartColor;
				StartColor = EndColor;
				EndColor = temp;
			}
		}
	}

}

#endif
