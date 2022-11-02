using System;
using Extenity.ConsistencyToolbox;

namespace Extenity.MathToolbox
{

	[Serializable]
	public struct ClampedFloat : IClamped
	{
		#region Initialization

		public ClampedFloat(float rawValue)
		{
			this.min = default(float);
			this.max = default(float);
			this.value = rawValue;
		}

		public ClampedFloat(float rawValue, float min, float max)
		{
			this.min = min;
			this.max = max;
			this.value = rawValue;
		}

		#endregion

		#region Data

#if UNITY
		[UnityEngine.SerializeField]
#endif
		private float value;
#if UNITY
		[UnityEngine.SerializeField]
#endif
		private float min;
#if UNITY
		[UnityEngine.SerializeField]
#endif
		private float max;

		public float RawValue
		{
			get { return value; }
			set { this.value = value; }
		}
		public float Value
		{
			get { return Clamp(); }
			set { this.value = value; }
		}
		public float Min
		{
			get { return min; }
			set { min = value; }
		}
		public float Max
		{
			get { return max; }
			set { max = value; }
		}

		public float NormalizedValue
		{
            get
            {
                var normalized = (float)(value - min) / (max - min);
                if (normalized < 0) return 0f;
                if (normalized > 1) return 1f;
                return normalized;
            }
			set { this.value = min + value * (max - min); }
		}

		public bool IsMinMaxValid
		{
			get { return Min < Max; }
		}

		private float Clamp()
		{
			if (value.CompareTo(min) < 0)
				return min;
			if (value.CompareTo(max) > 0)
				return max;
			return value;
		}

		#endregion

		#region Math Operations

		public static ClampedFloat operator +(ClampedFloat a, ClampedFloat b)
		{
			return new ClampedFloat(a.Value + b.Value, a.Min, a.Max);
		}

		public static ClampedFloat operator -(ClampedFloat a, ClampedFloat b)
		{
			return new ClampedFloat(a.Value - b.Value, a.Min, a.Max);
		}

		public static ClampedFloat operator *(ClampedFloat a, ClampedFloat b)
		{
			return new ClampedFloat(a.Value * b.Value, a.Min, a.Max);
		}

		public static ClampedFloat operator /(ClampedFloat a, ClampedFloat b)
		{
			return new ClampedFloat(a.Value / b.Value, a.Min, a.Max);
		}

		#endregion

		#region Equality

		public static bool operator ==(ClampedFloat a, ClampedFloat b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(ClampedFloat a, ClampedFloat b)
		{
			return !a.Equals(b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is ClampedFloat))
				return false;

			var castObj = (ClampedFloat)obj;
			return value.Equals(castObj.value);
		}

		public bool Equals(ClampedFloat obj)
		{
			return value.Equals(obj.value);
		}

		#endregion

		#region Comparison

		public int CompareTo(object obj)
		{
			if (obj == null || !(obj is ClampedFloat))
				return 1;

			var castObj = (ClampedFloat)obj;
			return value.CompareTo(castObj.value);
		}

		public int CompareTo(ClampedFloat obj)
		{
			return value.CompareTo(obj.value);
		}

		#endregion

		#region GetHashCode

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		#endregion

		#region ToString

		public override string ToString()
		{
			return Value.ToString();
		}

		public string ToStringLimits()
		{
			return "{Min: '" + Min + "' Max: '" + Max + "'}";
		}

		public string ToStringWithLimits()
		{
			return "{Value: '" + Value + "' Min: '" + Min + "' Max: '" + Max + "'}";
		}

		public string ToStringDetailed()
		{
			return "{Value: '" + Value + "' RawValue: '" + RawValue + "' Min: '" + Min + "' Max: '" + Max + "'}";
		}

		#endregion

		#region Consistency

		public void CheckConsistency(ConsistencyChecker checker)
		{
			if (IsMinMaxValid)
			{
				checker.AddError("Clamped value maximum limit is not greater than minimum limit.");
			}
		}

		#endregion
	}

}
