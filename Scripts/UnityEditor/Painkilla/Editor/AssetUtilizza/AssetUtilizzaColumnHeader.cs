using UnityEditor.IMGUI.Controls;

namespace Extenity.PainkillaTool.Editor
{

	public class AssetUtilizzaColumnHeader : MultiColumnHeader
	{
		public AssetUtilizzaColumnHeader(MultiColumnHeaderState state) : base(state)
		{
			mode = Mode.DefaultHeader;
		}

		public enum Mode
		{
			LargeHeader,
			DefaultHeader,
			MinimumHeaderWithoutSorting
		}

		private Mode _Mode;
		public Mode mode
		{
			get
			{
				return _Mode;
			}
			set
			{
				_Mode = value;
				switch (_Mode)
				{
					case Mode.LargeHeader:
						canSort = true;
						height = 37f;
						break;
					case Mode.DefaultHeader:
						canSort = true;
						height = DefaultGUI.defaultHeight;
						break;
					case Mode.MinimumHeaderWithoutSorting:
						canSort = false;
						height = DefaultGUI.minimumHeight;
						break;
				}
			}
		}
	}

}
