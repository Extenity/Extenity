using System;
using UnityEngine;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public enum ClickAreaShape
	{
		Square,
		Ellipse,
	}

	/// <summary>
	/// Transparent, clickable-only UI element.
	/// </summary>
	[RequireComponent(typeof(CanvasRenderer))]
	public class ClickArea : Graphic, ICanvasRaycastFilter
	{
		public ClickAreaShape Shape = ClickAreaShape.Square;

		public override void Rebuild(CanvasUpdate update)
		{
			// We do nothing. So UpdateMaterial and UpdateGeometry won't be called.
		}

		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			vertexHelper.Clear();
		}

		public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
		{
			var insideRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out _);

			switch (Shape)
			{
				case ClickAreaShape.Square:
					return insideRect;

				case ClickAreaShape.Ellipse:
				{
					if (insideRect)
					{
						if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out var localPoint))
						{
							var rect = rectTransform.rect;
							var diff = rect.center - localPoint;
							diff.x /= rect.width;
							diff.y /= rect.height;
							return diff.sqrMagnitude <= 0.5f * 0.5f; // 0.5 is the radius of unit circle.
						}
					}
					return false;
				}

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#region Override other Graphic methods for clarity

		protected override void UpdateMaterial()
		{
		}

		protected override void UpdateGeometry()
		{
		}

		public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
		{
		}

		public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha, bool useRGB)
		{
		}

		public override void CrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
		{
		}

		public override Material material
		{
			get => null;
			set { }
		}
		public override Material materialForRendering => null;
		public override Texture mainTexture => null;

		#endregion
	}

}
