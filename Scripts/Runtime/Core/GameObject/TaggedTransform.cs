#if UNITY_5_3_OR_NEWER

using System;
using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	[Serializable]
	public struct TaggedTransform
	{
		public string Tag;

		private Transform _Transform;
		public Transform Transform
		{
			get
			{
				if (!_Transform)
				{
					var go = GameObjectTools.FindSingleObjectWithTagEnsured(Tag);
					_Transform = go.transform;
				}
				return _Transform;
			}
		}
	}

}

#endif
