using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine.UI;

namespace Extenity.UIToolbox.Editor
{

	[CustomEditor(typeof(Blinker))]
	public class BlinkerInspector : ExtenityEditorBase<Blinker>
	{
		protected override void OnEnableDerived()
		{
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnBeforeDefaultInspectorGUI()
		{
			if (Me.ActivatedObject && typeof(ILayoutElement).IsAssignableFrom(Me.ActivatedObject.GetType()))
			{
				EditorGUILayout.HelpBox("CAUTION! It's not performance friendly to activate-deactivate UI objects. Use coloring and transparency whenever possible.", MessageType.Warning);
			}
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
		}
	}

}
