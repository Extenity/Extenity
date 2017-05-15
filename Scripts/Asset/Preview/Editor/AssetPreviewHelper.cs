using System;
using UnityEditor;
using UnityEngine;

namespace Extenity.Asset
{

	public class AssetPreviewHelper : MonoBehaviour
	{
		// TODO: Optimize
		/// <summary>
		/// Decompiled from method CreatePreviewForAsset of class UnityEditor.AssetPreviewUpdater.
		/// </summary>
		public static Texture2D CreatePreviewForAsset(UnityEngine.Object obj, UnityEngine.Object[] subAssets, string assetPath, int width, int height)
		{
			if (obj == null)
				return null;
			//Type customEditorType = CustomEditorAttributes.FindCustomEditorType(obj, false);
			var customEditorType = FindCustomEditorType(obj, false);
			if (customEditorType == null)
				return null;

			var method = customEditorType.GetMethod("RenderStaticPreview");
			if (method == null)
			{
				Debug.LogError((object)"Fail to find RenderStaticPreview base method");
				return null;
			}
			if (method.DeclaringType == typeof(Editor))
				return null;

			var editor = Editor.CreateEditor(obj);
			if (editor == null)
				return null;
			var texture2D = editor.RenderStaticPreview(assetPath, subAssets, width, height);
			UnityEngine.Object.DestroyImmediate(editor);

			return texture2D;
		}

		#region Reflection

		static AssetPreviewHelper()
		{
			CustomEditorAttributesType = typeof(AssetDatabase).Assembly.GetType("UnityEditor.CustomEditorAttributes", false);

			CustomEditorAttributesType.GetStaticMethodAsFunc("FindCustomEditorType", out _FindCustomEditorType);
		}

		public static readonly Type CustomEditorAttributesType;

		private static readonly Func<UnityEngine.Object, bool, Type> _FindCustomEditorType;
		public static Type FindCustomEditorType(UnityEngine.Object o, bool multiEdit) { return _FindCustomEditorType(o, multiEdit); }

		#endregion
	}

}
