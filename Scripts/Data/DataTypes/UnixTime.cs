using System;

namespace Extenity.DataToolbox
{

	public struct UnixTime
	{
		private double time;

		private static DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);

		public UnixTime(double unixTimestamp)
		{
			time = unixTimestamp;
		}

		public UnixTime(DateTime dateTime)
		{
			time = ConvertToDouble(dateTime);
		}

		public static UnixTime Parse(string unixTimestampStr)
		{
			return new UnixTime(double.Parse(unixTimestampStr));
		}

		override public string ToString()
		{
			return Time.ToString();
		}

		public string ToStringInt()
		{
			return TimeInt.ToString();
		}

		public static DateTime ToDateTime(double value)
		{
			return ConvertFrom(value);
		}

		#region Converters

		public static DateTime ConvertFrom(string unixTimestamp)
		{
			return ConvertFrom(Convert.ToDouble(unixTimestamp));
		}

		public static DateTime ConvertFrom(double unixTimestamp)
		{
			DateTime dateTime = origin;
			return dateTime.AddSeconds(unixTimestamp);
		}

		public static UnixTime ConvertTo(DateTime dateTime)
		{
			TimeSpan diff = dateTime - origin;
			return new UnixTime(diff.TotalSeconds);
		}

		public static double ConvertToDouble(DateTime dateTime)
		{
			TimeSpan diff = dateTime - origin;
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

		public static DateTime Origin
		{
			get { return origin; }
		}

		public DateTime DateTime
		{
			get { return ConvertFrom(time); }
			set { time = ConvertToDouble(value); }
		}

		public double Time
		{
			get { return time; }
			set { time = value; }
		}

		public long TimeInt
		{
			get { return (long)time; }
			set { time = value; }
		}

		public bool IsOrigin
		{
			get { return time == 0f; }
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
