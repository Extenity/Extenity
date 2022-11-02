using System;
using Extenity.ConsistencyToolbox;

namespace Extenity.MathToolbox
{

	[Serializable]
	public struct ClampedInt : IClamped
	{
		#region Initialization

		public ClampedInt(int rawValue)
		{
			this.min = default(int);
			this.max = default(int);
			this.value = rawValue;
		}

		public ClampedInt(int rawValue, int min, int max)
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
		private int value;
#if UNITY
		[UnityEngine.SerializeField]
#endif
		private int min;
#if UNITY
		[UnityEngine.SerializeField]
#endif
		private int max;

		public int RawValue
		{
			get { return value; }
			set { this.value = value; }
		}
		public int Value
		{
			get { return Clamp(); }
			set { this.value = value; }
		}
		public int Min
		{
			get { return min; }
			set { min = value; }
		}
		public int Max
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
			set { this.value = (int)(min + value * (max - min)); }
		}

		public bool IsMinMaxValid
		{
			get { return Min < Max; }
		}

		private int Clamp()
		{
			if (value.CompareTo(min) < 0)
				return min;
			if (value.CompareTo(max) > 0)
				return max;
			return value;
		}

		#endregion

		#region Math Operations

		public static ClampedInt operator +(ClampedInt a, ClampedInt b)
		{
			return new ClampedInt(a.Value + b.Value, a.Min, a.Max);
		}

		public static ClampedInt operator -(ClampedInt a, ClampedInt b)
		{
			return new ClampedInt(a.Value - b.Value, a.Min, a.Max);
		}

		public static ClampedInt operator *(ClampedInt a, ClampedInt b)
		{
			return new ClampedInt(a.Value * b.Value, a.Min, a.Max);
		}

		public static ClampedInt operator /(ClampedInt a, ClampedInt b)
		{
			return new ClampedInt(a.Value / b.Value, a.Min, a.Max);
		}

		#endregion

		#region Equality

		public static bool operator ==(ClampedInt a, ClampedInt b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(ClampedInt a, ClampedInt b)
		{
			return !a.Equals(b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is ClampedInt))
				return false;

			var castObj = (ClampedInt)obj;
			return value.Equals(castObj.value);
		}

		public bool Equals(ClampedInt obj)
		{
			return value.Equals(obj.value);
		}

		#endregion

		#region Comparison

		public int CompareTo(object obj)
		{
			if (obj == null || !(obj is ClampedInt))
				return 1;

			var castObj = (ClampedInt)obj;
			return value.CompareTo(castObj.value);
		}

		public int CompareTo(ClampedInt obj)
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
