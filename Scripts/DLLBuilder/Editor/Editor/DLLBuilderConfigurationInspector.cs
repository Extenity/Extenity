using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.DLLBuilder
{

	[CustomEditor(typeof(DLLBuilderConfiguration))]
	public class DLLBuilderConfigurationInspector : ExtenityObjectEditorBase<DLLBuilderConfiguration>
	{
		protected override void OnEnableDerived()
		{
			IsDefaultInspectorDrawingEnabled = true;
			IsDefaultInspectorScriptFieldEnabled = false;

			DLLBuilderConfiguration.LoadEnvironmentVariables();
		}

		protected override void OnDisableDerived()
		{
		}

		private static readonly GUILayoutOption[] ListButtonLayout = { GUILayout.Width(24), GUILayout.Height(24) };
		private static readonly GUILayoutOption[] AddButtonLayout = { GUILayout.Width(90), GUILayout.Height(24) };

		protected override void OnBeforeDefaultInspectorGUI()
		{
			EditorGUILayoutTools.DrawHeader("Environment Variables");
			if (DLLBuilderConfiguration.EnvironmentVariables.IsNotNullAndEmpty())
			{
				EditorGUI.BeginChangeCheck();
				for (var i = 0; i < DLLBuilderConfiguration.EnvironmentVariables.Count; i++)
				{
					var variable = DLLBuilderConfiguration.EnvironmentVariables[i];
					var isFirst = i == 0;
					var isLast = i == DLLBuilderConfiguration.EnvironmentVariables.Count - 1;
					GUILayout.BeginHorizontal();
					EditorGUI.BeginChangeCheck();
					variable.Key = EditorGUILayout.TextField(variable.Key, GUILayout.ExpandWidth(true));
					variable.Value = EditorGUILayout.TextField(variable.Value, GUILayout.ExpandWidth(true));
					if (EditorGUI.EndChangeCheck())
						DLLBuilderConfiguration.EnvironmentVariables[i] = variable;

					// Move up
					if (isFirst)
						EditorGUI.BeginDisabledGroup(true);
					if (GUILayout.Button("^", ListButtonLayout))
					{
						var iCached = i;
						EditorApplication.delayCall += () => { MoveEnvironmentVariableUp(iCached); };
					}
					if (isFirst)
						EditorGUI.EndDisabledGroup();

					// Move down
					if (isLast)
						EditorGUI.BeginDisabledGroup(true);
					if (GUILayout.Button("v", ListButtonLayout))
					{
						var iCached = i;
						EditorApplication.delayCall += () => { MoveEnvironmentVariableDown(iCached); };
					}
					if (isLast)
						EditorGUI.EndDisabledGroup();

					// Remove
					if (GUILayout.Button("x", ListButtonLayout))
					{
						var iCached = i;
						EditorApplication.delayCall += () => { RemoveEnvironmentVariable(iCached); };
					}

					GUILayout.EndHorizontal();
				}
				if (EditorGUI.EndChangeCheck())
				{
					DelayedSaveEnvironmentVariables();
				}
			}
			if (GUILayout.Button("Add", AddButtonLayout))
			{
				EditorApplication.delayCall += AddEnvironmentVariable;
			}

			GUILayout.Space(70f);
			EditorGUILayoutTools.DrawHeader("Configuration");
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
		}

		#region Environment Variables

		private static bool IsSaveAlreadyInitiated;

		private void DelayedSaveEnvironmentVariables()
		{
			if (IsSaveAlreadyInitiated)
				return;
			IsSaveAlreadyInitiated = true;
			EditorApplication.delayCall += DoSaveEnvironmentVariables;
		}

		private void DoSaveEnvironmentVariables()
		{
			DLLBuilderConfiguration.SaveEnvironmentVariables();
			IsSaveAlreadyInitiated = false;
		}

		private void AddEnvironmentVariable()
		{
			DLLBuilderConfiguration.EnvironmentVariables.Add(new KeyValue<string, string>());
			DLLBuilderConfiguration.SaveEnvironmentVariables();
			Repaint();
		}

		private void RemoveEnvironmentVariable(int i)
		{
			DLLBuilderConfiguration.EnvironmentVariables.RemoveAt(i);
			DLLBuilderConfiguration.SaveEnvironmentVariables();
			Repaint();
		}

		private void MoveEnvironmentVariableUp(int i)
		{
			var tmp = DLLBuilderConfiguration.EnvironmentVariables[i];
			DLLBuilderConfiguration.EnvironmentVariables[i] = DLLBuilderConfiguration.EnvironmentVariables[i - 1];
			DLLBuilderConfiguration.EnvironmentVariables[i - 1] = tmp;
			DLLBuilderConfiguration.SaveEnvironmentVariables();
			Repaint();
		}

		private void MoveEnvironmentVariableDown(int i)
		{
			var tmp = DLLBuilderConfiguration.EnvironmentVariables[i];
			DLLBuilderConfiguration.EnvironmentVariables[i] = DLLBuilderConfiguration.EnvironmentVariables[i + 1];
			DLLBuilderConfiguration.EnvironmentVariables[i + 1] = tmp;
			DLLBuilderConfiguration.SaveEnvironmentVariables();
			Repaint();
		}

		#endregion
	}

}
