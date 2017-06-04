using System;

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
					throw new Exception("Internal error! Clipboard system is not initialized yet!");
				return _Instance;
			}
			protected set
			{
				if (_Instance != null)
					throw new Exception("Internal error! There already was an existing Clipboard system running!");
				_Instance = value;
			}
		}

		#endregion

		protected abstract void DoSetClipboardText(string text);
		protected abstract string DoGetClipboardText();

		public static void SetClipboardText(string text)
		{
			Instance.DoSetClipboardText(text);
		}

		public static string GetClipboardText()
		{
			return Instance.DoGetClipboardText();
		}
	}

}
