namespace Extenity.UIToolbox
{

	/// <summary>
	/// A simple component to define a UI element or a scene object to show a tooltip when hovered over. Use
	/// <see cref="TooltippedMonoBehaviour.SetTooltip"/> to update tooltip content. Then a tooltip will be automatically
	/// displayed when the pointer hovers over this object.
	///
	/// If the object already has its own script and that script manages the tooltip content to be shown, then it might
	/// be easier to derive that script from <see cref="TooltippedMonoBehaviour"/> and use the functionality directly,
	/// instead of adding <see cref="TooltipArea"/> as another component.
	/// </summary>
	public class TooltipArea : TooltippedMonoBehaviour
	{
	}

}
