using System;
using UnityEngine;
using System.IO;
using Extenity.DataToolbox;

namespace Extenity.ApplicationToolbox.Editor
{

	public class EditorApplicationTools : MonoBehaviour
	{
		#region Paths

		public static string UnityProjectPath
		{
			get
			{
				// This does not work in threaded environment. So we use working directory instead.
				//return ApplicationTools.ApplicationPath.AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();

				return Directory.GetCurrentDirectory().AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();
			}
		}

		public static string EditorTempDirectoryPath
		{
			get
			{
				return Path.Combine(UnityProjectPath, "Temp").AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();
			}
		}

		private static string _UnityEditorExecutableDirectory;
		public static string UnityEditorExecutableDirectory
		{
			get
			{
				if (_UnityEditorExecutableDirectory == null)
				{
					//_UnityEditorExecutableDirectory = AppDomain.CurrentDomain.BaseDirectory; This returns null for some reason.
					var file = new FileInfo(typeof(UnityEditor.EditorApplication).Assembly.Location);
					var directory = file.Directory;
					var parentDirectory = directory.Parent;
					if (directory.Name != "Managed" || parentDirectory.Name != "Data")
						throw new Exception("Internal error! Unexpected Unity Editor executable location.");
					_UnityEditorExecutableDirectory = parentDirectory.Parent.FullName;
				}
				return _UnityEditorExecutableDirectory;
			}
		}

		#endregion
	}

}
