using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Extenity.IMGUIToolbox.Editor
{

	// The TreeModel is a utility class working on a list of serializable TreeElements where the order and the depth of each TreeElement define
	// the tree structure. Note that the TreeModel itself is not serializable (in Unity we are currently limited to serializing lists/arrays) but the 
	// input list is.
	// The tree representation (parent and children references) are then build internally using TreeElementUtility.ListToTree (using depth 
	// values of the elements). 
	// The first element of the input list is required to have depth == -1 (the hiddenroot) and the rest to have
	// depth >= 0 (otherwise an exception will be thrown)
	public class TreeModel<T> where T : TreeElement
	{
		private IList<T> m_Data;
		private T m_Root;

		public T root { get { return m_Root; } set { m_Root = value; } }

		public TreeModel(IList<T> data)
		{
			SetData(data);
		}

#if UNITY_6000_4_OR_NEWER
		public T Find(EntityId id)
#else
		public T Find(int id)
#endif
		{
			return m_Data.FirstOrDefault(element => element.id == id);
		}

		public void SetData(IList<T> data)
		{
			Init(data);
		}

		private void Init(IList<T> data)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data), "Input data is null. Ensure input is a non-null list.");

			m_Data = data;
			if (m_Data.Count > 0)
				m_Root = TreeElementUtility.ListToTree(data);
		}

#if UNITY_6000_4_OR_NEWER
		public IList<EntityId> GetAncestors(EntityId id)
#else
		public IList<int> GetAncestors(int id)
#endif
		{
#if UNITY_6000_4_OR_NEWER
			var parents = new List<EntityId>();
#else
			var parents = new List<int>();
#endif
			TreeElement T = Find(id);
			if (T != null)
			{
				while (T.parent != null)
				{
					parents.Add(T.parent.id);
					T = T.parent;
				}
			}
			return parents;
		}

#if UNITY_6000_4_OR_NEWER
		public IList<EntityId> GetDescendantsThatHaveChildren(EntityId id)
#else
		public IList<int> GetDescendantsThatHaveChildren(int id)
#endif
		{
			T searchFromThis = Find(id);
			if (searchFromThis != null)
			{
				return GetParentsBelowStackBased(searchFromThis);
			}
#if UNITY_6000_4_OR_NEWER
			return new List<EntityId>();
#else
			return new List<int>();
#endif
		}

#if UNITY_6000_4_OR_NEWER
		private IList<EntityId> GetParentsBelowStackBased(TreeElement searchFromThis)
#else
		private IList<int> GetParentsBelowStackBased(TreeElement searchFromThis)
#endif
		{
			Stack<TreeElement> stack = new Stack<TreeElement>();
			stack.Push(searchFromThis);

#if UNITY_6000_4_OR_NEWER
			var parentsBelow = new List<EntityId>();
#else
			var parentsBelow = new List<int>();
#endif
			while (stack.Count > 0)
			{
				TreeElement current = stack.Pop();
				if (current.hasChildren)
				{
					parentsBelow.Add(current.id);
					foreach (var T in current.children)
					{
						stack.Push(T);
					}
				}
			}

			return parentsBelow;
		}
	}

}
