//#define EnableDeferredSaveLogging

using System;
using Extenity.ApplicationToolbox;
using Extenity.DesignPatternsToolbox;
using Extenity.FlowToolbox;
#if UNITY
using UnityEngine;
#endif

namespace Extenity.DataToolbox
{

	public enum PathHashPostfix
	{
		No = 0,
		Yes = 1,
		OnlyInEditor = 2,
	}

#if UNITY

	public class DeferredSaveHelper : AutoSingletonUnity<DeferredSaveHelper>
	{
		protected override void AwakeDerived()
		{
			DontDestroyOnLoad(this);
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

#endif

	public static class PlayerPrefsTools
	{
		public static void SetBool(string key, bool value)
		{
#if UNITY
			PlayerPrefs.SetInt(key, value ? 1 : 0);
#else
			throw new System.NotImplementedException();
#endif
		}

		public static bool GetBool(string key, bool defaultValue = default(bool))
		{
#if UNITY
			return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;
#else
			throw new System.NotImplementedException();
#endif
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
#if UNITY
					if (UnityEngine.Application.isEditor)
						return key + PathHash;
					else
						return key;
#else
					throw new System.NotImplementedException();
#endif
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion

		#region Deferred Save

#pragma warning disable CS0414
		private static float DeferredSaveTriggerTime = -1f;
#pragma warning restore CS0414

		/// <summary>
		/// Triggers a delayed save operation. If triggered again consecutively, the delay will be set to the closest time.
		/// </summary>
		/// <param name="delay"></param>
		public static void DeferredSave(float delay)
		{
#if UNITY
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
#else
			throw new System.NotImplementedException();
#endif
		}

		private static void OnTimeToSave()
		{
#if UNITY
#if EnableDeferredSaveLogging
			Log.Info($"Deferred saving triggered at '{Time.unscaledTime}'");
#endif
			DeferredSaveTriggerTime = -1f;
			PlayerPrefs.Save();
#else
			throw new System.NotImplementedException();
#endif
		}

		#endregion
	}

}
