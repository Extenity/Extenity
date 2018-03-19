using UnityEditor;

namespace Extenity.GameObjectToolbox.Editor
{

	[CustomEditor(typeof(DontShowEditorHandler))]
	public class DontShowEditorHandlerEditor : UnityEditor.Editor
	{
		private Tool OverriddenTool = Tool.None;

		private void OnEnable()
		{
			ChangeToolToNone();
		}

		private void OnDisable()
		{
			Tools.current = OverriddenTool;
		}

		private void OnSceneGUI()
		{
			if (Tools.current != Tool.None)
			{
				ChangeToolToNone();
			}
		}

		private void ChangeToolToNone()
		{
			OverriddenTool = Tools.current;
			Tools.current = Tool.None;
		}
	}

}
