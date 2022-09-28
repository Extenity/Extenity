#if ExtenityMessenger && UseLegacyMessenger

using UnityEngine;

namespace Extenity.MessagingToolbox
{

	public class MessengerHelper : MonoBehaviour
	{
		#region Initialization / Deinitialization

		private static MessengerHelper Instance;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Initialize()
		{
			var go = new GameObject("_MessengerHelper");
			go.hideFlags = HideFlags.NotEditable;
			DontDestroyOnLoad(go);
			Instance = go.AddComponent<MessengerHelper>();

#if UNITY_EDITOR

			// Set to destroy when quitting from Play mode in Editor.
			UnityEditor.EditorApplication.playModeStateChanged += OnEditorApplicationOnplayModeStateChanged;

			void OnEditorApplicationOnplayModeStateChanged(UnityEditor.PlayModeStateChange change)
			{
				if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode)
				{
					UnityEditor.EditorApplication.playModeStateChanged -= OnEditorApplicationOnplayModeStateChanged;
					if (Instance)
					{
						Destroy(Instance.gameObject);
					}
				}
			}

#endif
		}

		#endregion

		#region Builtin Events

		protected void OnApplicationQuit()
		{
			Messenger.EmitEvent(BaseMessages.OnApplicationQuit);
		}

		protected void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus)
			{
				Messenger.EmitEvent(BaseMessages.OnApplicationPaused);
			}
			else
			{
				Messenger.EmitEvent(BaseMessages.OnApplicationResumed);
			}
		}

		#endregion
	}

}

#endif
