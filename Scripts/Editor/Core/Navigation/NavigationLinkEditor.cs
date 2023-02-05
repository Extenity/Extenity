#if !DisableUnityAI

using UnityEditor;
using UnityEditor.AI;
using UnityEngine;

namespace Extenity.NavigationToolbox.Editor
{

	[CanEditMultipleObjects]
	[CustomEditor(typeof(NavigationLink))]
	public class NavigationLinkEditor : UnityEditor.Editor
	{
		private SerializedProperty AgentTypeIDProperty;
		private SerializedProperty AreaProperty;
		private SerializedProperty CostModifierProperty;
		private SerializedProperty WidthProperty;
		private SerializedProperty BidirectionalProperty;
		private SerializedProperty StartPointProperty;
		private SerializedProperty EndPointProperty;
		private SerializedProperty SnapLayerMaskProperty;

		public static readonly Color NavMeshLinkHandleColor = new Color(255f, 167f, 39f, 210f) / 255;
		public static readonly Color NavMeshLinkDisabledHandleColor = new Color(255f * 0.75f, 167f * 0.75f, 39f * 0.75f, 100f) / 255;

		private void OnEnable()
		{
			AgentTypeIDProperty = serializedObject.FindProperty("AgentTypeID");
			AreaProperty = serializedObject.FindProperty("Area");
			CostModifierProperty = serializedObject.FindProperty("CostModifier");
			WidthProperty = serializedObject.FindProperty("Width");
			BidirectionalProperty = serializedObject.FindProperty("Bidirectional");
			StartPointProperty = serializedObject.FindProperty("StartPoint");
			EndPointProperty = serializedObject.FindProperty("EndPoint");
			SnapLayerMaskProperty = serializedObject.FindProperty("SnapLayerMask");

			NavigationEditorGUITools.ResetGizmoSelectionOfCustomNavigationLink();
#if UNITY_2022
			throw new System.NotImplementedException();
#else
			NavMeshVisualizationSettings.showNavigation++;
#endif
		}

		private void OnDisable()
		{
			NavigationEditorGUITools.ResetGizmoSelectionOfCustomNavigationLink();
#if UNITY_2022
			throw new System.NotImplementedException();
#else
			NavMeshVisualizationSettings.showNavigation--;
#endif
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			NavigationEditorGUITools.AgentTypePopup("Agent Type", AgentTypeIDProperty);
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(StartPointProperty);
			EditorGUILayout.PropertyField(EndPointProperty);
			EditorGUILayout.PropertyField(WidthProperty);
			EditorGUILayout.PropertyField(BidirectionalProperty);
			EditorGUILayout.PropertyField(SnapLayerMaskProperty);

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(CostModifierProperty);

			NavigationEditorGUITools.AreaPopup("Area Type", AreaProperty);

			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.Space();
		}

		[DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.Pickable)]
		private static void RenderBoxGizmo(NavigationLink navLink, GizmoType gizmoType)
		{
			if (!EditorApplication.isPlaying)
				navLink.UpdateLink();

			var color = navLink.enabled ? NavMeshLinkHandleColor : NavMeshLinkDisabledHandleColor;
			// TODO: Find a proper way of making the NavigationLink positions to be configured as Local or World coordinates. See 1878201.
			//NavigationGUITools.DrawCustomNavigationLinkGizmos(navLink.transform, color, () => navLink.StartPoint, () => navLink.EndPoint);
			NavigationEditorGUITools.DrawCustomNavigationLinkGizmos(null, color, () => navLink.StartPoint, () => navLink.EndPoint);
		}

		[DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
		private static void RenderBoxGizmoNotSelected(NavigationLink navLink, GizmoType gizmoType)
		{
#if UNITY_2022
			throw new System.NotImplementedException();
#else
			if (NavMeshVisualizationSettings.showNavigation > 0)
			{
				var color = navLink.enabled ? NavMeshLinkHandleColor : NavMeshLinkDisabledHandleColor;
				// TODO: Find a proper way of making the NavigationLink positions to be configured as Local or World coordinates. See 1878201.
				//NavigationGUITools.DrawCustomNavigationLinkGizmos(navLink.transform, color, () => navLink.StartPoint, () => navLink.EndPoint);
				NavigationEditorGUITools.DrawCustomNavigationLinkGizmos(null, color, () => navLink.StartPoint, () => navLink.EndPoint);
			}
#endif
		}

		public void OnSceneGUI()
		{
			var navLink = (NavigationLink)target;
			if (!navLink.enabled)
				return;

			NavigationEditorGUITools.DrawCustomNavigationLinkPoints(navLink,
				() => navLink.StartPoint, () => navLink.EndPoint,
				value =>
				{
					navLink.StartPoint = value;
					navLink.UpdateLink();
				}, value =>
				{
					navLink.EndPoint = value;
					navLink.UpdateLink();
				},
				true, true, NavMeshLinkHandleColor,
				true, navLink.SnapLayerMask);
		}
	}

}

#endif
