using System.Diagnostics;

namespace Extenity.OdinToolbox
{

	public static class OdinTools
	{
		/// <summary>
		/// Tells Odin to repaint the inspector. See example below for continuous repaint.
		/// </summary>
		/// <example>
		/// <code>
		/// #if UNITY_EDITOR
		/// 	[OnInspectorGUI]
		/// 	private void OnInspectorGUI()
		/// 	{
		/// 		OdinTools.Repaint();
		/// 	}
		/// #endif
		/// </code>
		/// </example>
		[Conditional("ODIN_INSPECTOR")]
		public static void Repaint()
		{
#if ODIN_INSPECTOR && UNITY_EDITOR
			Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
#endif
		}
	}

}
