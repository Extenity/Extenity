using Extenity.DataToolbox;
using Extenity.ScreenToolbox;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.UIToolbox
{

	/// <summary>
	/// Source: https://forum.unity.com/threads/canvashelper-resizes-a-recttransform-to-iphone-xs-safe-area.521107/
	/// </summary>
	// [ExecuteInEditMode] // See 11653662.
	public class FitToSafeArea : MonoBehaviour
	{
		[Required]
		public Canvas ContainerCanvas;
		[Required]
		public RectTransform PanelThatFitsIntoSafeAreaOfCanvas;

		protected void OnEnable()
		{
			ScreenTracker.OnScreenModified.AddListener(OnScreenModified);
			OnScreenModified(default);
		}

		protected void OnDisable()
		{
			ScreenTracker.OnScreenModified.RemoveListener(OnScreenModified);
		}

		private void OnScreenModified(ScreenInfo info)
		{
			ScreenTools.ApplyCustomizableSafeArea(ContainerCanvas, PanelThatFitsIntoSafeAreaOfCanvas);
		}

		#region Display SafeArea In Inspector

#if UNITY_EDITOR

		[PropertySpace(SpaceBefore = 20f)]
		[ShowInInspector, InlineProperty]
		private static Rect CurrentSafeArea => ScreenTools.SafeArea;

#endif

		#endregion

		#region Fix RectTransform

#if UNITY_EDITOR
		private string RectTransformFixTooltip;

		private bool IsRectTransformNeedsFixing => !Application.isPlaying && RunRectTransformFixer(true);

		[Button(ButtonSizes.Large)]
		[PropertySpace(SpaceBefore = 20)]
		[EnableIf(nameof(IsRectTransformNeedsFixing))]
		[InfoBox("$" + nameof(RectTransformFixTooltip), nameof(IsRectTransformNeedsFixing), InfoMessageType = InfoMessageType.Error)]
		private void FixRectTransform()
		{
			RunRectTransformFixer(false);
		}

		private bool RunRectTransformFixer(bool dryRun)
		{
			var rectTransform = PanelThatFitsIntoSafeAreaOfCanvas;
			if (!rectTransform)
			{
				// Just skip checking if there is no RectTransform.
				// It's already marked with [Required] so the user will know what to do.
				RectTransformFixTooltip = "";
				return false;
			}

			// Doesn't work for some reason. Probably needs a simple fix but skipped for now.
			// if (!dryRun)
			// {
			// 	UnityEditor.Undo.RecordObject(gameObject, $"Apply {nameof(FitToSafeArea)} {nameof(RectTransform)} fix");
			// }

			var stringBuilder = StringTools.SharedStringBuilder.Value;
			lock (stringBuilder)
			{
				stringBuilder.Clear(); // Make sure it is clean before starting to use.

				if (rectTransform.anchoredPosition != Vector2.zero)
				{
					if (dryRun)
					{
						stringBuilder.AppendLine("Position should be zero.");
					}
					else
					{
						rectTransform.anchoredPosition = Vector2.zero;
					}
				}
				if (rectTransform.sizeDelta != Vector2.zero)
				{
					if (dryRun)
					{
						stringBuilder.AppendLine("Size should be zero.");
					}
					else
					{
						rectTransform.sizeDelta = Vector2.zero;
					}
				}
				if (rectTransform.anchorMin != Vector2.zero)
				{
					if (dryRun)
					{
						stringBuilder.AppendLine("AnchorMin should be zero.");
					}
					else
					{
						rectTransform.anchorMin = Vector2.zero;
					}
				}
				if (rectTransform.anchorMax != Vector2.one)
				{
					if (dryRun)
					{
						stringBuilder.AppendLine("AnchorMax should be one.");
					}
					else
					{
						rectTransform.anchorMax = Vector2.one;
					}
				}
				if (rectTransform.pivot != new Vector2(0.5f, 0.5f))
				{
					if (dryRun)
					{
						stringBuilder.AppendLine("Pivot should be at the center.");
					}
					else
					{
						rectTransform.pivot = new Vector2(0.5f, 0.5f);
					}
				}
				if (rectTransform.localEulerAngles != Vector3.zero)
				{
					if (dryRun)
					{
						stringBuilder.AppendLine("Rotation should be zero.");
					}
					else
					{
						rectTransform.localEulerAngles = Vector3.zero;
					}
				}
				if (rectTransform.localScale != Vector3.one)
				{
					if (dryRun)
					{
						stringBuilder.AppendLine("Scale should be one.");
					}
					else
					{
						rectTransform.localScale = Vector3.one;
					}
				}

				RectTransformFixTooltip = stringBuilder.ToString();
				StringTools.ClearSharedStringBuilder(stringBuilder); // Make sure we will leave it clean after use.
				return RectTransformFixTooltip.Length > 0;
			}
		}

#endif

		#endregion

		#region Auto Find Components

#if UNITY_EDITOR

		protected void OnValidate()
		{
			if (ContainerCanvas == null)
			{
				ContainerCanvas = GetComponentInParent<Canvas>();
			}
			if (PanelThatFitsIntoSafeAreaOfCanvas == null)
			{
				PanelThatFitsIntoSafeAreaOfCanvas = GetComponent<RectTransform>();
			}
		}

#endif

		#endregion
	}

}
