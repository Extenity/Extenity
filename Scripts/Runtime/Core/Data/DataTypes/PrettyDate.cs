using System;

namespace Extenity.DataToolbox
{

	public static class PrettyDate
	{
		public class PrettyDateStrings
		{
			public string Now = "Now";
			public string MinuteAgo = "{0} minute ago";
			public string MinutesAgo = "{0} minutes ago";
			public string HourAgo = "{0} hour ago";
			public string HoursAgo = "{0} hours ago";
			public string Yesterday = "Yesterday";
			public string DaysAgo = "{0} days ago";
			public string WeeksAgo = "{0} weeks ago";
		}

		public static readonly PrettyDateStrings StandardPrettyDateStrings = new PrettyDateStrings();

		public static string GetElapsedPrettyDate(this TimeSpan timeSpan, PrettyDateStrings prettyDateStrings = null)
		{
			if (prettyDateStrings == null)
				prettyDateStrings = StandardPrettyDateStrings;

			// 1.
			// Get time span elapsed since the date.
			//Example: timeSpan = DateTime.Now.Subtract(time))

			// 2.
			// Get total number of days elapsed.
			int dayDiff = (int)timeSpan.TotalDays;

			// 3.
			// Get total number of seconds elapsed.
			int secDiff = (int)timeSpan.TotalSeconds;

			// 4.
			// Don't allow out of range values.
			if (dayDiff < 0 || dayDiff >= 31)
			{
				return null;
			}

			// 5.
			// Handle same-day times.
			if (dayDiff == 0)
			{
				// A.
				// Less than one minute ago.
				if (secDiff < 60)
				{
					return prettyDateStrings.Now;
				}
				// B.
				// Less than 2 minutes ago.
				if (secDiff < 120)
				{
					return string.Format(prettyDateStrings.MinuteAgo, 1);
				}
				// C.
				// Less than one hour ago.
				if (secDiff < 3600)
				{
					return string.Format(prettyDateStrings.MinutesAgo,
						Math.Floor((double)secDiff / 60));
				}
				// D.
				// Less than 2 hours ago.
				if (secDiff < 7200)
				{
					return string.Format(prettyDateStrings.HourAgo, 1);
				}
				// E.
				// Less than one day ago.
				if (secDiff < 86400)
				{
					return string.Format(prettyDateStrings.HoursAgo,
						Math.Floor((double)secDiff / 3600));
				}
			}
			// 6.
			// Handle previous days.
			if (dayDiff == 1)
			{
				return prettyDateStrings.Yesterday;
			}
			if (dayDiff < 7)
			{
				return string.Format(prettyDateStrings.DaysAgo,
				dayDiff);
			}
			if (dayDiff < 31)
			{
				return string.Format(prettyDateStrings.WeeksAgo,
				Math.Ceiling((double)dayDiff / 7));
			}
			return null;
		}
	}

}
