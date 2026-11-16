using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// Declares how the inspector of <typeparamref name="T"/> deviates from the default. Every serialized field
	/// that is not mentioned is drawn by a plain property field at the place it is declared.
	/// </summary>
	public sealed class InspectorBuilder<T> : IInspectorNodeContainer<T>
	{
		private readonly InspectorBlueprint Blueprint;

		internal InspectorBuilder(Type inspectorType)
		{
			Blueprint = new InspectorBlueprint(typeof(T), inspectorType);
		}

		internal InspectorBlueprint Build()
		{
			Blueprint.Seal();
			return Blueprint;
		}

		InspectorBuilder<T> IInspectorNodeContainer<T>.Builder => this;

		#region Inspector options

		/// <summary>How a nested inspector presents itself. Ignored by the inspector of an object.</summary>
		public InspectorBuilder<T> Header(HeaderMode mode)
		{
			Blueprint.ThrowIfSealed();
			Blueprint.Header = mode;
			return this;
		}

		/// <summary>
		/// The title of the value when it is an element of a list: the text of its row's header, and what the
		/// list's search looks at. A null or empty result falls back to the label the list asks for. A field that
		/// is not in a list keeps its own name as header.
		/// </summary>
		public InspectorBuilder<T> Title(Func<T, string> title)
		{
			Blueprint.ThrowIfSealed();
			if (title == null)
				throw new ArgumentNullException(nameof(title));
			Blueprint.Title = target => title((T)target);
			return this;
		}

		/// <summary>Leaves out the Script field of a component or ScriptableObject.</summary>
		public InspectorBuilder<T> HideScriptField()
		{
			Blueprint.ThrowIfSealed();
			Blueprint.HideScriptField = true;
			return this;
		}

		/// <summary>
		/// Re-evaluates a nested inspector when anything in the owning object changes, not only its own property.
		/// Needed when a rule reads the owning object through the context.
		/// </summary>
		public InspectorBuilder<T> TrackWholeObject()
		{
			Blueprint.ThrowIfSealed();
			Blueprint.TrackWholeObject = true;
			return this;
		}

		/// <summary>Re-evaluates the rules at the given interval while the editor is in Play Mode, for rules that read runtime state.</summary>
		public InspectorBuilder<T> PollWhilePlaying(int intervalMs = 250)
		{
			Blueprint.ThrowIfSealed();
			if (intervalMs < 16)
				throw new ArgumentOutOfRangeException(nameof(intervalMs), "The polling interval must be at least 16 ms.");
			Blueprint.PollWhilePlayingIntervalMs = intervalMs;
			return this;
		}

		/// <summary>
		/// Draws each top-level field that is a serializable class or struct as a section: a full-width header
		/// that follows the field's expanded state, with content that is built when opened and dropped when
		/// closed. For objects with so much data that building everything at once is too slow.
		/// </summary>
		public InspectorBuilder<T> TopLevelFieldsAsSections(bool searchable = true)
		{
			Blueprint.ThrowIfSealed();
			if (!Blueprint.IsObjectInspector)
				throw new InvalidOperationException($"{nameof(TopLevelFieldsAsSections)} is for the inspector of an object. '{typeof(T).Name}' is a nested type.");
			Blueprint.Sections = true;
			Blueprint.SectionsSearchable = searchable;
			return this;
		}

		/// <summary>A validation of the whole object. Its message is shown at the top.</summary>
		public InspectorBuilder<T> Validate(Func<T, InspectorContext, ValidationResult> rule)
		{
			Blueprint.ThrowIfSealed();
			if (rule == null)
				throw new ArgumentNullException(nameof(rule));
			Blueprint.ObjectValidators.Add((target, context) => rule((T)target, context));
			return this;
		}

		#endregion

		#region Fields

		public FieldRule<T, TField> Field<TField>(Expression<Func<T, TField>> field)
		{
			return new FieldRule<T, TField>(Blueprint.GetOrAddRule(FieldNames.Get(field)));
		}

		/// <summary>A field by its serialized name, for private serialized fields. Throws when the type has no such field.</summary>
		public FieldRule<T, object> Field(string serializedFieldName)
		{
			return new FieldRule<T, object>(Blueprint.GetOrAddRule(serializedFieldName));
		}

		public ListRule<T, TElement> List<TElement>(Expression<Func<T, IList<TElement>>> list)
		{
			return new ListRule<T, TElement>(Blueprint.GetOrAddRule(FieldNames.Get(list)));
		}

		/// <summary>A list by its serialized name, for private serialized fields.</summary>
		public ListRule<T, TElement> List<TElement>(string serializedFieldName)
		{
			return new ListRule<T, TElement>(Blueprint.GetOrAddRule(serializedFieldName));
		}

		#endregion

		#region Nodes

		/// <summary>
		/// Adds a node at the top level. A node that claims fields is drawn where the first of them is declared.
		/// Any other node goes to the bottom, in call order, unless it is placed explicitly.
		/// </summary>
		public TNode Add<TNode>(TNode node) where TNode : InspectorNode
		{
			Blueprint.ThrowIfSealed();
			if (node == null)
				throw new ArgumentNullException(nameof(node));
			Blueprint.Nodes.Add(node);
			return node;
		}

		#endregion
	}

}
