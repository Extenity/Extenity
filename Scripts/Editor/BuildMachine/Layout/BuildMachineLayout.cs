using System.IO;
using System.Runtime.CompilerServices;
using Extenity.ApplicationToolbox.Editor;
using Extenity.FileSystemToolbox;
using UnityEditor;

namespace Extenity.BuildMachine.Editor
{

	public static class BuildMachineLayout
	{
		#region Change Unity Editor Layout

		public static void LoadConsoleOnlyLayout()
		{
			DoLoadBuildMachineLayout(BuildMachineConstants.ConsoleOnlyLayoutFileName);
		}

		private static void DoLoadBuildMachineLayout(string layoutFileName, [CallerFilePath] string sourceFilePath = null)
		{
			var path = EditorApplicationTools.UnityProjectPath.MakeRelativePath(sourceFilePath);
			var directory = Path.GetDirectoryName(path);
			var layoutPath = Path.Combine(directory, layoutFileName);

			EditorUtility.LoadWindowLayout(layoutPath);
		}

		#endregion
	}

}
