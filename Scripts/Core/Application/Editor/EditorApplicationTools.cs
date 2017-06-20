using System;
using UnityEngine;
using System.IO;
using Extenity.DataToolbox;
using UnityEditor;

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

		#region Update Continuum

		/// <summary>
		/// Creates and destroys gameobjects to keep EditorApplication.update calls coming.
		/// That's the worst ever idea but it's the only way I could find.
		/// 
		/// Note that this is costly so try to use it only when needed.
		/// </summary>
		public static void GuaranteeNextUpdateCall()
		{
			var go = new GameObject("_EditorApplicationUpdateHelper");
			GameObject.DestroyImmediate(go);
		}

		/// <summary>
		/// A little trick to hopefully keep EditorApplication.update calls coming. It uses
		/// EditorApplication.delayCall to trigger a call to EditorApplication.update.
		/// This won't help if Editor window does not have focus.
		/// 
		/// Note that it may greatly increase calls to EditorApplication.update beyond needs.
		/// Scripts that does costly operations in their updates would slow down the editor.
		/// </summary>
		public static void IncreaseChancesOfNextUpdateCall()
		{
			EditorApplication.delayCall += () =>
			{
				EditorApplication.update.Invoke();
			};
		}

		#endregion
	}

}
