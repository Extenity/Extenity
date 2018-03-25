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
					var isPreviousReadonly = i > 0 && DLLBuilderConfiguration.EnvironmentVariables[i - 1].Value.Readonly;
					GUILayout.BeginHorizontal();
					EditorGUI.BeginChangeCheck();
					EditorGUI.BeginDisabledGroup(variable.Value.Readonly);
					variable.Key = EditorGUILayout.TextField(variable.Key, GUILayout.ExpandWidth(true));
					variable.Value.Value = EditorGUILayout.TextField(variable.Value.Value, GUILayout.ExpandWidth(true));
					EditorGUI.EndDisabledGroup();
					if (EditorGUI.EndChangeCheck())
					{
						if (!variable.Value.Readonly)
							DLLBuilderConfiguration.EnvironmentVariables[i] = variable;
					}

					if (variable.Value.Readonly)
					{
						GUILayout.EndHorizontal();
						continue;
					}

					// Move up
					if (isFirst || isPreviousReadonly)
						EditorGUI.BeginDisabledGroup(true);
					if (GUILayout.Button("^", ListButtonLayout))
					{
						var iCached = i;
						EditorApplication.delayCall += () => { MoveEnvironmentVariableUp(iCached); };
					}
					if (isFirst || isPreviousReadonly)
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
			DLLBuilderConfiguration.EnvironmentVariables.Add(new KeyValue<string, DLLBuilderConfiguration.EnvironmentVariable>());
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
