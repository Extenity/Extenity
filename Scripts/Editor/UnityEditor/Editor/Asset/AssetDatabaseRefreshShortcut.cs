using Extenity.ApplicationToolbox;
using UnityEditor;

namespace Extenity.AssetToolbox.Editor
{

	[InitializeOnLoad]
	public class AssetDatabaseRefreshShortcut
	{

		private static bool IsPressing = true;

		static AssetDatabaseRefreshShortcut()
		{
			EditorApplication.update += Update;
		}

		private static void Update()
		{
			if (OperatingSystemTools.IsAsyncKeyStateDown(AsyncKeyCodes.RControl) &&
			    OperatingSystemTools.IsAsyncKeyStateDown(AsyncKeyCodes.RShift) &&
			    OperatingSystemTools.IsAsyncKeyStateDown(AsyncKeyCodes.X))
			{
				if (!IsPressing) // Prevent calling refresh multiple times before user releases the keys
				{
					IsPressing = true;
					Log.Info("Refreshing asset database");
					if (EditorApplication.isPlaying)
					{
						EditorApplication.isPlaying = false;
					}
					AssetDatabase.Refresh();
				}
			}
			else
			{
				IsPressing = false;
			}
		}
	}

}
