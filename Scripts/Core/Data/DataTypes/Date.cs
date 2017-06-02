using System;

namespace Extenity.DataToolbox
{

	[Serializable]
	public struct Date : IEquatable<Date>, IEquatable<DateTime>
	{
		#region Data

		public short Year;
		public byte Month;
		public byte Day;

		#endregion

		#region Initialization

		public Date(DateTime date)
		{
			Year = (short)date.Year;
			Month = (byte)date.Month;
			Day = (byte)date.Day;
		}

		#endregion

		#region Equality

		public bool Equals(Date other)
		{
			return Year.Equals(other.Year) &&
				   Month.Equals(other.Month) &&
				   Day.Equals(other.Day);
		}

		public bool Equals(DateTime other)
		{
			return Year.Equals((short)other.Year) &&
				   Month.Equals((byte)other.Month) &&
				   Day.Equals((byte)other.Day);
		}

		#endregion

		#region Conversions

		public override string ToString()
		{
			return ((DateTime)this).ToString();
		}

		public static implicit operator DateTime(Date date)
		{
			return new DateTime((int)date.Year, (int)date.Month, (int)date.Day, 0, 0, 0, DateTimeKind.Unspecified);
		}

		public static explicit operator Date(DateTime dateTime)
		{
			return new Date(dateTime);
		}

		#endregion

		#region Tools

		public int DaysInMonth
		{
			get
			{
				return DateTime.DaysInMonth(Year, Month);
			}
		}

		/// <summary>
		/// Sets day by clamping for current month's length. Also clamps days lesser than 1 to 1.
		/// </summary>
		/// <param name="day"></param>
		/// <returns>Returns true if day was clamped. Returns false otherwise.</returns>
		public bool SetDay(int day)
		{
			var daysInMonth = DaysInMonth;
			if (day > daysInMonth)
			{
				Day = (byte)daysInMonth;
				return true;
			}
			if (day <= 0)
			{
				Day = 1;
				return true;
			}
			Day = (byte)day;
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>Returns true if day has changed since each month differs in day length. Returns false otherwise.</returns>
		public bool SetMonth(int month)
		{
			Month = (byte)month;
			var daysInMonth = DaysInMonth;
			if (Day > daysInMonth)
			{
				Day = (byte)daysInMonth;
				return true;
			}
			return false;
		}

		#endregion
	}

}
