#if UNITY_5_3_OR_NEWER

using UnityEngine;

namespace Extenity.JsonToolbox.Converters
{

	/// <summary>
	/// Custom <c>Newtonsoft.Json.JsonConverter</c> for <c>UnityEngine.Bounds</c>.
	/// Source: wanzyeestudio.blogspot.com
	/// </summary>
	public class BoundsConverter : PartialConverter<Bounds>
	{
		/// <summary>
		/// Prevent the properties from being stripped.
		/// </summary>
		/*
		 * https://docs.unity3d.com/Manual/IL2CPP-BytecodeStripping.html
		 * Instead of an extra file, work around by making and accessing a dummy instance.
		 */
		private void PreserveProperties()
		{
			var _dummy = new Bounds();

			_dummy.center = _dummy.center;
			_dummy.extents = _dummy.extents;
		}

		/// <summary>
		/// Get the property names include <c>center</c>, <c>extents</c>.
		/// </summary>
		/// <returns>The property names.</returns>
		protected override string[] GetPropertyNames()
		{
			return new[] { "center", "extents" };
		}
	}

}

#endif
