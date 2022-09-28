#if ExtenityScreenManagement

using System;
using Extenity.DataToolbox;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.UIToolbox.ScreenManagement
{

	[Serializable]
	public class Screen
	{
		[BoxGroup("Setup")]
		public string Name => Prefab.GameObjectNameSafe();

		[Required, AssetsOnly]
		[BoxGroup("Setup")]
		[Tooltip("Screen prefab should have a Panel component in its parent so that the system can take the control of its visibility.")]
		public Panel Prefab;

		[NonSerialized]
		[BoxGroup("Status")]
		[ShowInInspector, ReadOnly]
		public Panel Instance;

		[NonSerialized]
		[BoxGroup("Status"), ShowInInspector, ReadOnly]
		public int ShowRequestCount;
		[NonSerialized]
		[BoxGroup("Status"), ShowInInspector, ReadOnly]
		public int HideRequestCount;
		[NonSerialized]
		[BoxGroup("Status"), ShowInInspector, ReadOnly]
		public int CreateRequestCount;
	}

}

#endif
