#if UNITY_EDITOR_WIN

using UnityEditor;
using System.Runtime.InteropServices;

namespace Extenity.AssetToolbox.Editor
{

	[InitializeOnLoad]
	public class AssetDatabaseRefreshShortcut
	{
		[DllImport("user32.dll")]
		static extern short GetAsyncKeyState(int vKey);

		public static readonly int RWin = 91;
		public static readonly int RShift = 160;
		public static readonly int RControl = 162;
		public static readonly int R = 82;
		public static readonly int X = 88;
		public static readonly int Z = 90;

		private static bool IsPressing = true;

		static AssetDatabaseRefreshShortcut()
		{
			EditorApplication.update += Update;
		}

		private static void Update()
		{
			if ((GetAsyncKeyState(RControl) & 0x8000) != 0 &&
				(GetAsyncKeyState(RShift) & 0x8000) != 0 &&
				(GetAsyncKeyState(X) & 0x8000) != 0)
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

#endif
