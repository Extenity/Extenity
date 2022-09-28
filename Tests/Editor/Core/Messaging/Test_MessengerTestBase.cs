#if ExtenityMessenger && !UseLegacyMessenger

using Extenity.MessagingToolbox;
using Extenity.Testing;
using UnityEngine;

namespace ExtenityTests.MessagingToolbox
{

	public abstract class Test_MessengerTestBase : ExtenityTestBase
	{
		#region Deinitialization

		protected override void OnDeinitialize()
		{
			if (_TestMessenger)
			{
				GameObject.DestroyImmediate(_TestMessenger);
			}

			base.OnDeinitialize();
		}

		#endregion

		#region Test Messenger

		private Messenger _TestMessenger;
		public Messenger TestMessenger
		{
			get
			{
				if (!_TestMessenger)
				{
					var go = new GameObject("_TestMessenger", typeof(Messenger));
					// DontDestroyOnLoad(go); Nope!
					// go.hideFlags = HideFlags.HideAndDontSave; Nope!
					_TestMessenger = go.GetComponent<Messenger>();
				}
				return _TestMessenger;
			}
		}

		#endregion
	}

}

#endif
