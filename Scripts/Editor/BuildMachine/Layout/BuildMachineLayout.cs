using System.IO;
using System.Runtime.CompilerServices;
using Extenity.ApplicationToolbox;
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
			var projectPath = ApplicationTools.ApplicationPath;
			var path = projectPath.MakeRelativePath(sourceFilePath);
			var directory = Path.GetDirectoryName(path);
			var layoutPath = Path.Combine(directory, layoutFileName);

			EditorUtility.LoadWindowLayout(layoutPath);
		}

		#endregion
	}

}
