#if UNITY_5_3_OR_NEWER

using UnityEngine;
using System.Linq;

namespace Extenity.JsonToolbox.Converters
{

	/// <summary>
	/// Custom <c>Newtonsoft.Json.JsonConverter</c> for <c>UnityEngine.Matrix4x4</c>.
	/// Source: wanzyeestudio.blogspot.com
	/// </summary>
	public class Matrix4x4Converter : PartialConverter<Matrix4x4>
	{
		/// <summary>
		/// Get the property names include from <c>m00</c> to <c>m33</c>.
		/// </summary>
		/// <returns>The property names.</returns>
		protected override string[] GetPropertyNames()
		{
			var _indexes = new[] { "0", "1", "2", "3" };
			return _indexes.SelectMany((row) => _indexes.Select((column) => "m" + row + column)).ToArray();
		}
	}

}

#endif
