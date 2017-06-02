using UnityEditor;

namespace Extenity.GameObjectToolbox.Editor
{

	[CustomEditor(typeof(DontShowEditorHandler))]
	public class DontShowEditorHandlerEditor : UnityEditor.Editor
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
