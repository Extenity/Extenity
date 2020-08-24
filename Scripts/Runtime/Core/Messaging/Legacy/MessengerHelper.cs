#if UseLegacyMessenger

using UnityEngine;

namespace Extenity.MessagingToolbox
{

	public class MessengerHelper : MonoBehaviour
	{
		#region Initialize

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Initialize()
		{
			var go = new GameObject("_MessengerHelper");
			go.hideFlags = HideFlags.DontSave;
			DontDestroyOnLoad(go);
			go.AddComponent<MessengerHelper>();
		}

		#endregion

		#region Builtin Events

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
