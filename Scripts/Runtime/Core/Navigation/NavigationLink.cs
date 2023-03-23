#if UNITY

#if !DisableUnityAI

using Extenity.DataToolbox;
using UnityEngine;
using UnityEngine.AI;

namespace Extenity.NavigationToolbox
{

	/// <summary>
	/// Based on https://github.com/Unity-Technologies/NavMeshComponents
	/// </summary>
	[ExecuteInEditMode]
	[DefaultExecutionOrder(-101)]
	[AddComponentMenu("Navigation/Navigation Link", 33)]
	[HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
	public class NavigationLink : MonoBehaviour
	{
		// Any change in this field requires 'UpdateLink' to be called.
		public Vector3 StartPoint;

		// Any change in this field requires 'UpdateLink' to be called.
		public Vector3 EndPoint;

		// Any change in this field requires 'UpdateLink' to be called.
		public float CostModifier;

		// Any change in this field requires 'UpdateLink' to be called.
		public bool Bidirectional;

		// Any change in this field requires 'UpdateLink' to be called.
		public float Width;

		// Any change in this field requires 'UpdateLink' to be called.
		public int Area;

		// Any change in this field requires 'UpdateLink' to be called.
		public int AgentTypeID;

		// Any change in this field requires 'UpdateLink' to be called.
		public LayerMask SnapLayerMask;

		private NavMeshLinkInstance LinkInstance = new NavMeshLinkInstance();

		private void OnEnable()
		{
			AddLink();
		}

		private void OnDisable()
		{
			LinkInstance.Remove();
		}

		public void UpdateLink()
		{
			LinkInstance.Remove();
			AddLink();
		}

		private void AddLink()
		{
#if UNITY_EDITOR
			if (LinkInstance.valid)
			{
				Log.ErrorWithContext(this, "Link is already added");
				return;
			}
#endif

			var link = new NavMeshLinkData
			{
				startPosition = StartPoint,
				endPosition = EndPoint,
				costModifier = CostModifier,
				bidirectional = Bidirectional,
				width = Width,
				area = Area,
				agentTypeID = AgentTypeID
			};
			LinkInstance = NavMesh.AddLink(link, Vector3.zero, Quaternion.identity);
			//LinkInstance = NavMesh.AddLink(link, transform.position, transform.rotation); // TODO: Find a proper way of making the NavigationLink positions to be configured as Local or World coordinates. See 1878201.
			LinkInstance.owner = this;
			if (!LinkInstance.valid)
				Log.ErrorWithContext(this, $"Failed to add navmesh link of object '{this.FullGameObjectName()}'.");
		}

#if UNITY_EDITOR
		protected void OnValidate()
		{
			if (!LinkInstance.valid)
				return;

			UpdateLink();
		}
#endif

		#region Log

		private static readonly Logger Log = new(nameof(NavigationLink));

		#endregion
	}

}

#endif

#endif
