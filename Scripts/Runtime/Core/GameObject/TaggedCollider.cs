#if UNITY

using System;
using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	[Serializable]
	public struct TaggedCollider
	{
		public string Tag;

		private Collider _Collider;
		public Collider Collider
		{
			get
			{
				if (!_Collider)
				{
					var go = GameObjectTools.FindSingleObjectWithTagEnsured(Tag);
					_Collider = go.GetComponent<Collider>();
				}
				return _Collider;
			}
		}
	}

}

#endif
