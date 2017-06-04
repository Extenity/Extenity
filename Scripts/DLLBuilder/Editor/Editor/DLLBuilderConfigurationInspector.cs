using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Extenity.UnityEditorToolbox.Editor;

namespace Extenity.DLLBuilder
{

	[CustomEditor(typeof(DLLBuilderConfiguration))]
	public class DLLBuilderConfigurationInspector : ExtenityObjectEditorBase<DLLBuilderConfiguration>
	{
		protected override void OnEnableDerived()
		{
			IsDefaultInspectorDrawingEnabled = true;
			IsDefaultInspectorScriptFieldEnabed = false;
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
		}
	}

}
