using System;
using System.IO;
using System.Linq;
using System.Text;
using Extenity.ApplicationToolbox;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class AsmdefContainmentChecker
	{
		[InitializeOnLoadMethod]
		public static void Check()
		{
			// See if Unity generates Assembly-CSharp.dll which means there are some types and they are not covered
			// inside Assembly Definitions.
			var directoryPath = Path.Combine(ApplicationTools.UnityProjectPaths.LibraryDirectory, "ScriptAssemblies");
			var dllRuntimePath = Path.Combine(directoryPath, "Assembly-CSharp.dll");
			var dllEditorPath = Path.Combine(directoryPath, "Assembly-CSharp-Editor.dll");

			if (File.Exists(dllRuntimePath) ||
			    File.Exists(dllEditorPath))
			{
				var stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Script detected outside of asmdefs, which increases compilation times. Click to see details.");
				stringBuilder.AppendLine("Make sure these types are covered in Assembly Definitions:");

				var unwantedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(assembly =>
				{
					var name = assembly.GetName().Name;
					return name.Equals("Assembly-CSharp-Editor", StringComparison.Ordinal) ||
					       name.Equals("Assembly-CSharp", StringComparison.Ordinal);
				}).ToArray();

				foreach (var assembly in unwantedAssemblies)
				{
					stringBuilder.AppendLine("Assembly: " + assembly.GetName().Name);
					foreach (var type in assembly.GetTypes())
					{
						stringBuilder.AppendLine("\t" + type.FullName);
					}
				}
				Log.Warning(stringBuilder.ToString());
			}
		}

		#region Log

		private static readonly Logger Log = new(nameof(AsmdefContainmentChecker));

		#endregion
	}

}
