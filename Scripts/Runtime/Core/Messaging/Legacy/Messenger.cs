#if ExtenityMessenger && UseLegacyMessenger

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Extenity.DataToolbox;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extenity.MessagingToolbox
{

	public static class Messenger
	{
		#region Deinitialization

		// No need to call it for now. But it might be needed in future.
		// internal static void Shutdown()
		// {
		// 	_DeregisterAllEvents();
		// }

		#endregion

		#region Events and Registration

		private static readonly Dictionary<string, ExtenityEvent> EventsByEventNames = new Dictionary<string, ExtenityEvent>();

		/// <param name="order">Lesser ordered callback gets called earlier. Callbacks that have the same order gets called in the order of AddListener calls. Negative values are allowed.</param>
		public static void RegisterEvent(string eventName, Action callback, int order = 0)
		{
			if (string.IsNullOrEmpty(eventName) ||
			    callback == null ||
			    callback.IsUnityObjectTargetedAndDestroyed()
			)
			{
				return; // Ignore
			}

			if (IsRegistrationLoggingEnabled)
				LogRegistration.VerboseWithContext(callback.Target as Object, $"Registering callback '<b>{callback.Method.Name}</b> in {callback.Target}' for event '<b>{eventName}</b>'");

			if (!EventsByEventNames.TryGetValue(eventName, out var events))
			{
				events = new ExtenityEvent();
				EventsByEventNames[eventName] = events;
			}
			events.AddListener(callback, order);
		}

		public static void DeregisterEvent(string eventName, Action callback)
		{
			if (string.IsNullOrEmpty(eventName) ||
			    callback == null)
				// (callback.Target as UnityEngine.Object) == null) No! We should remove the callback even though the object is no longer available.
			{
				return; // Ignore
			}

			if (IsRegistrationLoggingEnabled)
				LogRegistration.VerboseWithContext(callback.Target as Object, $"Deregistering callback '<b>{callback.Method.Name}</b> in {callback.Target}' from event '<b>{eventName}</b>'");

			if (EventsByEventNames.TryGetValue(eventName, out var events))
			{
				events.RemoveListener(callback);
			}
		}

		public static void DeregisterAllEvents(object callbackTarget)
		{
			if (callbackTarget == null)
				// (callback.Target as UnityEngine.Object) == null) No! We should remove the callback even though the object is no longer available.
			{
				return; // Ignore
			}

			if (IsRegistrationLoggingEnabled)
				LogRegistration.VerboseWithContext(callbackTarget as Object, $"Deregistering all callbacks of '{callbackTarget}'");

			foreach (var item in EventsByEventNames)
			{
				item.Value.RemoveAllListenersThatTargets(callbackTarget);
			}
		}

		private static void _DeregisterAllEvents()
		{
			// Commented out to prevent spam.
			// if (IsRegistrationLoggingEnabled)
			// 	LogRegistration("Deregistering all callbacks", null);

			foreach (var item in EventsByEventNames)
			{
				item.Value.RemoveAllListeners();
			}
		}

		#endregion

		#region Event Names

		private static string[] _EventNames;
		public static string[] EventNames
		{
			get
			{
				if (_EventNames == null || _EventNames.Length == 0)
				{
					Debug.LogException(new Exception("No event names were registered yet. It might probably be some system initialization order issue."));
				}
				return _EventNames;
			}
		}

		public static void AddConstStringFieldsAsEventNames(Type type)
		{
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

			var result = _EventNames == null
				? new List<string>(fields.Length)
				: _EventNames.ToList();

			for (var i = 0; i < fields.Length; i++)
			{
				var field = fields[i];
				if (field.IsLiteral && !field.IsInitOnly)
				{
					var value = field.GetValue(null) as string;
					if (!string.IsNullOrWhiteSpace(value))
					{
						if (!result.Contains(value))
						{
							result.Add(value);
						}
					}
				}
			}

			_EventNames = result.ToArray();
		}

		#endregion

		#region Emit

		public static void EmitEvent(string eventName)
		{
			if (IsEmitLoggingEnabled && !EmitLogFilter.Contains(eventName))
				LogEmit.Verbose($"Emitting '<b>{eventName}</b>'");

			if (EventsByEventNames.TryGetValue(eventName, out var events))
			{
				events.InvokeSafe();
			}
		}

		#endregion

		#region Log

		public static bool IsEmitLoggingEnabled = false;
		public static bool IsRegistrationLoggingEnabled = false;
		public static HashSet<string> EmitLogFilter = new HashSet<string>();
		
		private static readonly Logger LogRegistration = new(nameof(Messenger));
		private static readonly Logger LogEmit = new(nameof(Messenger));

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(Messenger));

		#endregion
	}

}

#endif
