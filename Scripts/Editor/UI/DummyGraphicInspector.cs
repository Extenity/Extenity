using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;

namespace Extenity.UIToolbox.Editor
{

	[CustomEditor(typeof(DummyGraphic))]
	public class DummyGraphicInspector : ExtenityEditorBase<DummyGraphic>
	{
		private SerializedProperty RaycastTargetProperty;

		protected override void OnEnableDerived()
		{
			IsDefaultInspectorDrawingEnabled = false;
			RaycastTargetProperty = GetProperty("m_RaycastTarget");
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			EditorGUILayout.PropertyField(RaycastTargetProperty);
		}
	}

}
