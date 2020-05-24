using UnityEngine;

namespace Extenity.JsonToolbox.Converters
{

	/// <summary>
	/// Custom <c>Newtonsoft.Json.JsonConverter</c> for <c>UnityEngine.Rect</c>.
	/// Source: wanzyeestudio.blogspot.com
	/// </summary>
	public class RectConverter : PartialConverter<Rect>
	{
		/// <summary>
		/// Get the property names include <c>x</c>, <c>y</c>, <c>width</c>, <c>height</c>.
		/// </summary>
		/// <returns>The property names.</returns>
		protected override string[] GetPropertyNames()
		{
			return new[] { "x", "y", "width", "height" };
		}
	}

}
