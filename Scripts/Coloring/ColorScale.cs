using UnityEngine;

namespace Extenity.Coloring
{

	public abstract class ColorScale : IColorScale
	{
		#region Configuration

		private float _MinimumValue;
		private float _MaximumValue;
		private float _OneOverDifference;

		public float MinimumValue
		{
			get { return _MinimumValue; }
			set
			{
				_MinimumValue = value;
				_OneOverDifference = (_MaximumValue - _MinimumValue) > 0f ? 1f / (_MaximumValue - _MinimumValue) : 0f;
			}
		}

		public float MaximumValue
		{
			get { return _MaximumValue; }
			set
			{
				_MaximumValue = value;
				_OneOverDifference = (_MaximumValue - _MinimumValue) > 0f ? 1f / (_MaximumValue - _MinimumValue) : 0f;
			}
		}

		protected float OneOverDifference
		{
			get { return _OneOverDifference; }
		}

		public void SetMinimumAndMaximum(float minimumValue, float maximumValue)
		{
			_MinimumValue = minimumValue;
			_MaximumValue = maximumValue;
			_OneOverDifference = (_MaximumValue - _MinimumValue) > 0f ? 1f / (_MaximumValue - _MinimumValue) : 0f;
		}

		#endregion

		#region Initialization

		public ColorScale()
		{
			_MinimumValue = 0f;
			_MaximumValue = 1f;
			_OneOverDifference = (_MaximumValue - _MinimumValue) > 0f ? 1f / (_MaximumValue - _MinimumValue) : 0f;
		}

		public ColorScale(float minimumValue, float maximumValue)
		{
			_MinimumValue = minimumValue;
			_MaximumValue = maximumValue;
			_OneOverDifference = (_MaximumValue - _MinimumValue) > 0f ? 1f / (_MaximumValue - _MinimumValue) : 0f;
		}

		#endregion

		#region Get Color

		public abstract Color32 GetNormalizedColor32(float normalizedScalePoint);

		public Color32 GetColor32(float scalePoint)
		{
			var normalizedScalePoint = (scalePoint - _MinimumValue) * _OneOverDifference;
			return GetNormalizedColor32(normalizedScalePoint);
		}

		#endregion
	}

}
