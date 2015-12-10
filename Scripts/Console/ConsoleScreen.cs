//#define DISABLE_CONSOLE

using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;


public class ConsoleScreen : SingletonUnity<ConsoleScreen>
{
	#region Message Struct

	public enum MessageTypes
	{
		Normal,
		Warning,
		Error,
	};

	private struct Message
	{
		public readonly string messageText;
		public readonly MessageTypes messageType;
		public readonly Color displayColor;
		public readonly bool useCustomColor;

		public Message(string messageText)
		{
			this.messageText = messageText;
			this.messageType = MessageTypes.Normal;
			this.displayColor = Color.black;
			this.useCustomColor = false;
		}

		public Message(string messageText, MessageTypes messageType)
		{
			this.messageText = messageText;
			this.messageType = messageType;
			this.displayColor = Color.black;
			this.useCustomColor = false;
		}

		public Message(string messageText, MessageTypes messageType, Color displayColor)
		{
			this.messageText = messageText;
			this.messageType = messageType;
			this.displayColor = displayColor;
			this.useCustomColor = true;
		}

		public Message(string messageText, Color displayColor)
		{
			this.messageText = messageText;
			this.messageType = MessageTypes.Normal;
			this.displayColor = displayColor;
			this.useCustomColor = true;
		}

		public bool IsNormal { get { return messageType == MessageTypes.Normal; } }
		public bool IsError { get { return messageType == MessageTypes.Error; } }
		public bool IsWarning { get { return messageType == MessageTypes.Warning; } }

		public string TypeText
		{
			get
			{
				switch (messageType)
				{
					case MessageTypes.Normal: return "";
					case MessageTypes.Error: return "ERROR";
					case MessageTypes.Warning: return "WARNING";
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public Color Color
		{
			get
			{
				if (useCustomColor)
					return displayColor;

				if (IsError)
					return Instance.errorTextColor;

				if (IsWarning)
					return Instance.warningTextColor;

				return Instance.normalTextColor;
			}
		}
	}

	#endregion

	public int margin = 30;
	public KeyCode showHideKey = KeyCode.BackQuote;
	public bool showHideOnFourFingers = true;

	public Color normalTextColor = Color.white;
	public Color warningTextColor = Color.yellow;
	public Color errorTextColor = Color.red;

	public bool showConsoleOnError = true;
	public bool showConsoleOnWarning = true;

#if !DISABLE_CONSOLE
	private List<Message> messages;
	private StringBuilder copyableString;
	private bool isHidden = true;
	private bool isDisplayingCopyableText;
	private Vector2 scrollPosition = Vector2.zero;
	private int previousTouchCount;
#endif

	void Awake()
	{
#if !DISABLE_CONSOLE
		InitializeSingleton(this);

		messages = new List<Message>();

		showConsoleOnError = PlayerPrefsExt.GetBool("ShowConsoleOnError", showConsoleOnError);
		showConsoleOnWarning = PlayerPrefsExt.GetBool("ShowConsoleOnWarning", showConsoleOnWarning);

		EnableUnityLogCatching(true);
#endif
	}

	protected override void OnDestroy()
	{
#if !DISABLE_CONSOLE
		EnableUnityLogCatching(false);
		base.OnDestroy();
#endif
	}

	void Update()
	{
#if !DISABLE_CONSOLE
		if (Input.GetKeyDown(showHideKey))
		{
			ToggleHidden();
		}

		if (showHideOnFourFingers)
		{
			if (Input.touchCount == 4 && previousTouchCount != 4)
			{
				ToggleHidden();
			}
			previousTouchCount = Input.touchCount;
		}
#endif
	}

	private static void UnityLogCallback(string condition, string stackTrace, LogType type)
	{
		if (CheckFilter(condition))
			return;

		switch (type)
		{
			case LogType.Log:
				Instance.LogInternal(condition, MessageTypes.Normal, Color.black, false);
				break;
			case LogType.Warning:
				Instance.LogInternal(condition, MessageTypes.Warning, Color.black, false);
				break;
			case LogType.Assert:
			case LogType.Error:
			case LogType.Exception:
				Instance.LogInternal(condition, MessageTypes.Error, Color.black, false);
				break;
			default:
				throw new NotImplementedException();
		}
	}

	#region Text Filter

	private static List<string> excludeFiltersStartsWith;
	private static List<string> excludeFiltersEndsWith;
	private static List<string> excludeFiltersContains;

	public static void AddFilterStartsWith(string filterText)
	{
		if (excludeFiltersStartsWith == null)
			excludeFiltersStartsWith = new List<string>();

		excludeFiltersStartsWith.Add(filterText);
	}

	public static void AddFilterEndsWith(string filterText)
	{
		if (excludeFiltersEndsWith == null)
			excludeFiltersEndsWith = new List<string>();

		excludeFiltersEndsWith.Add(filterText);
	}

	public static void AddFilterContains(string filterText)
	{
		if (excludeFiltersContains == null)
			excludeFiltersContains = new List<string>();

		excludeFiltersContains.Add(filterText);
	}

	private static bool CheckFilter(string text)
	{
		if (excludeFiltersStartsWith != null)
		{
			for (int i = 0; i < excludeFiltersStartsWith.Count; i++)
			{
				if (text.StartsWith(excludeFiltersStartsWith[i]))
					return true;
			}
		}

		if (excludeFiltersEndsWith != null)
		{
			for (int i = 0; i < excludeFiltersEndsWith.Count; i++)
			{
				if (text.EndsWith(excludeFiltersEndsWith[i]))
					return true;
			}
		}

		if (excludeFiltersContains != null)
		{
			for (int i = 0; i < excludeFiltersContains.Count; i++)
			{
				if (text.Contains(excludeFiltersContains[i]))
					return true;
			}
		}

		return false;
	}

	#endregion

	#region GUI

	void OnGUI()
	{
#if !DISABLE_CONSOLE
		if (isHidden)
			return;

		Rect rect = new Rect(margin, margin, Screen.width - margin * 2, Screen.height - margin * 2);

		GUI.Box(rect, "");
		GUI.Box(rect, "");
		GUI.Box(rect, "");
		GUI.Box(rect, "");

		GUI.Window(-9246, rect, ConsoleWindow, "Console");

		GUI.BringWindowToFront(-9246);
#endif
	}

	private void ConsoleWindow(int windowID)
	{
#if !DISABLE_CONSOLE
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();

		scrollPosition = GUILayout.BeginScrollView(scrollPosition);

		if (isDisplayingCopyableText)
		{
			BuildCopyableString();
			GUILayout.TextArea(copyableString.ToString(), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true), GUILayout.Width(255.0F));
		}
		else
		{
			DrawMessages();
		}

		GUILayout.EndScrollView();

		GUILayout.Space(5);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		if (GUILayout.Button("Show Copyable", GUILayout.Width(120), GUILayout.Height(40)))
		{
			isDisplayingCopyableText = !isDisplayingCopyableText;
		}

		if (GUILayout.Button("Close", GUILayout.Width(120), GUILayout.Height(40)))
		{
			HideConsole();
		}

		GUILayout.FlexibleSpace();

		GUILayout.BeginVertical();

		bool showConsoleOnErrorNew = GUILayout.Toggle(showConsoleOnError,
			"Show on error", GUILayout.Height(20));
		if (showConsoleOnError != showConsoleOnErrorNew)
		{
			showConsoleOnError = showConsoleOnErrorNew;
			PlayerPrefsExt.SetBool("ShowConsoleOnError", showConsoleOnError);
		}

		bool showConsoleOnWarningNew = GUILayout.Toggle(showConsoleOnWarning,
			"Show on warning", GUILayout.Height(20));
		if (showConsoleOnWarning != showConsoleOnWarningNew)
		{
			showConsoleOnWarning = showConsoleOnWarningNew;
			PlayerPrefsExt.SetBool("ShowConsoleOnWarning", showConsoleOnWarning);
		}

		GUILayout.EndVertical();

		//GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
#endif
	}

	private void DrawMessages()
	{
#if !DISABLE_CONSOLE
		if (messages == null)
			return;

		for (int i = 0; i < messages.Count; i++)
		{
			var message = messages[i];
			Color oldColor = GUI.color;
			GUI.color = message.Color;
			GUILayout.Label(message.messageText);
			GUI.color = oldColor;
		}
#endif
	}

	private void BuildCopyableString()
	{
#if !DISABLE_CONSOLE
		if (messages == null)
			return;

		copyableString = new StringBuilder();

		for (int i = 0; i < messages.Count; i++)
		{
			var message = messages[i];

			if (message.IsNormal)
			{
				copyableString.AppendLine(message.messageText);
			}
			else
			{
				copyableString.AppendLine(message.TypeText + ": " + message.messageText);
			}
		}
#endif
	}

	#endregion

	#region Usage

	public static void ShowConsole()
	{
#if !DISABLE_CONSOLE
		Instance.isHidden = false;
#endif
	}

	public static void HideConsole()
	{
#if !DISABLE_CONSOLE
		Instance.isHidden = true;
#endif
	}

	public static void ToggleHidden()
	{
#if !DISABLE_CONSOLE
		Instance.isHidden = !Instance.isHidden;
#endif
	}

	public static bool IsHidden
	{
		get
		{
#if !DISABLE_CONSOLE
			return Instance.isHidden;
#else
			return false;
#endif
		}
	}

	public static void EnableUnityLogCatching(bool enable)
	{
#if !DISABLE_CONSOLE
		if (enable)
		{
			UnityEngine.Application.logMessageReceived += UnityLogCallback;
		}
		else
		{
			UnityEngine.Application.logMessageReceived -= UnityLogCallback;
		}
#endif
	}

	public static void Clear()
	{
#if !DISABLE_CONSOLE
		Instance.ClearInternal();
#endif
	}

	public static void Log(string text)
	{
#if !DISABLE_CONSOLE
		Instance.LogInternal(text, MessageTypes.Normal, Color.black, false);
#endif
	}

	public static void Log(string text, MessageTypes messageType)
	{
#if !DISABLE_CONSOLE
		Instance.LogInternal(text, messageType, Color.black, false);
#endif
	}

	public static void Log(string text, Color displayColor)
	{
#if !DISABLE_CONSOLE
		Instance.LogInternal(text, MessageTypes.Normal, displayColor, true);
#endif
	}

	public static void Log(string text, MessageTypes messageType, Color displayColor)
	{
#if !DISABLE_CONSOLE
		Instance.LogInternal(text, messageType, displayColor, true);
#endif
	}

	public static void LogWarning(string text)
	{
#if !DISABLE_CONSOLE
		Instance.LogInternal(text, MessageTypes.Warning, Color.black, false);
#endif
	}

	public static void LogError(string text)
	{
#if !DISABLE_CONSOLE
		Instance.LogInternal(text, MessageTypes.Error, Color.black, false);
#endif
	}

	#endregion

	#region Usage Internal

	protected void ClearInternal()
	{
#if !DISABLE_CONSOLE
		messages.Clear();
#endif
	}

	protected void LogInternal(string message, MessageTypes messageType, Color displayColor, bool useCustomColor)
	{
#if !DISABLE_CONSOLE
		if (messages == null)
			return;

		if (messageType == MessageTypes.Error || messageType == MessageTypes.Warning)
		{
			message += "\n" + DebugReflection.StackTraceToString(6);

			if ((showConsoleOnError && messageType == MessageTypes.Error) ||
				(showConsoleOnWarning && messageType == MessageTypes.Warning))
			{
				ShowConsole();
			}
		}

		messages.Add(useCustomColor ? new Message(message, messageType, displayColor) : new Message(message, messageType));

		scrollPosition = new Vector2(scrollPosition.x, 50000.0F);
#endif
	}

	#endregion
}
