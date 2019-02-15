using System.Reflection;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class EditorWindowTools
	{
		#region EditorWindow.docked

		private static PropertyInfo DockedPropertyInfo;

		public static bool IsDocked(this EditorWindow window)
		{
			if (DockedPropertyInfo == null)
				DockedPropertyInfo = typeof(EditorWindow).GetProperty("docked", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);
			return (bool)DockedPropertyInfo.GetValue(window, null);
		}

		#endregion
	}

}
