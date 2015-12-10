using System;

public static class PrettyDate
{
	public class PrettyDateStrings
	{
		public string now = "Now";
		public string minuteAgo = "{0} minute ago";
		public string minutesAgo = "{0} minutes ago";
		public string hourAgo = "{0} hour ago";
		public string hoursAgo = "{0} hours ago";
		public string yesterday = "Yesterday";
		public string daysAgo = "{0} days ago";
		public string weeksAgo = "{0} weeks ago";
	}

	public static PrettyDateStrings standardPrettyDateStrings = new PrettyDateStrings();

	public static string GetElapsedPrettyDate(TimeSpan timeSpan, PrettyDateStrings prettyDateStrings = null)
	{
		if (prettyDateStrings == null)
			prettyDateStrings = standardPrettyDateStrings;

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
				return prettyDateStrings.now;
			}
			// B.
			// Less than 2 minutes ago.
			if (secDiff < 120)
			{
				return string.Format(prettyDateStrings.minuteAgo, 1);
			}
			// C.
			// Less than one hour ago.
			if (secDiff < 3600)
			{
				return string.Format(prettyDateStrings.minutesAgo,
					Math.Floor((double)secDiff / 60));
			}
			// D.
			// Less than 2 hours ago.
			if (secDiff < 7200)
			{
				return string.Format(prettyDateStrings.hourAgo, 1);
			}
			// E.
			// Less than one day ago.
			if (secDiff < 86400)
			{
				return string.Format(prettyDateStrings.hoursAgo,
					Math.Floor((double)secDiff / 3600));
			}
		}
		// 6.
		// Handle previous days.
		if (dayDiff == 1)
		{
			return prettyDateStrings.yesterday;
		}
		if (dayDiff < 7)
		{
			return string.Format(prettyDateStrings.daysAgo,
			dayDiff);
		}
		if (dayDiff < 31)
		{
			return string.Format(prettyDateStrings.weeksAgo,
			Math.Ceiling((double)dayDiff / 7));
		}
		return null;
	}
}
