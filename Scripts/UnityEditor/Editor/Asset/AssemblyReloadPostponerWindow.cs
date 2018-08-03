using UnityEngine;
using UnityEditor;

namespace Extenity.AssetToolbox.Editor
{

	/// <summary>
	/// http://answers.unity3d.com/questions/561715/can-you-disable-the-autorecompile.html
	/// </summary>
	public class AssemblyReloadPostponerWindow : EditorWindow
	{

		System.Action DebugDelegate;

		[MenuItem("Assets/Assembly Reload Postponer")]
		public static void Init()
		{
			GetWindow<AssemblyReloadPostponerWindow>();
		}

		bool locked = false;

		void OnEnable()
		{
			DebugDelegate = CompilingPrevented;
		}

		void OnGUI()
		{
			if (EditorApplication.isCompiling && locked)
			{
				EditorApplication.LockReloadAssemblies();
				DebugDelegate(); //this is just so the console won't be flooded
			}
			GUILayout.Label("Assemblies currently locked: " + locked.ToString());
			if (GUILayout.Button("Lock Reload"))
			{
				locked = true;
			}
			if (GUILayout.Button("Unlock Reload"))
			{
				EditorApplication.UnlockReloadAssemblies();
				locked = false;
				if (EditorApplication.isCompiling)
				{
					Debug.Log("You can now reload assemblies.");
					DebugDelegate = CompilingPrevented;
				}
			}

			Repaint();
		}

		private void CompilingPrevented()
		{
			Debug.Log("Compiling currently prevented: press Unlock Reload to reallow compilation.");
			DebugDelegate = EmptyMethod;
		}

		private void EmptyMethod() { }
	}

}
