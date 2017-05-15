using System;
using Extenity.DataTypes;
using UnityEngine;

namespace Extenity.ConsoleToolbox
{

	public class TextPad : SingletonUnity<TextPad>
	{
		[Serializable]
		public struct TextPadEntry
		{
			public float time;
			public string text;

			public TextPadEntry(float time, string text)
			{
				this.time = time;
				this.text = text;
			}
		}

		public int leftMargin = 5;
		public int bottomMargin = 90;
		public int textHeight = 20;
		public Font font;
		public Color fontColor = Color.yellow;
		public int fontSize = 16;
		public FontStyle fontStyle = FontStyle.Bold;

		private const int historyCount = 30;
		private const int showRecentCount = 8;
		private const float showRecentTime = 5f;
		private const int maxTextCharacters = 100;

		public TextPadEntry[] history;

		public bool EnableLogCallbackRegister = false;
		private bool IsLogCallbackRegistered;

		private int lastAddedIndex;
		private bool historyOpened;
		private Rect rect; // Cache optimization

		private GUIStyle textStyle;


		void Awake()
		{
			InitializeSingleton(this, true);

			rect = new Rect(leftMargin, 0, 0, textHeight);

			textStyle = new GUIStyle();

			if (EnableLogCallbackRegister)
			{
				UnityEngine.Application.logMessageReceived += LogCallback;
				IsLogCallbackRegistered = true;
			}

			Clear();
		}

		protected override void OnDestroy()
		{
			if (IsLogCallbackRegistered)
			{
				UnityEngine.Application.logMessageReceived -= LogCallback;
				IsLogCallbackRegistered = false;
			}

			base.OnDestroy();
		}

		private void LogCallback(string condition, string stackTrace, LogType type)
		{
			Write(condition);
		}

		public void Clear()
		{
			lastAddedIndex = historyCount - 1; // To make it loop to starting index on first text.

			history = new TextPadEntry[historyCount];

			for (int i = 0; i < historyCount; i++)
			{
				history[i] = new TextPadEntry(-10000f, "");
			}
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				historyOpened = !historyOpened;
			}
		}

		void OnGUI()
		{
			textStyle.font = font;
			textStyle.fontSize = fontSize;
			textStyle.fontStyle = fontStyle;
			textStyle.normal.textColor = fontColor;

			if (historyOpened)
				DrawHistory();
			else
				DrawRecent();
		}

		void DrawHistory()
		{
			rect.y = Screen.height - bottomMargin;
			rect.width = Screen.width;

			int iDraw = lastAddedIndex;
			for (int i = 0; i < historyCount; i++)
			{
				GUI.Label(rect, history[iDraw].text, textStyle);
				rect.y -= rect.height;

				if (--iDraw < 0)
					iDraw = historyCount - 1;
			}
		}

		void DrawRecent()
		{
			float currentTime = Time.time;
			rect.y = Screen.height - bottomMargin;
			rect.width = Screen.width;

			int iDraw = lastAddedIndex;
			for (int i = 0; i < showRecentCount; i++)
			{
				if (history[iDraw].time + showRecentTime > currentTime)
				{
					GUI.Label(rect, history[iDraw].text, textStyle);
					rect.y -= rect.height;
				}
				else
					break;

				if (--iDraw < 0)
					iDraw = historyCount - 1;
			}
		}

		public static void Write(string text, bool forwardToDebugLog = true)
		{
			Instance.AddTextInternal(text, forwardToDebugLog);
		}

		private void AddTextInternal(string text, bool forwardToDebugLog = true)
		{
			if (++lastAddedIndex >= historyCount)
			{
				lastAddedIndex = 0;
			}

			string addedText = text.ClipIfNecessary(maxTextCharacters);

			history[lastAddedIndex] = new TextPadEntry(Time.time, addedText);

			if (forwardToDebugLog)
			{
				Debug.Log(addedText);
			}
		}
	}

}

