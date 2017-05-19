using UnityEditor;

namespace Extenity.GameObjectToolbox
{

	[CustomEditor(typeof(DontShowEditorHandler))]
	public class DontShowEditorHandlerEditor : Editor
	{
		Tool LastTool = Tool.None;

		void OnEnable()
		{
			LastTool = Tools.current;
			Tools.current = Tool.None;
		}

		void OnDisable()
		{
			Tools.current = LastTool;
		}
	}

}
