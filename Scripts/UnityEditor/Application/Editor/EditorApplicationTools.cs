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
				return ApplicationTools.ApplicationPath.AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();
			}
		}

		public static string EditorTempDirectoryPath
		{
			get
			{
				return Path.Combine(UnityProjectPath, "Temp").AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();
			}
		}

		#endregion
	}

}
