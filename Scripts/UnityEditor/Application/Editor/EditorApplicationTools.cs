using UnityEngine;
using System.IO;
using Extenity.DataToolbox;

namespace Extenity.ApplicationToolbox.Editor
{

	public class EditorApplicationTools : MonoBehaviour
	{
		#region Paths

		public static string EditorTempDirectoryPath
		{
			get
			{
				return Path.Combine(ApplicationTools.ApplicationPath, "Temp").AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();
			}
		}

		#endregion
	}

}
