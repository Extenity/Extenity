#if ExtenityScreenManagement

using System;
using System.Linq;
using System.Reflection;
using Extenity.MessagingToolbox;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.UIToolbox.ScreenManagement
{

	public abstract class ScreensBase : MonoBehaviour
	{
		#region Events

		protected abstract void RegisterEvents();

		protected void OnEnable()
		{
			CheckConsistency();
			RegisterEvents();
		}

		protected void OnDisable()
		{
#if ExtenityMessenger && UseLegacyMessenger
			Messenger.DeregisterAllEvents(this);
#else
			#error Not implemented yet!
#endif
		}

		#endregion

		#region Create/Show/Hide Screen

		protected void ShowOrCreateScreen(Screen screen)
		{
			if (screen == null)
				throw new ArgumentNullException();

			screen.ShowRequestCount++;

			// Create screen if not created before.
			if (!screen.Instance)
			{
				screen.CreateRequestCount++;
				LogCreation.VerboseWithContext(screen.Prefab, $"Creating '{screen.Name}' screen.");
				var go = Instantiate(screen.Prefab.gameObject);
				screen.Instance = go.GetComponent<Panel>();
				if (!screen.Instance)
				{
					Log.ErrorWithContext(screen.Prefab, "Screen prefab should have a Panel component in its parent so that the system can take the control of its visibility.");
				}
				go.SetActive(true);
			}

			// Show screen
			LogVisibility.VerboseWithContext(screen.Instance, $"Showing '{screen.Name}' screen.");
			screen.Instance.BecomeVisible();
		}

		protected void HideScreen(Screen screen)
		{
			if (screen == null)
				throw new ArgumentNullException();

			screen.HideRequestCount++;

			if (!screen.Instance) // Ignore the request. No instance means there is no visible thing to hide.
			{
				LogVisibility.VerboseWithContext(screen.Prefab, $"Hiding '{screen.Name}' screen but there was no instance.");
				return;
			}

			LogVisibility.VerboseWithContext(screen.Instance, $"Hiding '{screen.Name}' screen.");
			screen.Instance.BecomeInvisible();
		}

		#endregion

		#region Consistency

		private void CheckConsistency()
		{
			var type = GetType();
			var publicMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
			if (publicMethods.Length > 0)
			{
				var typeName = type.Name;
				Log.WarningWithContext(this, $"{typeName} should not have any public methods. The whole point of managing the whole UI from a single manager is reducing the 'code coupling'. Making {typeName} reachable from outside world breaks that rule. Instead, catch events in {typeName} and update screen visibilities as a response to events happening in the application. That way, all screen visibility commands can be gathered in one place and won't spread throughout the code base. Detected public methods are: " + string.Join(", ", publicMethods.Select(item => item.Name)));
			}
		}

		#endregion

		#region Log

		private static readonly Logger LogCreation = new("Screens");
		private static readonly Logger LogVisibility = new("Screens");
		private static readonly Logger Log = new("Screens");

		#endregion
	}

}

#endif
