using UnityEngine;

namespace Extenity.JsonToolbox.Converters
{

	/// <summary>
	/// Custom <c>Newtonsoft.Json.JsonConverter</c> for <c>UnityEngine.Vector2</c>.
	/// Source: wanzyeestudio.blogspot.com
	/// </summary>
	public class Vector2Converter : PartialConverter<Vector2>
	{
		/// <summary>
		/// Get the property names include <c>x</c>, <c>y</c>.
		/// </summary>
		/// <returns>The property names.</returns>
		protected override string[] GetPropertyNames()
		{
			return new[] { "x", "y" };
		}
	}

}
