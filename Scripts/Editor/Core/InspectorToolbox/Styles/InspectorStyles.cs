using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// Loads the style sheets of the inspector toolbox and puts them on an element. Every host root applies them
	/// itself, so a drawer looks right in any window it ends up in.
	/// </summary>
	public static class InspectorStyles
	{
		// The GUIDs of the .uss.meta files next to this file. Loading by GUID works wherever the package is
		// installed: embedded, in the package cache or copied into Assets.
		public const string BaseStyleSheetGuid = "c18127a34fb845f5ae083eb43a9736dd";
		public const string DarkStyleSheetGuid = "529517d29f4449779c6c85ad0af4d037";
		public const string LightStyleSheetGuid = "ff4cb203b8e4428dbfb2016048830083";

		/// <summary>On the root of everything the toolbox styles. All variables are defined on it.</summary>
		public const string RootClass = "ext-inspector";
		/// <summary>On a property field whose label was chosen by its list and must win over the element's own title.</summary>
		public const string ExplicitLabelClass = "ext-explicit-label";

		private static StyleSheet _Base;
		private static StyleSheet _Dark;
		private static StyleSheet _Light;

		public static StyleSheet Base => _Base != null ? _Base : _Base = Load(BaseStyleSheetGuid, "InspectorToolbox.uss");
		public static StyleSheet Dark => _Dark != null ? _Dark : _Dark = Load(DarkStyleSheetGuid, "InspectorToolbox.Dark.uss");
		public static StyleSheet Light => _Light != null ? _Light : _Light = Load(LightStyleSheetGuid, "InspectorToolbox.Light.uss");

		/// <summary>The sheet with the colours of the editor skin in use.</summary>
		public static StyleSheet Skin => EditorGUIUtility.isProSkin ? Dark : Light;

		/// <summary>
		/// Adds the toolbox's class and style sheets to the element. Safe to call more than once and on nested
		/// elements.
		/// </summary>
		public static void Apply(VisualElement root)
		{
			if (root == null)
				throw new ArgumentNullException(nameof(root));
			root.AddToClassList(RootClass);
			var baseSheet = Base;
			var skinSheet = Skin;
			if (!root.styleSheets.Contains(baseSheet))
				root.styleSheets.Add(baseSheet);
			if (!root.styleSheets.Contains(skinSheet))
				root.styleSheets.Add(skinSheet);
		}

		private static StyleSheet Load(string guid, string fileName)
		{
			var path = AssetDatabase.GUIDToAssetPath(guid);
			if (string.IsNullOrEmpty(path))
				throw new InvalidOperationException($"The style sheet '{fileName}' of the inspector toolbox was not found by its GUID '{guid}'. Its .meta file must keep that GUID.");
			var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
			if (sheet == null)
				throw new InvalidOperationException($"The asset at '{path}' could not be loaded as a style sheet.");
			return sheet;
		}
	}

}
