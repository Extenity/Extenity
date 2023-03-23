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
		#region Schemes

		[ListDrawerSettings(Expanded = true, OnBeginListElementGUI = "_BeginListEntry", OnEndListElementGUI = "_EndListEntry")]
		[PropertySpace(20f, 10f)]
		[InfoBox("All scheme names that will be used in the application should be defined here.")]
		[ValidateInput("ValidateAvailableInputSchemeNames", "Available Input Schemes should not contain an empty name and should contain at least one name.", ContinuousValidationCheck = true)]
		public List<string> AvailableInputSchemeNames = new List<string>
		{
			"Default",
		};

		[PropertySpace(10f, 20f)]
		[InfoBox("The fallback scheme will be used when trying to switch to an input scheme that is not defined among Available Input Scheme Names.")]
		[ValidateInput("ValidateFallbackInputScheme", "Fallback Scheme is not defined among " + nameof(AvailableInputSchemeNames) + ".", ContinuousValidationCheck = true)]
		public string FallbackInputSchemeName = "Default";

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

		private bool ValidateAvailableInputSchemeNames(List<string> list)
		{
			if (list == null || list.Count == 0)
				return false;
			return !list.Any(string.IsNullOrWhiteSpace);
		}

		private bool ValidateFallbackInputScheme(string fallbackInputSchemeName)
		{
			return AvailableInputSchemeNames.Contains(fallbackInputSchemeName);
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

			// Select the Fallback Scheme if the specified one is unknown.
			var isInvalid = !AvailableInputSchemeNames.Any(entry => newScheme.Equals(entry, StringComparison.InvariantCultureIgnoreCase));
			if (isInvalid)
			{
				Log.Warning($"Input scheme '{newScheme}' is not defined. Falling back to '{FallbackInputSchemeName}'.");
				newScheme = FallbackInputSchemeName;
			}

			CurrentScheme = newScheme;
			OnInputSchemeChanged.InvokeSafe(newScheme);
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(TouchInputManager));

		#endregion
	}

}
