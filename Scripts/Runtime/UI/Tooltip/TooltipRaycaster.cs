using System;
using UnityEngine;

namespace Extenity.UIToolbox
{

	/// <summary>
	/// Setup a <see cref="TooltipRaycaster"/> so that it can manage when to show and hide tooltips. A good practice
	/// might be to add it as a sibling component to EventSystem.
	/// </summary>
	public class TooltipRaycaster : MonoBehaviour
	{
		#region Configuration

		public LayerMask TooltipObjectLayers;

		#endregion

		#region Links

		private Camera _Camera;
		private Camera Camera
		{
			get
			{
				if (!_Camera)
				{
					_Camera = Camera.main;
				}
				return _Camera;
			}
		}

		#endregion

		#region Detect Hovering Over UI and Scene Objects

		private void LateUpdate()
		{
			// See if the mouse is over a UI element.
			{
				var input = StandaloneImprovedInputModule.CurrentInput;
				if (input.IsPointerOverGameObject())
				{
					var hoveredObject = input.GameObjectUnderPointer();
					var tooltipDetails = GetTooltipOfObject(hoveredObject);
					if (tooltipDetails)
					{
						InformHover(tooltipDetails);
						return;
					}
					else
					{
						// Mouse is hovering over a UI element even though it does not have tooltip. So do not continue
						// to check if the mouse is over a scene object.
						InformHover(null);
						return;
					}
				}
			}

			// See if the mouse is over a scene object that displays a tooltip.
			{
				var ray = Camera.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray, out var hitInfo, float.MaxValue, TooltipObjectLayers.value))
				{
					var tooltipDetails = GetTooltipOfObject(hitInfo.collider.gameObject);
					if (tooltipDetails)
					{
						InformHover(tooltipDetails);
						return;
					}
				}
			}

			// Did not find any hovered tooltipped object.
			InformHover(null);
		}

		#endregion

		#region Hovered Object

		[NonSerialized]
		public TooltippedMonoBehaviour HoveredObject;

		private void InformHover(TooltippedMonoBehaviour tooltippedObject)
		{
			Log.Warning("Not implemented yet!");
			// var previouslyHoveredObject = HoveredObject;
			// HoveredObject = tooltippedObject;
			//
			// if (HoveredObject)
			// {
			// 	Tooltip.Instance.Show(HoveredObject, HoveredObject.HeaderContent, HoveredObject.FlavorContent, HoveredObject.DescriptionContent, HoveredObject.ContentVersion);
			// }
			// else if (previouslyHoveredObject)
			// {
			// 	Tooltip.Instance.Hide(previouslyHoveredObject);
			// }
			// else
			// {
			// 	Tooltip.Instance.Hide(null);
			// }
		}

		#endregion

		#region Get Tooltip of Object

		private TooltippedMonoBehaviour GetTooltipOfObject(GameObject go)
		{
			// TODO OPTIMIZATION: Using GetComponentInParent is not cool. Maybe cache the result? But then, we better reset the cache regularly, maybe once in every 3 seconds. That will hopefully prevent errors related to dynamically moved or pooled UI elements or overwhelmingly increasing size of the cache. Think wisely.
			return go.GetComponentInParent<TooltippedMonoBehaviour>();
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(TooltipRaycaster));

		#endregion
	}

}
