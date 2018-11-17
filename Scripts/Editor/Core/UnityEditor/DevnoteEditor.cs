using Extenity.IMGUIToolbox;
using Extenity.TextureToolbox;
using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	[CustomEditor(typeof(Devnote))]
	public class DevnoteEditor : UnityEditor.Editor
	{
		private const float RequiredMouseScreenDistanceToObjectForDisplayingTheLabel = 100f;
		private static readonly Vector2 IconSize = new Vector2(50, 50);

		private Devnote Me;
		private bool IsDisplayingNoteLabel;

		//static DevnoteEditor()
		//{
		//	EditorApplication.delayCall += () =>
		//	{
		//		Log.Info("####");
		//	};
		//}

		private void Awake()
		{
			Me = (Devnote)target;

			SceneView.onSceneGUIDelegate += OnSceneGUIDelegate;
		}

		private void OnDisable()
		{
			SceneView.onSceneGUIDelegate -= OnSceneGUIDelegate;
		}

		//private void OnSceneGUI()
		private void OnSceneGUIDelegate(SceneView sceneView)
		{
			//GUILayout.BeginArea(new Rect());
			//Handles.BeginGUI();
			//Me.transform.position = Handles.DoPositionHandle(Me.transform.position, Me.transform.rotation);
			//Handles.EndGUI();
			//GUILayout.EndArea();
			//return;

			if (Event.current.modifiers != EventModifiers.None)
			{
				return;
			}

			var noteContent = new GUIContent(Me.Note);
			var camera = Camera.current;

			var objectScreenPosition = camera.WorldToScreenPoint(Me.transform.position);
			var objectDistanceToCamera = objectScreenPosition.z;

			if (objectDistanceToCamera < 0)
				return; // Object is behind the camera. So do not draw.

			var rect = new Rect(objectScreenPosition.x - IconSize.x / 2f, Screen.height - objectScreenPosition.y - IconSize.y / 2f, IconSize.x, IconSize.y);
			GUILayout.BeginArea(rect);
			if (GUILayout.Button(objectDistanceToCamera.ToString("N2"), GUILayoutTools.ExpandWidthAndHeight))
			{
				Selection.activeGameObject = Me.gameObject;
			}
			GUILayout.EndArea();

			// Label
			var mouseScreenPosition = (Vector2)Event.current.mousePosition;
			mouseScreenPosition.y = Screen.height - mouseScreenPosition.y;
			var mouseScreenDistanceToObject = (mouseScreenPosition - (Vector2)objectScreenPosition).magnitude;
			if (mouseScreenDistanceToObject < RequiredMouseScreenDistanceToObjectForDisplayingTheLabel)
			{
				if (!IsDisplayingNoteLabel)
				{
					sceneView.Repaint();
					IsDisplayingNoteLabel = true;
				}

				var noteLabelSize = NoteLabelStyle.CalcSize(noteContent);
				rect.x = objectScreenPosition.x - noteLabelSize.x / 2f;
				//rect.y += rect.width - 5; // Place to bottom
				rect.y += -noteLabelSize.y - 5; // Place to top
				rect.width = noteLabelSize.x;
				rect.height = noteLabelSize.y;
				GUI.Label(rect, noteContent, NoteLabelStyle);
			}
			else
			{
				if (IsDisplayingNoteLabel)
				{
					sceneView.Repaint();
					IsDisplayingNoteLabel = false;
				}
			}
		}

		#region Style

		private GUIStyle _NoteLabelStyle;
		private GUIStyle NoteLabelStyle
		{
			get
			{
				if (_NoteLabelStyle == null)
				{
					_NoteLabelStyle = new GUIStyle(EditorStyles.label);
					_NoteLabelStyle.alignment = TextAnchor.UpperLeft; // For some reason, UpperCenter does not work. So we do this by hand.
					_NoteLabelStyle.normal.background = TextureTools.CreateSimpleTexture(4, 4, new Color(0.5f, 0.1f, 0.2f, 0.8f));
					_NoteLabelStyle.padding = new RectOffset(10, 10, 10, 10);
				}
				return _NoteLabelStyle;
			}
		}

		#endregion
	}

}
