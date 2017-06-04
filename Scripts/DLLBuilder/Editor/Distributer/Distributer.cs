using System.IO;
using System.Linq;
using Extenity.DataToolbox;
using UnityEngine;

namespace Extenity.DLLBuilder
{

	public static class Distributer
	{
		public static void DistributeToOutsideProjects()
		{
			var sourceDirectoryPath = Constants.OutputProjectExtenityDirectory;
			Debug.LogFormat("Distributing '{0}' to outside projects", sourceDirectoryPath);

			var targets = DLLBuilderConfiguration.Instance.Distributer.Targets.Where(
				item => item != null &&
				item.Enabled &&
				!string.IsNullOrEmpty(item.Path)).ToList();

			if (targets.IsNullOrEmpty())
			{
				Debug.LogWarning("There are no outside projects to be distributed. Please check builder configuration.");
				return;
			}

			foreach (var target in targets)
			{
				var targetDirectoryPath = target.Path.FixDirectorySeparatorChars('/').AddDirectorySeparatorToEnd('/');

				// Check that we don't destroy an unwanted directory.
				if (!targetDirectoryPath.EndsWith("/Extenity/"))
				{
					Debug.LogErrorFormat("Distribution target directory '{0}' does not end with 'Extenity'. This is a precaution to prevent destruction. Please make sure the target directory is correct.", target.Path);
					continue;
				}

				// Check that the target directory exists. We want to make sure user creates the directory first. This is more safer.
				if (!Directory.Exists(targetDirectoryPath))
				{
					Debug.LogErrorFormat("Distribution target directory '{0}' does not exist. Please make sure the target directory is created.", target.Path);
					continue;
				}

				Debug.LogFormat("Distributing to '{0}'", targetDirectoryPath);

				// TODO: Better just sync files, instead of deleting and copying from scratch.
				DirectoryTools.Delete(targetDirectoryPath);
				DirectoryTools.Copy(sourceDirectoryPath, targetDirectoryPath, null, null, SearchOption.AllDirectories, true, true, false, null);
			}

			Debug.Log("Distribution completed.");
		}
	}

}
