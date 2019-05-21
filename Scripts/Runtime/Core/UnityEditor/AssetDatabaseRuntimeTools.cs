using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Extenity.UnityEditorToolbox
{

	/// <summary>
	/// Helper class to use <see cref="AssetDatabase"/> operations outside of editor scripts.
	/// Though these operations are marked with <see cref="ConditionalAttribute"/> so they are
	/// only available in Editor and not in built binaries.
	/// </summary>
	public static class AssetDatabaseRuntimeTools
	{
		#region ReleaseCachedFileHandles

		/// <summary>
		/// Make Unity release the files to prevent any IO errors.
		/// </summary>
#if !UNITY_EDITOR
		[System.Diagnostics.Conditional("UNITY_EDITOR")]
#endif
		public static void ReleaseCachedFileHandles()
		{
#if UNITY_EDITOR
			AssetDatabase.ReleaseCachedFileHandles();
#endif
		}

		#endregion
	}

}
