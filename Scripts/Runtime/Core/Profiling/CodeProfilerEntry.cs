using System;
using System.Collections.Generic;
using Extenity.MathToolbox;

namespace Extenity.ProfilingToolbox
{

	[Serializable]
	public class CodeProfilerEntry
	{
		#region Metadata

		public readonly int ID;

		#endregion

		#region Initialization

		private CodeProfilerEntry(int id, CodeProfilerEntry parent)
		{
			ID = id;
			Parent = parent;
			if (Parent.Children == null)
				Parent.Children = new List<CodeProfilerEntry>(20); // Allocate some, but not too much
			Parent.Children.Add(this);
		}

		/// <summary>
		/// This constructor is dedicated to BaseEntry. Do not use it elsewhere.
		/// </summary>
		internal CodeProfilerEntry()
		{
			ID = 0;
			Parent = null;
			Children = new List<CodeProfilerEntry>(20); // Preallocate some, but not too much
		}

		#endregion

		#region Hierarch, Parent, Children

		[NonSerialized]
		public readonly CodeProfilerEntry Parent;

		public List<CodeProfilerEntry> Children;

		public bool IsChildExists(int id)
		{
			if (Children != null)
			{
				for (int i = 0; i < Children.Count; i++)
				{
					if (Children[i].ID == id)
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns true if the child is just created.
		/// </summary>
		internal bool GetOrAddChild(int id, out CodeProfilerEntry entry)
		{
			if (Children != null)
			{
				for (int i = 0; i < Children.Count; i++)
				{
					if (Children[i].ID == id)
					{
						entry = Children[i];
						return false;
					}
				}
			}
			entry = new CodeProfilerEntry(id, this);
			Children.Add(entry);
			return true;
		}

		#endregion

		#region Duration

		[NonSerialized]
		public double StartTime;

		public double LastDuration;

		public double TotalDuration;
		public int TotalCount;
		public double AverageDuration
		{
			get
			{
				return TotalCount > 0
						? TotalDuration / TotalCount
						: 0.0;
			}
		}

		public readonly RunningHotMeanFloat RunningAverageDuration = new RunningHotMeanFloat(20);

		#endregion
	}

}
