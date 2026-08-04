using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Extenity.IMGUIToolbox.Editor
{
    [Serializable]
#if UNITY_6000_4_OR_NEWER
    public class ExtenityTreeViewState : TreeViewState<EntityId> { }
#else
    public class ExtenityTreeViewState : TreeViewState<int> { }
#endif
}