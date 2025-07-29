#if UNITY_5_3_OR_NEWER

using UnityEngine;

namespace Extenity.RenderingToolbox
{

	public static class ShaderTools
	{
		#region Common Shader Properties

		private static int _BaseColorProperty = 0;
		public static int BaseColorProperty
		{
			get
			{
				if (_BaseColorProperty == 0)
				{
					_BaseColorProperty = Shader.PropertyToID("_BaseColor");
				}
				return _BaseColorProperty;
			}
		}

		private static int _OffsetProperty = 0;
		public static int OffsetProperty
		{
			get
			{
				if (_OffsetProperty == 0)
				{
					_OffsetProperty = Shader.PropertyToID("_Offset");
				}
				return _OffsetProperty;
			}
		}

		#endregion
	}

}

#endif
