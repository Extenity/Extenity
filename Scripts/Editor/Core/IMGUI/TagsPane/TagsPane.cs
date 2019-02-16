using System.Collections.Generic;

namespace Extenity.IMGUIToolbox.Editor
{

	public class TagsPane
	{
		public int EditingIndex = -1;
		public string PreviousTagValueBeforeEditing = "";
		public bool NeedsRepaint = false;
		public bool NeedsEditingFocus = false;

		internal List<int> LineBreaks = new List<int>();
	}

}
