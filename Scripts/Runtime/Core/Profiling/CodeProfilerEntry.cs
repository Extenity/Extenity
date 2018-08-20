using System;
using Extenity.DataToolbox;

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
			Parent.Children = Parent.Children.Add(this);
		}

		/// <summary>
		/// This constructor is dedicated to BaseEntry. Do not use it elsewhere.
		/// </summary>
		internal CodeProfilerEntry()
		{
			ID = 0;
			Parent = null;
			CollectionTools.ResizeIfRequired(ref Children, 20); // Preallocate some
		}

		#endregion

		#region Hierarch, Parent, Children

		[NonSerialized]
		public readonly CodeProfilerEntry Parent;

		// The array size does not change so often, so it will better be an array instead of a list for performance concerns.
		public CodeProfilerEntry[] Children;

		public bool IsChildExists(int id)
		{
			if (Children != null)
			{
				for (int i = 0; i < Children.Length; i++)
				{
					if (Children[i].ID == id)
						return true;
				}
			}
			return false;
		}

		public CodeProfilerEntry GetOrAddChild(int id)
		{
			if (Children != null)
			{
				for (int i = 0; i < Children.Length; i++)
				{
					if (Children[i].ID == id)
						return Children[i];
				}
			}
			var newEntry = new CodeProfilerEntry(id, this);
			Children = Children.Add(newEntry);
			return newEntry;
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

		#endregion
	}

}
