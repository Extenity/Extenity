using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public abstract class ExtenityEditorWindowBase : EditorWindow
	{
		#region OnGUI

		protected abstract void OnGUIDerived();

		protected void OnGUI()
		{
			CalculateRightMouseButtonScrolling();
			OnGUIDerived();
		}

		#endregion

		#region Title And Icon

		/// <summary>
		/// Call this inside OnEnable. Icon texture should have been set for DontDestroyOnLoad and HideAndDontSave set for hideFlags.
		/// </summary>
		public void SetTitleAndIcon(string title, Texture2D icon)
		{
			titleContent = new GUIContent(title, icon);
		}

		#endregion

		#region Scroll

		protected Vector2 ScrollPosition = Vector2.zero;

		#endregion

		#region Scroll Window With Right Mouse Button

		public bool IsRightMouseButtonScrollingEnabled = false;
		private bool WasScrollingWithRightMouseButton;

		private void CalculateRightMouseButtonScrolling()
		{
			if (Event.current.isMouse)
			{
				if (IsRightMouseButtonScrollingEnabled && Event.current.button == 1 && Event.current.type == EventType.MouseDrag)
				{
					ScrollPosition -= Event.current.delta;

					Event.current.Use();
					Repaint();

					WasScrollingWithRightMouseButton = true;
				}

				// Prevent any right click events if right click is used for scrolling.
				if (WasScrollingWithRightMouseButton && Event.current.type == EventType.MouseUp)
				{
					WasScrollingWithRightMouseButton = false;
					Event.current.Use();
					Repaint();
				}
			}
		}

		#endregion

		#region Repaint After Script Reload

		//[DidReloadScripts]
		//private static void InternalRepaintAfterReload()
		//{
		//	// TODO: This will be implemented along with registering all windows in a static list. So that we can iterate the list here and call Repaint on them. Note that there is also EditorApplication.isCompiling if this approach fails in a way.
		//}

		#endregion

		#region Layout

		public static readonly GUILayoutOption SmallButtonHeight = GUILayout.Height(18);
		public static readonly GUILayoutOption MediumButtonHeight = GUILayout.Height(24);
		public static readonly GUILayoutOption BigButtonHeight = GUILayout.Height(36f);

		#endregion

		#region Thread Safe Repaint

		public void ThreadSafeRepaint()
		{
			EditorApplication.delayCall += Repaint;
		}

		#endregion

		#region serializedObject

		// Name intentionally left small casing to make it compatible with Editor.serializedObject
		private SerializedObject _serializedObject;
		public SerializedObject serializedObject
		{
			get
			{
				if (_serializedObject == null)
				{
					_serializedObject = new SerializedObject(this);
				}
				return _serializedObject;
			}
		}

		#endregion
	}

}
