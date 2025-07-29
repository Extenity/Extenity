#if UNITY_5_3_OR_NEWER

using UnityEngine;

namespace Extenity.ColoringToolbox
{

	public abstract class TwoWayColorScale : IColorScale
	{
		#region Configuration

		private float _MinimumValue;
		private float _MaximumValue;
		private float _OneOverMinimumValue;
		private float _OneOverMaximumValue;

		public float MinimumValue
		{
			get { return _MinimumValue; }
			set
			{
				_MinimumValue = value;
				_OneOverMinimumValue = _MinimumValue < 0f ? 1f / _MinimumValue : 0f;
			}
		}

		public float MaximumValue
		{
			get { return _MaximumValue; }
			set
			{
				_MaximumValue = value;
				_OneOverMaximumValue = _MaximumValue > 0f ? 1f / _MaximumValue : 0f;
			}
		}

		protected float OneOverMinimumValue
		{
			get { return _OneOverMinimumValue; }
		}

		protected float OneOverMaximumValue
		{
			get { return _OneOverMaximumValue; }
		}

		public void SetMinimumAndMaximum(float minimumValue, float maximumValue)
		{
			MinimumValue = minimumValue;
			MaximumValue = maximumValue;
		}

		#endregion

		#region Initialization

		public TwoWayColorScale()
		{
			MinimumValue = -1f;
			MaximumValue = 1f;
		}

		public TwoWayColorScale(float minimumValue, float maximumValue)
		{
			MinimumValue = minimumValue;
			MaximumValue = maximumValue;
		}

		#endregion

		#region Get Color

		public abstract Color32 GetPositiveNormalizedColor32(float normalizedScalePoint);
		public abstract Color32 GetNegativeNormalizedColor32(float normalizedScalePoint);

		public Color32 GetColor32(float scalePoint)
		{
			if (scalePoint > 0f)
			{
				scalePoint = scalePoint * _OneOverMaximumValue;
				return GetPositiveNormalizedColor32(scalePoint);
			}
			else
			{
				scalePoint = -(scalePoint - _MinimumValue) * _OneOverMinimumValue;
				scalePoint = 1f - scalePoint;
				return GetNegativeNormalizedColor32(scalePoint);
			}
		}

		#endregion
	}

}

#endif
