using System.Collections.Generic;
using UnityEngine;
using Extenity.ColoringToolbox;
using Extenity.IMGUIToolbox;

namespace ExtenityExamples.IMGUIToolbox
{

	public class Example_GUIBars : MonoBehaviour
	{
		public Rect Rect = new Rect(20f, 20f, 200f, 80f);
		public float EditorHeight = 220f;

		public float[] BarValues =
		{
			0.2f,
			0.5f,
			0.3f,
			0.6f,
			1.0f,
			0.9f,
			0.1f,
			0.0f,
			0.3f,
			1.1f,
			-0.1f,
		};

		public GradientColorScale BarColorScale = new GradientColorScale(new List<ColorStop>
		{
			new ColorStop(0.0f, new Color32(134, 221, 18, 255)),
			new ColorStop(0.6f, new Color32(238, 225, 51, 255)),
			new ColorStop(1.0f, new Color32(242, 86, 68, 255)),
		});
		public SingleColorScale BarBackgroundColorScale = new SingleColorScale(new Color32(71, 73, 75, 255));

		private void OnGUI()
		{
			GUITools.Bars(Rect, 5f, true, BarColorScale, BarBackgroundColorScale, BarValues.Length, index => BarValues[index]);
		}
	}

}
