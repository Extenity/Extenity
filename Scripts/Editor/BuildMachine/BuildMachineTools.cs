using UnityEditor;

namespace Extenity.BuildMachine.Editor
{

	public static class BuildMachineTools
	{
		#region Change Unity Editor Layout

		public static void LoadBuildMachineLayout()
		{
			EditorUtility.LoadWindowLayout(BuildMachineConstants.LayoutPath);
		}

		#endregion
	}

}
