using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public static class ButtonExtensions
{
	// TODO: Move to UITools
	public static void SetNormalColorAlpha(this Button me, float value)
	{
		var colors = me.colors;
		colors.normalColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, value);
		me.colors = colors;
	}
}
