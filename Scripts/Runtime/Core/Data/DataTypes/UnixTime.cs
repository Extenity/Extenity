using System;
using System.Globalization;

namespace Extenity.DataToolbox
{

	public struct UnixTime
	{
		public double Time;

		public static readonly DateTime Origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);

		public UnixTime(double unixTimestamp)
		{
			Time = unixTimestamp;
		}

		public UnixTime(DateTime dateTime)
		{
			Time = ConvertToDouble(dateTime);
		}

		/// <summary>
		/// Reads a timestamp written with the invariant culture, as <see cref="ToString"/> writes it, whatever the
		/// culture of the machine. Thousands separators are not allowed, so that a timestamp written with a decimal
		/// comma throws instead of being read as a much larger number.
		/// </summary>
		public static UnixTime Parse(string unixTimestampStr)
		{
			return new UnixTime(double.Parse(unixTimestampStr, NumberStyles.Float, CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Writes the timestamp with the invariant culture, so that <see cref="Parse"/> reads it back on any machine.
		/// </summary>
		public override string ToString()
		{
			return Time.ToString(CultureInfo.InvariantCulture);
		}

		public string ToStringInt()
		{
			return TimeInt.ToString(CultureInfo.InvariantCulture);
		}

		public static DateTime ToDateTime(double value)
		{
			return ConvertFrom(value);
		}

		#region Converters

		public static DateTime ConvertFrom(string unixTimestamp)
		{
			return ConvertFrom(Parse(unixTimestamp).Time);
		}

		public static DateTime ConvertFrom(double unixTimestamp)
		{
			DateTime dateTime = Origin;
			return dateTime.AddSeconds(unixTimestamp);
		}

		public static UnixTime ConvertTo(DateTime dateTime)
		{
			TimeSpan diff = dateTime - Origin;
			return new UnixTime(diff.TotalSeconds);
		}

		public static double ConvertToDouble(DateTime dateTime)
		{
			TimeSpan diff = dateTime - Origin;
			return diff.TotalSeconds;
		}

		#endregion

		public static UnixTime Now
		{
			get { return ConvertTo(DateTime.Now); }
		}
		public static double NowDouble
		{
			get { return ConvertToDouble(DateTime.Now); }
		}
		public static long NowInt
		{
			get { return (long)ConvertToDouble(DateTime.Now); }
		}

		public static UnixTime Today
		{
			get { return ConvertTo(DateTime.Today); }
		}
		public static double TodayDouble
		{
			get { return ConvertToDouble(DateTime.Today); }
		}
		public static long TodayInt
		{
			get { return (long)ConvertToDouble(DateTime.Today); }
		}

		public static UnixTime UtcNow
		{
			get { return ConvertTo(DateTime.UtcNow); }
		}
		public static double UtcNowDouble
		{
			get { return ConvertToDouble(DateTime.UtcNow); }
		}
		public static long UtcNowInt
		{
			get { return (long)ConvertToDouble(DateTime.UtcNow); }
		}

		public DateTime DateTime
		{
			get { return ConvertFrom(Time); }
			set { Time = ConvertToDouble(value); }
		}

		public long TimeInt
		{
			get { return (long)Time; }
			set { Time = value; }
		}

		public bool IsOrigin
		{
			get { return Time == 0f; }
		}

		public static long SecondsInADay
		{
			get { return 60 * 60 * 24; }
		}

		public static long SecondsInAnHour
		{
			get { return 60 * 60; }
		}
	}

	public static class UnixTimeTools
	{
		public static UnixTime ToUnixTime(this DateTime dateTime)
		{
			return UnixTime.ConvertTo(dateTime);
		}

		public static long ToUnixTimeInt(this DateTime dateTime)
		{
			return (long)UnixTime.ConvertToDouble(dateTime);
		}
	}

}
