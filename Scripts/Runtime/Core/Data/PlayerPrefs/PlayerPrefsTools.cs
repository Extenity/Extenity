//#define EnableDeferredSaveLogging

using System;
using Extenity.ApplicationToolbox;
using Extenity.DesignPatternsToolbox;
using Extenity.FlowToolbox;
using UnityEngine;

namespace Extenity.DataToolbox
{

	public enum PathHashPostfix
	{
		No = 0,
		Yes = 1,
		OnlyInEditor = 2,
	}

	public class DeferredSaveHelper : AutoSingletonUnity<DeferredSaveHelper>
	{
		protected void Awake()
		{
			InitializeSingleton(this, true);
		}

		#region Test

		// Test tools. Keep them here commented out for future needs.
		/*
		protected void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				DeferredSave(2f);
			}
			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				DeferredSave(5f);
			}
			if (Input.GetKeyDown(KeyCode.Alpha9))
			{
				DeferredSave(9f);
			}
		}
		*/

		#endregion
	}

	public static class PlayerPrefsTools
	{
		public static void SetBool(string key, bool value)
		{
			PlayerPrefs.SetInt(key, value ? 1 : 0);
		}

		public static bool GetBool(string key, bool defaultValue = default(bool))
		{
			return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;
		}

		#region Path Hash Postfix

		private static string _PathHash;
		public static string PathHash
		{
			get
			{
				if (string.IsNullOrEmpty(_PathHash))
				{
					_PathHash = "-" + ApplicationTools.PathHash;
				}
				return _PathHash;
			}
		}

		#endregion

		#region Generate Key

		public static string GenerateKey(string key, PathHashPostfix appendPathHashToKey)
		{
			switch (appendPathHashToKey)
			{
				case PathHashPostfix.No:
					return key;
				case PathHashPostfix.Yes:
					return key + PathHash;
				case PathHashPostfix.OnlyInEditor:
					if (Application.isEditor)
						return key + PathHash;
					else
						return key;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion

		#region Deferred Save

		private static float DeferredSaveTriggerTime = -1f;

		/// <summary>
		/// Triggers a delayed save operation. If triggered again consecutively, the delay will be set to the closest time.
		/// </summary>
		/// <param name="delay"></param>
		public static void DeferredSave(float delay)
		{
			var now = Time.unscaledTime;

			if (DeferredSaveTriggerTime > 0f)
			{
				// See if which one is bigger and choose the closest one.
				var remainingTimeOfOngoingOperation = DeferredSaveTriggerTime - now;
				if (delay < remainingTimeOfOngoingOperation)
				{
					DeferredSaveHelper.Instance.CancelFastInvoke(OnTimeToSave); // Cancel the previous call first.
				}
				else
				{
					// No need to do anything. Ignore current deferred save request because the ongoing operation is expected to be completed even sooner.
					return;
				}
			}

			DeferredSaveTriggerTime = now + delay;
#if EnableDeferredSaveLogging
			Log.Info($"Deferred save with a delay of '{delay}' is set for '{DeferredSaveTriggerTime}'");
#endif
			DeferredSaveHelper.Instance.FastInvoke(OnTimeToSave, delay, true);
		}

		private static void OnTimeToSave()
		{
#if EnableDeferredSaveLogging
			Log.Info($"Deferred saving triggered at '{Time.unscaledTime}'");
#endif
			DeferredSaveTriggerTime = -1f;
			PlayerPrefs.Save();
		}

		#endregion
	}

}
