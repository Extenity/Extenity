using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.DesignPatternsToolbox;
using Extenity.MessagingToolbox;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.UIToolbox.TouchInput
{

	public class TouchInputManager : SingletonUnity<TouchInputManager>
	{
		#region Initialization

		protected void Awake()
		{
			InitializeSingleton(true);
		}

		#endregion

		#region Schemes

		[ListDrawerSettings(Expanded = true, OnBeginListElementGUI = nameof(_BeginListEntry), OnEndListElementGUI = nameof(_EndListEntry))]
		[ValidateInput(nameof(ValidateAvailableInputSchemeNames), "Available Input Schemes should not contain an empty name and should contain at least one name.", InfoMessageType.Error)]
		public List<string> AvailableInputSchemeNames = new List<string>
		{
			"Default",
		};

#if UNITY_EDITOR

		private void _BeginListEntry(int index)
		{
			GUILayout.BeginHorizontal();
		}

		private void _EndListEntry(int index)
		{
			var scheme = AvailableInputSchemeNames[index];
			var scene = gameObject.scene;

			var isDisabled =
				!Application.isPlaying ||
				!scene.IsValid() ||
				UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(scene) ||
				(!string.IsNullOrWhiteSpace(CurrentScheme) && CurrentScheme.Equals(scheme, StringComparison.OrdinalIgnoreCase));

			UnityEditor.EditorGUI.BeginDisabledGroup(isDisabled);
			if (GUILayout.Button("Switch"))
			{
				ChangeInputScheme(scheme);
			}
			UnityEditor.EditorGUI.EndDisabledGroup();

			GUILayout.EndHorizontal();
		}

#endif

		#endregion

		#region Change Input Scheme

		public string CurrentScheme { get; private set; }

		public static readonly ExtenityEvent<string> OnInputSchemeChanged = new ExtenityEvent<string>();

		public void ChangeInputScheme(string newScheme)
		{
			if (newScheme.EqualsOrBothEmpty(CurrentScheme, StringComparison.OrdinalIgnoreCase))
				return;

			if (!AvailableInputSchemeNames.Contains(newScheme))
			{
				throw new Exception($"Input scheme '{newScheme}' is not defined.");
			}

			CurrentScheme = newScheme;
			OnInputSchemeChanged.Invoke(newScheme);
		}

#if UNITY_EDITOR

		private bool ValidateAvailableInputSchemeNames(List<string> list)
		{
			if (list == null || list.Count == 0)
				return false;
			return !list.Any(string.IsNullOrWhiteSpace);
		}

#endif

		#endregion
	}

}
