using UnityEngine;
using Extenity.Logging;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(ColorScaleUI))]
public class ColorScaleUIInspector : ExtenityEditorBase<ColorScaleUI>
{
	protected override void OnEnableDerived()
	{
	}

	protected override void OnDisableDerived()
	{
	}

	protected override void OnAfterDefaultInspectorGUI()
	{
	}
}
