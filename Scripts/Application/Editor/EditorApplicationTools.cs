using UnityEngine;
using System.IO;

namespace Extenity.Applicational
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
