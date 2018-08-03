using System;
using System.Collections.Generic;

namespace Extenity.DataToolbox
{

	public static class DateTimeTools
	{
		public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
		{
			int diff = startOfWeek - dt.DayOfWeek;
			return dt.AddDays(diff).Date;
		}

		public static DateTime StartOfYear(this DateTime dt)
		{
			return dt.StartOfMonth().AddMonths(-(dt.Month - 1));
		}

		public static DateTime EndOfYear(this DateTime dt)
		{
			return dt.StartOfMonth().AddMonths(12 - dt.Month);
		}

		public static DateTime StartOfMonth(this DateTime dt)
		{
			return dt.AddDays(-(dt.Day - 1));
		}

		public static DateTime EndOfMonth(this DateTime dt)
		{
			return dt.AddMonths(1).AddDays(-dt.Day);
		}

		public static DateTime StartOfDay(this DateTime dt)
		{
			return dt.AddHours(-dt.Hour).AddMinutes(-dt.Minute).AddSeconds(-dt.Second);
		}

		public static DateTime EndOfDay(this DateTime dt)
		{
			return dt.AddHours(23 - dt.Hour).AddMinutes(59 - dt.Minute).AddSeconds(59 - dt.Second);
		}

		public static DateTime ClearTime(this DateTime dt)
		{
			return dt.StartOfDay();
		}

		#region Day Of Week

		public static int ToIndexStartingAtSunday(this DayOfWeek dayOfWeek)
		{
			return (int)dayOfWeek;
		}

		public static int ToIndexStartingAtMonday(this DayOfWeek dayOfWeek)
		{
			var index = (int)dayOfWeek - 1;
			if (index == -1)
				index = 6;
			return index;
		}

		public static DateTime NextWeekday(this DateTime start, DayOfWeek day)
		{
			int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
			return start.AddDays(daysToAdd);
		}

		public static DateTime PreviousWeekday(this DateTime start, DayOfWeek day)
		{
			int daysToRemove = ((int)day - (int)start.DayOfWeek - 7) % 7;
			return start.AddDays(daysToRemove);
		}

		#endregion

		#region Each Day

		public static IEnumerable<DateTime> EachDay(this DateTime start, DateTime end)
		{
			var currentDay = start.Date;

			while (currentDay <= end)
			{
				yield return currentDay;
				currentDay = currentDay.AddDays(1);
			}
		}

		#endregion

		#region Day Names

		public static string GetEnglishDayNameStartingFromMonday(int dayOfWeek)
		{
			switch (dayOfWeek)
			{
				case 0: return "Monday";
				case 1: return "Tuesday";
				case 2: return "Wednesday";
				case 3: return "Thursday";
				case 4: return "Friday";
				case 5: return "Saturday";
				case 6: return "Sunday";
				default: return "";
			}
		}

		public static string GetEnglishDayNameAbbreviationStartingFromMonday(int dayOfWeek)
		{
			switch (dayOfWeek)
			{
				case 0: return "Mon";
				case 1: return "Tue";
				case 2: return "Wed";
				case 3: return "Thu";
				case 4: return "Fri";
				case 5: return "Sat";
				case 6: return "Sun";
				default: return "";
			}
		}

		#endregion

		#region Milliseconds

		public static double TotalMilliseconds(this DateTime dateTime)
		{
			return (dateTime - DateTime.MinValue).TotalMilliseconds;
		}

		#endregion

		#region Formatted String

		public static string ToFullDateTimeForFilename(this DateTime datetime)
		{
			return $"{datetime:yyyy.MM.dd HH.mm.ss}";
		}

		public static string ToFullDateTime(this DateTime datetime)
		{
			return $"{datetime:yyyy.MM.dd HH:mm:ss}";
		}

		public static string ToFullDateTimeMsecForFilename(this DateTime datetime)
		{
			return $"{datetime:yyyy.MM.dd HH.mm.ss.ffff}";
		}

		public static string ToFullDateTimeMsec(this DateTime datetime)
		{
			return $"{datetime:yyyy.MM.dd HH:mm:ss.ffff}";
		}

		public static string ToStringMinutesSecondsMilliseconds(this TimeSpan timeSpan)
		{
			return ((int)timeSpan.TotalMinutes).ToString("00") + ":" + timeSpan.Seconds.ToString("00") + "." + timeSpan.Milliseconds.ToString("000");
		}

		public static string ToStringHoursMinutesSecondsMilliseconds(this TimeSpan timeSpan)
		{
			return ((int)timeSpan.TotalHours).ToString() + ":" + timeSpan.Minutes.ToString("00") + ":" + timeSpan.Seconds.ToString("00") + "." + timeSpan.Milliseconds.ToString("000");
		}

		public static string ToStringMinutesSeconds(this TimeSpan timeSpan)
		{
			return ((int)timeSpan.TotalMinutes).ToString("00") + ":" + (object)timeSpan.Seconds.ToString("00");
		}

		#endregion
	}

}
