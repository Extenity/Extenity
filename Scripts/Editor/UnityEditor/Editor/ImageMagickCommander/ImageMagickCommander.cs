using System;
using System.Diagnostics;
using System.IO;
using Extenity.FileSystemToolbox;
using Extenity.GameObjectToolbox;
using Extenity.SceneManagementToolbox;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extenity.UnityEditorToolbox.ImageMagick
{

	public static class ImageMagickCommander
	{
		#region Reflection Probe Blurring

		public static void BlurReflectionProbesInScenes(ActiveCheck activeCheck, SceneListFilter sceneListFilter)
		{
			SceneManagerTools.GetScenes(sceneListFilter)
			                 .ForEach(scene => scene.BlurReflectionProbes(activeCheck));
		}

		public static void BlurReflectionProbes(this Scene scene, ActiveCheck activeCheck)
		{
			const int sizeX = 96;
			const int sizeY = 16;

			var reflectionProbes = scene.FindObjectsOfType<ReflectionProbe>(activeCheck);
			foreach (var reflectionProbe in reflectionProbes)
			{
				if (!reflectionProbe.bakedTexture)
					continue;

				var path = AssetDatabase.GetAssetPath(reflectionProbe.bakedTexture);
				Log.Info($"Downsizing Reflection Probe baked texture to '{sizeX}x{sizeY}' at path: {path}");

				path = Path.GetFullPath(path).FixDirectorySeparatorChars('\\');
				var directoryPath = Path.GetDirectoryName(path).FixDirectorySeparatorChars('\\');

				ResizeImages(path, sizeX, sizeY, directoryPath + "/%[filename:fname].exr");
			}

			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
		}

		#endregion

		#region Resize Image

		public static void ResizeImages(string inputFilter, int sizeX, int sizeY, string outputFilter)
		{
			var errorReceived = false;

			var process = new Process();
			process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.CreateNoWindow = false;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.FileName = "magick.exe";
			process.StartInfo.Arguments = $"\"{inputFilter}\" -resize {sizeX}x{sizeY} -set filename:fname \"%t\" \"{outputFilter}\"";

			process.ErrorDataReceived += (sender, eventArgs) =>
			{
				var data = eventArgs.Data.Trim();
				if (!string.IsNullOrEmpty(data))
				{
					errorReceived = true;
					Log.Error("ImageMagick Error: " + data);
				}
			};
			process.OutputDataReceived += (sender, eventArgs) =>
			{
				var data = eventArgs.Data.Trim();
				if (!string.IsNullOrEmpty(data))
					Log.Info("ImageMagick Output: " + data);
			};

			//Log.Info("launch |     " + process.StartInfo.FileName);
			//Log.Info("args |     " + process.StartInfo.Arguments);

			process.Start();

			process.BeginErrorReadLine();
			process.BeginOutputReadLine();

			process.WaitForExit();

			if (errorReceived)
			{
				throw new Exception("Failed to convert image. See previous errors.");
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(ImageMagickCommander));

		#endregion
	}

}
