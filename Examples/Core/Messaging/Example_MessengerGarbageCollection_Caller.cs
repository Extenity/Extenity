#if ExtenityMessenger && !UseLegacyMessenger

using UnityEngine;
using Extenity.MessagingToolbox;

namespace Extenity.Messaging
{

	public class Example_MessengerGarbageCollection_Caller : MonoBehaviour
	{
		protected void Awake()
		{
			Log.Info("Press F3 to emit a test message.", this);
			Log.Info("Press F4 to destroy listener component.", this);
			Log.Info("Press F5 to destroy listener component using DestroyImmediate.", this);
			Log.Info("Press F8 to create a new listener component (Don't instantiate multiple listeners to test F4 and F5 or logs will be confusing).", this);
			Log.Info("Press F12 to list all registered listeners.");
		}

		protected void Update()
		{
			if (Input.GetKeyDown(KeyCode.F3))
			{
				Global.EmitMessage("1001", "Test message...");
			}
			if (Input.GetKeyDown(KeyCode.F4))
			{
				Global.EmitMessage("1001", "This message is emitted before calling Destroy");
				Destroy(GetComponent<Example_MessengerGarbageCollection_Listener>());
				Global.EmitMessage("1001", "This message is emitted after Destroy which should be delivered to MyMessageHandler since the object is still alive and OnDestroy is not called yet");
			}
			if (Input.GetKeyDown(KeyCode.F5))
			{
				Global.EmitMessage("1001", "This message is emitted before calling DestroyImmediate");
				DestroyImmediate(GetComponent<Example_MessengerGarbageCollection_Listener>());
				Global.EmitMessage("1001", "This message is emitted after DestroyImmediate which should NOT be delivered to MyMessageHandler");
			}
			if (Input.GetKeyDown(KeyCode.F8))
			{
				gameObject.AddComponent<Example_MessengerGarbageCollection_Listener>();
			}
			if (Input.GetKeyDown(KeyCode.F12))
			{
				Global.DebugLogListAllMessageListeners();
			}
		}

		private Messenger _Global;
		public Messenger Global
		{
			get
			{
				if (!_Global)
				{
					var go = new GameObject("_GlobalMessenger", typeof(Messenger));
					go.hideFlags = HideFlags.HideAndDontSave;
					_Global = go.GetComponent<Messenger>();
				}
				return _Global;
			}
		}
	}

}

#endif
