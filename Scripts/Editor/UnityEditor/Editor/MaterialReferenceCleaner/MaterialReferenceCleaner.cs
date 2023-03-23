using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	public class MaterialReferenceCleaner : ExtenityEditorWindowBase
	{
		#region Configuration

		protected override WindowSpecifications Specifications => new WindowSpecifications
		{
			Title = "Ref Cleaner",
			EnableRightMouseButtonScrolling = true,
		};

		#endregion

		#region Initialization

		protected override void OnEnableDerived()
		{
			GetSelectedMaterial();
		}

		[MenuItem(ExtenityMenu.Painkiller + "Material Reference Cleaner", priority = ExtenityMenu.PainkillerPriorityEnd)]
		public static void ShowWindow()
		{
			EditorWindowTools.ToggleWindow<MaterialReferenceCleaner>();
		}

		#endregion

		#region Deinitialization

		protected override void OnDestroyDerived()
		{
			DeinitializeSelection();
		}

		#endregion

		#region GUI

		protected override void OnGUIDerived()
		{
			EditorGUIUtility.labelWidth = 200f;

			if (SelectedMaterial == null)
			{
				EditorGUILayout.LabelField("No material selected");
			}
			else
			{
				if (GUILayout.Button("Remove All", BigButtonHeight))
				{
					var cachedMaterial = SelectedMaterial;
					EditorApplication.delayCall += () =>
					{
						RemoveAllUnusedReferencesAndParameters(cachedMaterial, true);
						Repaint();
					};
				}

				EditorGUILayout.Space();

				ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);
				{

					EditorGUILayout.LabelField("Selected material:", SelectedMaterial.name);
					EditorGUILayout.LabelField("Shader:", SelectedMaterial.shader.name);
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Properties");

					SerializedObject.Update();

					EditorGUI.indentLevel++;

					EditorGUILayout.LabelField("Textures");
					EditorGUI.indentLevel++;
					ProcessProperties("m_SavedProperties.m_TexEnvs");
					EditorGUI.indentLevel--;

					EditorGUILayout.LabelField("Floats");
					EditorGUI.indentLevel++;
					ProcessProperties("m_SavedProperties.m_Floats");
					EditorGUI.indentLevel--;

					EditorGUILayout.LabelField("Colors");
					EditorGUI.indentLevel++;
					ProcessProperties("m_SavedProperties.m_Colors");
					EditorGUI.indentLevel--;

					EditorGUI.indentLevel--;
				}
				GUILayout.EndScrollView();
			}

			EditorGUIUtility.labelWidth = 0;
		}

		private void ProcessProperties(string path)
		{
			var properties = SerializedObject.FindProperty(path);
			if (properties != null && properties.isArray)
			{
				for (int i = 0; i < properties.arraySize; i++)
				{
					var displayName = properties.GetArrayElementAtIndex(i).displayName;
					var exist = SelectedMaterial.HasProperty(displayName);

					if (exist)
					{
						EditorGUILayout.LabelField(displayName, "Exist");
					}
					else
					{
						using (new EditorGUILayout.HorizontalScope())
						{
							EditorGUILayout.LabelField(displayName, "Old reference", "CN StatusWarn");
							if (GUILayout.Button("Remove", GUILayout.Width(80f)))
							{
								properties.DeleteArrayElementAtIndex(i);
								SerializedObject.ApplyModifiedProperties();
								AssetDatabase.SaveAssets();
							}
						}
					}
				}
			}
		}

		#endregion

		#region Asset Display Events

		protected void OnSelectionChange()
		{
			GetSelectedMaterial();
		}

		protected void OnProjectChange()
		{
			GetSelectedMaterial();
		}

		public class MaterialPostprocessor : AssetPostprocessor
		{
			private void OnPostprocessMaterial(Material material)
			{
				if (SelectedMaterial && SelectedMaterial == material)
				{
					Log.Info("Quickly displaying the reimported material: " + material);
					GetSelectedMaterial();
				}
			}
		}

		#endregion

		#region Remove All Unused References And Parameters

		public static void RemoveAllUnusedReferencesAndParameters(Material material, bool saveAssets)
		{
			var serializedObject = new SerializedObject(material);

			_RemoveAll(serializedObject, material, "m_SavedProperties.m_TexEnvs");
			_RemoveAll(serializedObject, material, "m_SavedProperties.m_Floats");
			_RemoveAll(serializedObject, material, "m_SavedProperties.m_Colors");

			if (saveAssets)
			{
				AssetDatabase.SaveAssets();
			}
		}

		private static void _RemoveAll(SerializedObject serializedObject, Material material, string path)
		{
			var properties = serializedObject.FindProperty(path);
			if (properties != null && properties.isArray)
			{
				for (int i = 0; i < properties.arraySize; i++)
				{
					var propName = properties.GetArrayElementAtIndex(i).displayName;
					var exist = material.HasProperty(propName);

					if (!exist)
					{
						properties.DeleteArrayElementAtIndex(i);
						serializedObject.ApplyModifiedProperties();
						i--;
					}
				}
			}
		}

		#endregion

		#region Selected Material

		private static Material SelectedMaterial;
		private static SerializedObject SerializedObject;

		private void DeinitializeSelection()
		{
			SelectedMaterial = null;
			SerializedObject = null;
		}

		private static void GetSelectedMaterial()
		{
			SelectedMaterial = Selection.activeObject as Material;
			if (SelectedMaterial != null)
			{
				SerializedObject = new SerializedObject(SelectedMaterial);
			}

			GetWindow<MaterialReferenceCleaner>().Repaint();
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(MaterialReferenceCleaner));

		#endregion
	}

}
