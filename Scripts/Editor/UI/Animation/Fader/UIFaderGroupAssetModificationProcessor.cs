using System;
using UnityEditor;
using UnityEngine;

namespace Extenity.UIToolbox.Editor
{

	/// <summary>
	/// Responsible for ForceHiddenAndUntouched to work when saving UI prefabs.
	/// </summary>
	class UIFaderGroupAssetModificationProcessor : UnityEditor.AssetModificationProcessor
	{
		static string[] OnWillSaveAssets(string[] paths)
		{
			foreach (string path in paths)
			{
				if (path.EndsWith(".prefab", StringComparison.InvariantCultureIgnoreCase))
				{
					var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
					if (gameObject)
					{
						var faderGroups = gameObject.GetComponentsInChildren<UIFaderGroup>();
						foreach (var faderGroup in faderGroups)
						{
							if (faderGroup.ForceHiddenAndUntouched)
							{
								foreach (var fader in faderGroup.Faders)
								{
									fader.InitialState = FadeState.Untouched;
									fader.FadeOutImmediate();
								}
							}
						}
					}
				}
			}
			return paths;
		}
	}

}
