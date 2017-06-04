using System.IO;
using Extenity.DataToolbox;
using UnityEngine;

namespace Extenity.DLLBuilder
{

	public static class PackageBuilder
	{
		public static void CopyExtenityAssetsToOutputProject()
		{
			Debug.Log("Copying Extenity assets to output project");

			foreach (var directory in Constants.CopiedPackageDirectories)
			{
				var sourceDirectoryPath = Constants.ExtenityDirectory + directory;
				var targetDirectoryPath = Constants.OutputProjectExtenityDirectory + directory;

				// TODO: Better just sync files, instead of deleting and copying from scratch.
				DirectoryTools.Delete(targetDirectoryPath);
				DirectoryTools.Copy(sourceDirectoryPath, targetDirectoryPath, null, null, SearchOption.AllDirectories, true, true, false, null);
			}
		}
	}

}
