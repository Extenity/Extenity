using UnityEngine;
using UnityEditor;
using System;
using Extenity.DataToolbox;
using Extenity.ReflectionToolbox;

namespace Extenity.UnityEditorToolbox.Editor
{

	[Serializable]
	public sealed class WebView : ScriptableObject
	{
		#region Initialization

		static WebView()
		{
			var editorAssembly = typeof(UnityEditor.Editor).Assembly;
			var editorWindowType = typeof(EditorWindow);
			var webViewType = editorAssembly.GetType("UnityEditor.WebView");
			var webViewV8CallbackCSharpType = editorAssembly.GetType("UnityEditor.WebViewV8CallbackCSharp");
			var guiViewType = editorAssembly.GetType("UnityEditor.GUIView");
			if (webViewType == null)
				throw new NullReferenceException();
			if (webViewV8CallbackCSharpType == null)
				throw new NullReferenceException();
			if (guiViewType == null)
				throw new NullReferenceException();

			webViewV8CallbackCSharpType.GetMethodAsAction("Callback", out Callback);

			editorWindowType.GetFieldAsFunc("m_Parent", out GetView);

			webViewType.GetMethodAsAction("DestroyWebView", out _DestroyWebView);
			webViewType.GetMethodAsAction("InitWebView", out _InitWebView, new[] { guiViewType, typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool) });
			webViewType.GetMethodAsAction("ExecuteJavascript", out _ExecuteJavascript);
			webViewType.GetMethodAsAction("LoadURL", out _LoadURL);
			webViewType.GetMethodAsAction("LoadFile", out _LoadFile);
			webViewType.GetMethodAsFunc("DefineScriptObject", out _DefineScriptObject);
			webViewType.GetMethodAsAction("SetDelegateObject", out _SetDelegateObject);
			webViewType.GetMethodAsAction("SetHostView", out _SetHostView, new[] { guiViewType });
			webViewType.GetMethodAsAction("SetSizeAndPosition", out _SetSizeAndPosition);
			webViewType.GetMethodAsAction("SetFocus", out _SetFocus);
			webViewType.GetMethodAsFunc("HasApplicationFocus", out _HasApplicationFocus);
			webViewType.GetMethodAsAction("SetApplicationFocus", out _SetApplicationFocus);
			webViewType.GetMethodAsAction("Show", out _Show);
			webViewType.GetMethodAsAction("Hide", out _Hide);
			webViewType.GetMethodAsAction("Back", out _Back);
			webViewType.GetMethodAsAction("Forward", out _Forward);
			webViewType.GetMethodAsAction("SendOnEvent", out _SendOnEvent);
			webViewType.GetMethodAsAction("Reload", out _Reload);
			webViewType.GetMethodAsAction("AllowRightClickMenu", out _AllowRightClickMenu);
			webViewType.GetMethodAsAction("ShowDevTools", out _ShowDevTools);
			webViewType.GetMethodAsAction("ToggleMaximize", out _ToggleMaximize);
		}

		public void Initialize(object view, Rect rect, bool showResizeHandle)
		{
			CreateUnityWebViewIfRequired();

			InitWebView(view, (int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height, showResizeHandle);
		}

		#endregion

		#region Deinitialization

		public void OnDestroy()
		{
			if (UnityWebView != null)
			{
				SetHostView(null);
				DestroyImmediate(UnityWebView);
				UnityWebView = null;
			}
		}

		#endregion

		#region Unity WebView Instance

		private ScriptableObject UnityWebView;

		private void CreateUnityWebViewIfRequired()
		{
			if (UnityWebView == null)
			{
				var webViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.WebView");
				UnityWebView = ScriptableObject.CreateInstance(webViewType);
				UnityWebView.hideFlags = HideFlags.HideAndDontSave;
			}
		}

		#endregion

		#region Internals - EditorWindow

		public static InstanceFunc<object, object> GetView;

		#endregion

		#region Internals - UnityEditor.WebViewV8CallbackCSharp

		public static InstanceAction<object, string> Callback;

		#endregion

		#region Internals - UnityEditor.WebView

		// DestroyWebView
		// InitWebView
		// ExecuteJavascript
		// LoadURL
		// LoadFile
		// DefineScriptObject
		// SetDelegateObject
		// SetHostView
		// SetSizeAndPosition
		// SetFocus
		// HasApplicationFocus
		// SetApplicationFocus
		// Show
		// Hide
		// Back
		// Forward
		// SendOnEvent
		// Reload
		// AllowRightClickMenu
		// ShowDevTools
		// ToggleMaximize

		private static InstanceAction<ScriptableObject> _DestroyWebView;
		private static InstanceAction<ScriptableObject, object, int, int, int, int, bool> _InitWebView;
		private static InstanceAction<ScriptableObject, string> _ExecuteJavascript;
		private static InstanceAction<ScriptableObject, string> _LoadURL;
		private static InstanceAction<ScriptableObject, string> _LoadFile;
		private static InstanceFunc<ScriptableObject, string, ScriptableObject, bool> _DefineScriptObject;
		private static InstanceAction<ScriptableObject, ScriptableObject> _SetDelegateObject;
		private static InstanceAction<ScriptableObject, object> _SetHostView;
		private static InstanceAction<ScriptableObject, int, int, int, int> _SetSizeAndPosition;
		private static InstanceAction<ScriptableObject, bool> _SetFocus;
		private static InstanceFunc<ScriptableObject, bool> _HasApplicationFocus;
		private static InstanceAction<ScriptableObject, bool> _SetApplicationFocus;
		private static InstanceAction<ScriptableObject> _Show;
		private static InstanceAction<ScriptableObject> _Hide;
		private static InstanceAction<ScriptableObject> _Back;
		private static InstanceAction<ScriptableObject> _Forward;
		private static InstanceAction<ScriptableObject, string> _SendOnEvent;
		private static InstanceAction<ScriptableObject> _Reload;
		private static InstanceAction<ScriptableObject, bool> _AllowRightClickMenu;
		private static InstanceAction<ScriptableObject> _ShowDevTools;
		private static InstanceAction<ScriptableObject> _ToggleMaximize;

		public void DestroyWebView() { _DestroyWebView(UnityWebView); }
		private void InitWebView(object host, int x, int y, int width, int height, bool showResizeHandle) { _InitWebView(UnityWebView, host, x, y, width, height, showResizeHandle); }
		public void ExecuteJavascript(string scriptCode) { _ExecuteJavascript(UnityWebView, scriptCode); }
		public void LoadURL(string url) { _LoadURL(UnityWebView, url); }
		public void LoadFile(string path) { _LoadFile(UnityWebView, path); }
		public bool DefineScriptObject(string path, ScriptableObject obj) { return _DefineScriptObject(UnityWebView, path, obj); }
		public void SetDelegateObject(ScriptableObject value) { _SetDelegateObject(UnityWebView, value); }
		public void SetHostView(object view) { _SetHostView(UnityWebView, view); }
		public void SetSizeAndPosition(int x, int y, int width, int height) { _SetSizeAndPosition(UnityWebView, x, y, width, height); }
		public void SetFocus(bool value) { _SetFocus(UnityWebView, value); }
		public bool HasApplicationFocus() { return _HasApplicationFocus(UnityWebView); }
		public void SetApplicationFocus(bool applicationFocus) { _SetApplicationFocus(UnityWebView, applicationFocus); }
		public void Show() { _Show(UnityWebView); }
		public void Hide() { _Hide(UnityWebView); }
		public void Back() { _Back(UnityWebView); }
		public void Forward() { _Forward(UnityWebView); }
		public void SendOnEvent(string jsonStr) { _SendOnEvent(UnityWebView, jsonStr); }
		public void Reload() { _Reload(UnityWebView); }
		public void AllowRightClickMenu(bool allowRightClickMenu) { _AllowRightClickMenu(UnityWebView, allowRightClickMenu); }
		public void ShowDevTools() { _ShowDevTools(UnityWebView); }
		public void ToggleMaximize() { _ToggleMaximize(UnityWebView); }

		public void SetSizeAndPosition(Rect rect) { SetSizeAndPosition((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height); }

		#endregion
	}

}
