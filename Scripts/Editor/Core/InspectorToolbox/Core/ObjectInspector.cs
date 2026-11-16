using System;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// The base of a custom editor for a MonoBehaviour or ScriptableObject. Derive, add
	/// <c>[CustomEditor(typeof(T))]</c> and declare only what differs from the default in
	/// <see cref="Configure"/>: every serialized field that is not mentioned is still drawn, in declaration
	/// order, so a newly added field can never be hidden by a forgotten editor.
	/// </summary>
	public abstract class ObjectInspector<T> : UnityEditor.Editor, ICustomizedInspector where T : UnityEngine.Object
	{
		public Type TargetType => typeof(T);

		/// <summary>The inspector built by the last <see cref="CreateInspectorGUI"/> call. Null before that.</summary>
		public InspectorInstance Instance { get; private set; }

		/// <summary>Runs once per editor class. The result is cached and shared by every instance of the editor.</summary>
		protected abstract void Configure(InspectorBuilder<T> b);

		public InspectorBlueprint GetBlueprint()
		{
			return InspectorBlueprints.GetOrBuild(GetType(), () =>
			{
				var builder = new InspectorBuilder<T>(GetType());
				Configure(builder);
				return builder.Build();
			});
		}

		public sealed override VisualElement CreateInspectorGUI()
		{
			Instance = GetBlueprint().Instantiate(serializedObject, null);
			return Instance.Root;
		}
	}

}
