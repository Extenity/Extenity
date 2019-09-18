using UnityEngine;
using Extenity.MessagingToolbox;

namespace Extenity.Messaging
{

	public class Test_MessengerGarbageCollection_Caller : MonoBehaviour
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
				Messenger.Global.Emit("1001", "Test message...");
			}
			if (Input.GetKeyDown(KeyCode.F4))
			{
				Messenger.Global.Emit("1001", "This message is emitted before calling Destroy");
				Destroy(GetComponent<Test_MessengerGarbageCollection_Listener>());
				Messenger.Global.Emit("1001", "This message is emitted after Destroy which should be delivered to MyMessageHandler since the object is still alive and OnDestroy is not called yet");
			}
			if (Input.GetKeyDown(KeyCode.F5))
			{
				Messenger.Global.Emit("1001", "This message is emitted before calling DestroyImmediate");
				DestroyImmediate(GetComponent<Test_MessengerGarbageCollection_Listener>());
				Messenger.Global.Emit("1001", "This message is emitted after DestroyImmediate which should NOT be delivered to MyMessageHandler");
			}
			if (Input.GetKeyDown(KeyCode.F8))
			{
				gameObject.AddComponent<Test_MessengerGarbageCollection_Listener>();
			}
			if (Input.GetKeyDown(KeyCode.F12))
			{
				Messenger.Global.DebugLogListAllListeners();
			}
		}
	}

}
