using System;
using System.Reflection;
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
			//System.Type customEditorType = CustomEditorAttributes.FindCustomEditorType(obj, false);
			var customEditorType = FindCustomEditorType(obj, false);
			if (customEditorType == null)
				return null;
			MethodInfo method = customEditorType.GetMethod("RenderStaticPreview");
			if (method == null)
			{
				Debug.LogError((object)"Fail to find RenderStaticPreview base method");
				return (Texture2D)null;
			}
			if (method.DeclaringType == typeof(Editor))
				return (Texture2D)null;
			Editor editor = Editor.CreateEditor(obj);
			if ((UnityEngine.Object)editor == (UnityEngine.Object)null)
				return (Texture2D)null;
			Texture2D texture2D = editor.RenderStaticPreview(assetPath, subAssets, width, height);
			UnityEngine.Object.DestroyImmediate((UnityEngine.Object)editor);
			return texture2D;
		}

		#region Reflection

		private static Type _CustomEditorAttributesType;

		public static Type CustomEditorAttributesType
		{
			get
			{
				if (_CustomEditorAttributesType == null)
				{
					_CustomEditorAttributesType = typeof(AssetDatabase).Assembly.GetType("UnityEditor.CustomEditorAttributes", false);
					if (_CustomEditorAttributesType == null)
					{
						throw new Exception("CustomEditorAttributes type could not be found.");
					}
				}
				return _CustomEditorAttributesType;
			}
		}

		private static Func<UnityEngine.Object, bool, Type> _FindCustomEditorType;

		public static Func<UnityEngine.Object, bool, Type> FindCustomEditorType
		{
			get
			{
				if (_FindCustomEditorType == null)
				{
					CustomEditorAttributesType.GetStaticMethodAsFunc("FindCustomEditorType", out _FindCustomEditorType);
				}
				return _FindCustomEditorType;
			}
		}

		#endregion

	}

}
