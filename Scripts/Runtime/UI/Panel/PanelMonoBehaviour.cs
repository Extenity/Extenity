#if ExtenityScreenManagement

using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.UIToolbox
{

	public abstract class PanelMonoBehaviour : MonoBehaviour
	{
		#region Initialization and Deinitialization

		protected virtual void Awake()
		{
			LogIfPanelLinkingRequired();

			if (LinkedPanel)
			{
				LinkedPanel.Register(this);
			}
		}

		protected virtual void OnDestroy()
		{
			if (LinkedPanel)
			{
				LinkedPanel.Deregister(this);
			}
		}

		#endregion

		#region Links

		[FoldoutGroup("Links")]
#if UNITY_EDITOR
		[InfoBox("Visibility callbacks are designed to be called for the status changes of this linked " + nameof(Panel) + ".",
		         InfoMessageType.Error, VisibleIf = nameof(NeedToBeLinkedToPanel))]
#endif
		public Panel LinkedPanel;

		#endregion

		#region Visibility Callbacks

		protected internal virtual void OnBeforeBecomingVisible() { }
		protected internal virtual void OnAfterBecameVisible() { }
		protected internal virtual void OnBeforeBecomingInvisible() { }
		protected internal virtual void OnAfterBecameInvisible() { }

		private bool IsImplementedAnyVisibilityCallbacks
		{
			get
			{
				var type = GetType();
				var baseType = typeof(PanelMonoBehaviour);

				return
					type.GetMethod(nameof(OnBeforeBecomingVisible), BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType != baseType ||
					type.GetMethod(nameof(OnAfterBecameVisible), BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType != baseType ||
					type.GetMethod(nameof(OnBeforeBecomingInvisible), BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType != baseType ||
					type.GetMethod(nameof(OnAfterBecameInvisible), BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType != baseType;
			}
		}

		#endregion

		#region Editor

#if UNITY_EDITOR

		/// <summary>
		/// Checks if there is no linked object AND if the visibility callbacks are implemented in the derived class. That
		/// means they are expected to be called but won't be called because the listening object is not stated.
		/// </summary>
		private bool NeedToBeLinkedToPanel => !LinkedPanel && IsImplementedAnyVisibilityCallbacks;

		private void LogIfPanelLinkingRequired()
		{
			if (NeedToBeLinkedToPanel)
			{
				Log.FatalWithContext(this, $"Visibility callbacks won't be called for '{this}' until a '{nameof(LinkedPanel)}' is specified in the inspector.");
			}
		}

#else

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private void LogIfPanelLinkingRequired()
		{
		}

#endif

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(PanelMonoBehaviour));

		#endregion
	}

}

#endif
