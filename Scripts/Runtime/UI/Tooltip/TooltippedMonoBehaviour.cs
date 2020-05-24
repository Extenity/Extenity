using UnityEngine;

namespace Extenity.UIToolbox
{

	/// <summary>
	/// When an object is required to show a tooltip, either add <see cref="TooltipArea"/> component to that object or
	/// derive that object's script from <see cref="TooltippedMonoBehaviour"/>. Then call
	/// <see cref="TooltippedMonoBehaviour.SetTooltip"/> to update tooltip content and a tooltip will be automatically
	/// displayed when the pointer hovers over this object.
	///
	/// Tooltip system requires a <see cref="TooltipRaycaster"/> setup.
	/// </summary>
	[DisallowMultipleComponent]
	public abstract class TooltippedMonoBehaviour : MonoBehaviour
	{
		#region Content

		public string FlavorContent { get; private set; }
		public string HeaderContent { get; private set; }
		public string DescriptionContent { get; private set; }

		public void SetTooltip(string headerContent, string flavorContent, string descriptionContent)
		{
			GenerateNewTooltipContentVersion();
			HeaderContent = headerContent;
			FlavorContent = flavorContent;
			DescriptionContent = descriptionContent;
		}

		public void SetTooltipHeader(string headerContent, bool resetOthers = false)
		{
			GenerateNewTooltipContentVersion();
			HeaderContent = headerContent;
			if (resetOthers)
			{
				FlavorContent = "";
				DescriptionContent = "";
			}
		}

		public void SetTooltipHeaderAndFlavor(string headerContent, string flavorContent, bool resetOthers = false)
		{
			GenerateNewTooltipContentVersion();
			HeaderContent = headerContent;
			FlavorContent = flavorContent;
			if (resetOthers)
			{
				DescriptionContent = "";
			}
		}

		public void SetTooltipDescription(string descriptionContent, bool resetOthers = false)
		{
			GenerateNewTooltipContentVersion();
			DescriptionContent = descriptionContent;
			if (resetOthers)
			{
				HeaderContent = "";
				FlavorContent = "";
			}
		}

		public void ClearTooltip()
		{
			GenerateNewTooltipContentVersion();
			HeaderContent = null;
			FlavorContent = null;
			DescriptionContent = null;
		}

		#endregion

		#region Content Version

		public int ContentVersion { get; private set; }

		private static int LastGivenContentVersion = 100;

		private void GenerateNewTooltipContentVersion()
		{
			ContentVersion = ++LastGivenContentVersion;
		}

		#endregion
	}

}
