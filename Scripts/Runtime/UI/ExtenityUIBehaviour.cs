using Extenity.GameObjectToolbox;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extenity.UIToolbox
{

	public class ExtenityUIBehaviour : UIBehaviour
	{
		#region Initialization

		protected override void OnEnable()
		{
			base.OnEnable();

			EmitCanvasGroupInteractivity();
		}

		#endregion

		#region CanvasGroup

		private CanvasGroup _CanvasGroup;
		public CanvasGroup CanvasGroup
		{
			get
			{
				if (!_CanvasGroup)
				{
					_CanvasGroup = gameObject.GetComponentInParent<CanvasGroup>(true, true);
				}
				return _CanvasGroup;
			}
		}

		public void ClearCanvasGroupCache()
		{
			_CanvasGroup = null;
		}

		#endregion

		#region Canvas Group Modification

		protected override void OnCanvasGroupChanged()
		{
			base.OnCanvasGroupChanged();

			// In a better world, we do not need to find out which CanvasGroup Unity is talking about. Rather, Unity
			// tells which CanvasGroup it is with a method parameter of this callback. But whatever.

			if (!CanvasGroup)
			{
				Log.FatalWithContext(this, "Failed to find which CanvasGroup the callback is called for.");
				return;
			}

			// Log.CurrentMethod($"--- Interactable: {CanvasGroup.interactable} \t Alpha: {CanvasGroup.alpha}");

			EmitCanvasGroupInteractivity();
		}

		#endregion

		#region Canvas Group Interactivity

		// TODO: Some testing would be nice.

		private bool IsCanvasGroupInteractivityEmittedBefore = false;
		private bool WasInteractable;

		private void EmitCanvasGroupInteractivity()
		{
			// Note that missing CanvasGroup is interpreted as being non-interactable.
			var isInteractable = CanvasGroup && CanvasGroup.interactable;

			var needsEmitting =
				!IsCanvasGroupInteractivityEmittedBefore || // See if trying to emit for the first time, which should always be emitted.
			    WasInteractable != isInteractable; // See if Interactable state is changed after previous emit.
			WasInteractable = isInteractable;
			IsCanvasGroupInteractivityEmittedBefore = true;

			if (needsEmitting)
			{
				if (isInteractable)
					OnBecameInteractable();
				else
					OnBecameNonInteractable();
			}
		}

		protected virtual void OnBecameInteractable()
		{
		}

		protected virtual void OnBecameNonInteractable()
		{
		}

		#endregion

		#region Parent Change

		protected override void OnBeforeTransformParentChanged()
		{
			// CanvasGroup might have changed. So the CanvasGroup cache should be invalidated here.
			ClearCanvasGroupCache();

			base.OnBeforeTransformParentChanged();
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(ExtenityUIBehaviour));

		#endregion
	}

}
