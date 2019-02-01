using System.Collections.Generic;

namespace Extenity.DebugToolbox.GraphPlotting
{

	public class TagEntry
	{
		public float Time;
		public string Text;

		public TagEntry()
		{
			Time = float.NaN;
			Text = "";
		}

		public TagEntry(float time, string text)
		{
			Time = time;
			Text = text;
		}
	}

	public class TagEntryTimeComparer : IComparer<TagEntry>
	{
		public int Compare(TagEntry a, TagEntry b)
		{
			return a.Time.CompareTo(b.Time);
		}
	}

}