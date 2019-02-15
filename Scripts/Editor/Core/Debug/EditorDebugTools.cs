using System.Reflection;
using UnityEditor;

namespace Extenity.DebugToolbox.Editor
{

	public static class EditorDebugTools
	{
		#region Clear Console

		/// <summary>
		/// See: https://answers.unity.com/questions/578393/clear-console-through-code-in-development-build.html
		/// </summary>
		public static void ClearDeveloperConsole()
		{
			//Debug.ClearDeveloperConsole(); This does not work for some reason.

			var assembly = Assembly.GetAssembly(typeof(SceneView));
			var logEntries = assembly.GetType("UnityEditor.LogEntries");
			var clearMethod = logEntries.GetMethod("Clear");
			clearMethod.Invoke(new object(), null);
		}

		#endregion
	}

}
