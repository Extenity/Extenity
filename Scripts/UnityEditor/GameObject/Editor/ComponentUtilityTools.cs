using System;
using UnityEngine;

namespace Extenity.GameObjectToolbox.Editor
{

	public static class ComponentUtilityTools
	{
		#region Find Component Index

		public static int FindComponentIndex(this Transform me, Component component)
		{
			var components = me.GetComponents<Component>();
			for (var i = 0; i < components.Length; i++)
			{
				if (components[i] == component)
					return i;
			}
			return -1;
		}

		public static bool FindComponentIndices(this Transform me, Component component1, Component component2, out int index1, out int index2)
		{
			var components = me.GetComponents<Component>();
			index1 = -1;
			index2 = -1;
			for (var i = 0; i < components.Length; i++)
			{
				var item = components[i];
				if (item == component1)
				{
					index1 = i;
					if (index2 >= 0)
						return true;
				}
				else if (item == component2)
				{
					index2 = i;
					if (index1 >= 0)
						return true;
				}
			}
			return false;
		}

		#endregion

		#region Move Component

		public static void MoveComponent(this Component me, int moveAmount)
		{
			if (moveAmount == 0)
				return;

			if (moveAmount < 0)
			{
				moveAmount = -moveAmount;
				for (int i = 0; i < moveAmount; i++)
					UnityEditorInternal.ComponentUtility.MoveComponentUp(me);
			}
			else
			{
				for (int i = 0; i < moveAmount; i++)
					UnityEditorInternal.ComponentUtility.MoveComponentDown(me);
			}
		}

		public static int MoveComponentAbove(this Component me, Component target)
		{
			if (!me)
				throw new ArgumentNullException("me");
			if (!target)
				throw new ArgumentNullException("target");
			if (me == target)
				return 0; // Ignore move request
			if (me.transform != target.transform)
				throw new Exception("Tried to relatively move components between different objects.");

			int indexMe;
			int indexTarget;
			if (!me.transform.FindComponentIndices(me, target, out indexMe, out indexTarget))
				throw new Exception("Internal error!"); // That's odd.
			int indexShouldBe = indexTarget;

			int upTopFactor = indexMe < indexTarget ? -1 : 0;
			var moveAmount = indexShouldBe - indexMe + upTopFactor;
			MoveComponent(me, moveAmount);
			return moveAmount;
		}

		public static int MoveComponentBelow(this Component me, Component target)
		{
			if (!me)
				throw new ArgumentNullException("me");
			if (!target)
				throw new ArgumentNullException("target");
			if (me == target)
				return 0; // Ignore move request
			if (me.transform != target.transform)
				throw new Exception("Tried to relatively move components between different objects.");

			int indexMe;
			int indexTarget;
			if (!me.transform.FindComponentIndices(me, target, out indexMe, out indexTarget))
				throw new Exception("Internal error!"); // That's odd.
			int indexShouldBe = indexTarget + 1;

			int upTopFactor = indexMe < indexTarget ? -1 : 0;
			var moveAmount = indexShouldBe - indexMe + upTopFactor;
			MoveComponent(me, moveAmount);
			return moveAmount;
		}

		#endregion
	}

}
