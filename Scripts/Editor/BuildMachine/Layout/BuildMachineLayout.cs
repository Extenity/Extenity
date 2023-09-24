using System.IO;
using System.Runtime.CompilerServices;
using Extenity.ApplicationToolbox;
using Extenity.BuildToolbox.Editor;
using Extenity.FileSystemToolbox;
using UnityEditor;

namespace Extenity.BuildMachine.Editor
{

	public static class BuildMachineLayout
	{
		#region Change Unity Editor Layout

		public static void LoadConsoleOnlyLayout()
		{
			// Skip in batch mode.
			if (!BuildTools.IsBatchMode)
			{
				// Delay the layout change to the next frame. Otherwise it will be reverted by Unity.
				// It might be a good idea to make this method async and allow caller to wait for the layout change.
				EditorApplication.delayCall += () =>
				{
					try
					{
						DoLoadBuildMachineLayout(BuildMachineConstants.ConsoleOnlyLayoutFileName);
					}
					catch
					{
						// Ignored
					}
				};
			}
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
