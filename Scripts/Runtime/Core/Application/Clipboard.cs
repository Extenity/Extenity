
namespace Extenity.ApplicationToolbox
{

	public abstract class Clipboard
	{
		#region Singleton

		private static Clipboard _Instance;
		public static Clipboard Instance
		{
			get
			{
				if (_Instance == null)
					throw new InternalException(113857182); // Clipboard system is not initialized yet
				return _Instance;
			}
			protected set
			{
				if (_Instance != null)
					throw new InternalException(112857182); // There already was an existing Clipboard system running
				_Instance = value;
			}
		}

		#endregion

		protected abstract void DoSetClipboardText(string text, bool log);
		protected abstract string DoGetClipboardText();

		public static void SetClipboardText(string text, bool log)
		{
			Instance.DoSetClipboardText(text, log);
		}

		public static string GetClipboardText()
		{
			return Instance.DoGetClipboardText();
		}
	}

}
