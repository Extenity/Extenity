#if UNITY_5_3_OR_NEWER

using UnityEngine;

namespace Extenity.JsonToolbox.Converters
{

	/// <summary>
	/// Custom <c>Newtonsoft.Json.JsonConverter</c> for <c>UnityEngine.Vector3</c>.
	/// Source: wanzyeestudio.blogspot.com
	/// </summary>
	public class Vector3Converter : PartialConverter<Vector3>
	{
		/// <summary>
		/// Get the property names include <c>x</c>, <c>y</c>, <c>z</c>.
		/// </summary>
		/// <returns>The property names.</returns>
		protected override string[] GetPropertyNames()
		{
			return new[] { "x", "y", "z" };
		}
	}

}

#endif
