//#define EnableDeferredSaveLogging

using System;
using Extenity.ApplicationToolbox;
using Extenity.DesignPatternsToolbox;
using Extenity.FlowToolbox;
#if UNITY_5_3_OR_NEWER
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

	public static class PlayerPrefsTools
	{
		public static void SetBool(string key, bool value)
		{
#if UNITY_5_3_OR_NEWER
			PlayerPrefs.SetInt(key, value ? 1 : 0);
#else
			throw new System.NotImplementedException();
#endif
		}

		public static bool GetBool(string key, bool defaultValue = default(bool))
		{
#if UNITY_5_3_OR_NEWER
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
#if UNITY_5_3_OR_NEWER
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

		private static bool _IsDeferredSavePending;
#if UNITY_5_3_OR_NEWER
		private static double DeferredSaveTriggerTime;
#endif

		/// <summary>
		/// Whether a save requested with <see cref="DeferredSave"/> is still waiting to be written to disk.
		/// </summary>
		public static bool IsDeferredSavePending => _IsDeferredSavePending;

		/// <summary>
		/// Triggers a delayed save operation. If triggered again before the save happens, the closest of the two times
		/// is kept, so a stream of changes is written at most once per delay rather than being put off by every change.
		///
		/// A pending save is also written when the application quits, which in the Editor includes leaving Play Mode, and
		/// when <see cref="FlushDeferredSave"/> is called. Nothing is left pending from one Play Mode session to the next.
		/// </summary>
		public static void DeferredSave(float delay)
		{
#if UNITY_5_3_OR_NEWER
			var now = Loop.UnscaledTime;

			if (_IsDeferredSavePending)
			{
				// See if which one is bigger and choose the closest one.
				var remainingTimeOfOngoingOperation = DeferredSaveTriggerTime - now;
				if (delay >= remainingTimeOfOngoingOperation)
				{
					// No need to do anything. Ignore current deferred save request because the ongoing operation is expected to be completed even sooner.
					return;
				}
			}
			else
			{
				Loop.RegisterUpdateEvery100MillisecondsUnscaled(CheckDeferredSave);
				Application.quitting += FlushDeferredSave;
				_IsDeferredSavePending = true;
			}

			DeferredSaveTriggerTime = now + delay;
#if EnableDeferredSaveLogging
			Log.Info($"Deferred save with a delay of '{delay}' is set for '{DeferredSaveTriggerTime}'");
#endif
#else
			throw new System.NotImplementedException();
#endif
		}

#if UNITY_5_3_OR_NEWER
		private static void CheckDeferredSave()
		{
			if (Loop.UnscaledTime < DeferredSaveTriggerTime)
				return;

#if EnableDeferredSaveLogging
			Log.Info($"Deferred saving triggered at '{Loop.UnscaledTime}'");
#endif
			FlushDeferredSave();
		}
#endif

		/// <summary>
		/// Writes the save that <see cref="DeferredSave"/> left pending to disk now, for example when the screen that made
		/// the changes closes. Does nothing when no save is pending.
		/// </summary>
		public static void FlushDeferredSave()
		{
#if UNITY_5_3_OR_NEWER
			if (!_IsDeferredSavePending)
				return;

			_IsDeferredSavePending = false;
			Loop.DeregisterUpdateEvery100MillisecondsUnscaled(CheckDeferredSave);
			Application.quitting -= FlushDeferredSave;
			PlayerPrefs.Save();
#else
			throw new System.NotImplementedException();
#endif
		}

		#endregion
	}

}
