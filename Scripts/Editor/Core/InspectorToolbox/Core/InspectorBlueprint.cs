using System;
using System.Collections.Generic;
using UnityEditor;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// The declared customization of one type, built once by running <c>Configure</c> and then only read.
	/// <see cref="Instantiate"/> makes the element tree for one object or one property.
	/// </summary>
	public sealed class InspectorBlueprint
	{
		/// <summary>The type the customization was written for. A derived instance can be drawn with it too.</summary>
		public Type TargetType { get; }
		/// <summary>The editor or drawer class that declared the customization.</summary>
		public Type InspectorType { get; }
		/// <summary>True for the inspector of an object, false for a nested class, struct or managed reference.</summary>
		public bool IsObjectInspector { get; }

		public HeaderMode Header { get; internal set; }
		public Func<object, string> Title { get; internal set; }
		public bool HideScriptField { get; internal set; }
		public bool TrackWholeObject { get; internal set; }
		public int PollWhilePlayingIntervalMs { get; internal set; }
		public bool Sections { get; internal set; }
		public bool SectionsSearchable { get; internal set; }

		/// <summary>How many instances were made from this blueprint since the last domain reload or reset.</summary>
		public int InstantiationCount { get; internal set; }

		internal readonly List<FieldRuleData> Rules = new List<FieldRuleData>();
		internal readonly Dictionary<string, FieldRuleData> RulesByName = new Dictionary<string, FieldRuleData>(StringComparer.Ordinal);
		internal readonly List<InspectorNode> Nodes = new List<InspectorNode>();
		internal readonly List<Func<object, InspectorContext, ValidationResult>> ObjectValidators = new List<Func<object, InspectorContext, ValidationResult>>();

		private string[] _ReferencedFieldNames = Array.Empty<string>();
		/// <summary>Every serialized field name the customization mentions.</summary>
		public IReadOnlyList<string> ReferencedFieldNames => _ReferencedFieldNames;

		public bool IsSealed { get; private set; }

		internal InspectorBlueprint(Type targetType, Type inspectorType)
		{
			TargetType = targetType;
			InspectorType = inspectorType;
			IsObjectInspector = typeof(UnityEngine.Object).IsAssignableFrom(targetType);
		}

		#region Build

		internal void ThrowIfSealed()
		{
			if (IsSealed)
				throw new InvalidOperationException($"The inspector of '{TargetType.Name}' is already built. Rules can only be declared inside Configure.");
		}

		internal FieldRuleData GetOrAddRule(string fieldName)
		{
			ThrowIfSealed();
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentException("A field name is required.", nameof(fieldName));
			if (RulesByName.TryGetValue(fieldName, out var existing))
				return existing;

			if (!FieldNames.TryFind(TargetType, fieldName, out var field))
				throw new ArgumentException($"'{TargetType.FullName}' has no field named '{fieldName}'.");
			if (!FieldNames.IsDeclaredSerialized(field))
				throw new ArgumentException($"Field '{TargetType.FullName}.{fieldName}' is not serialized by Unity, so it has no property to draw. Use a live value for members that are not serialized.");

			var rule = new FieldRuleData(fieldName, field);
			Rules.Add(rule);
			RulesByName.Add(fieldName, rule);
			return rule;
		}

		internal void Seal()
		{
			ThrowIfSealed();

			// Which node draws which field. A field can be drawn once only.
			foreach (var node in Nodes)
			{
				var claimedByThisNode = new HashSet<string>(StringComparer.Ordinal);
				foreach (var fieldName in node.ClaimedFieldNames)
				{
					var rule = GetOrAddRule(fieldName);
					if (!claimedByThisNode.Add(fieldName) || (rule.Container != null && rule.Container != node))
						throw new InvalidOperationException($"Field '{TargetType.Name}.{fieldName}' is placed in more than one group or row. A field can be drawn once only.");
					rule.Container = node;
				}
			}

			// Looking up a placement anchor can add a rule, so the list is walked by index.
			for (var i = 0; i < Rules.Count; i++)
			{
				var rule = Rules[i];
				if (rule.Container != null && rule.Placement.Kind != PlacementKind.Default)
					throw new InvalidOperationException($"Field '{TargetType.Name}.{rule.FieldName}' is inside a group or a row, where fields are drawn in the order they are added. Place the group or the row instead.");
				ValidatePlacement(rule.Placement, $"field '{rule.FieldName}'");
				if (rule.Placement.AnchorFieldName == rule.FieldName && rule.Placement.AnchorFieldName != null)
					throw new InvalidOperationException($"Field '{TargetType.Name}.{rule.FieldName}' is placed relative to itself.");
			}
			foreach (var node in Nodes)
			{
				ValidatePlacement(node.Placement, $"node '{node.GetType().Name}'");
			}

			var names = new string[Rules.Count];
			for (var i = 0; i < names.Length; i++)
				names[i] = Rules[i].FieldName;
			_ReferencedFieldNames = names;
			IsSealed = true;
		}

		private void ValidatePlacement(Placement placement, string what)
		{
			if (placement.Kind != PlacementKind.Before && placement.Kind != PlacementKind.After)
				return;
			// Looking the anchor up also registers it as a referenced field and throws when it does not exist.
			GetOrAddRule(placement.AnchorFieldName);
		}

		#endregion

		#region Layout

		/// <summary>
		/// The order the fields and nodes are drawn in, for the given serialized children (in declaration order).
		/// </summary>
		internal List<LayoutEntry> PlanLayout(IReadOnlyList<string> childNames)
		{
			var entries = new List<LayoutEntry>(childNames.Count + Nodes.Count);
			var emitted = new HashSet<InspectorNode>();

			foreach (var childName in childNames)
			{
				if (RulesByName.TryGetValue(childName, out var rule) && rule.Container != null)
				{
					// The container is drawn where the first of its fields is declared.
					if (rule.Container.Placement.Kind == PlacementKind.Default && emitted.Add(rule.Container))
						entries.Add(new LayoutEntry(rule.Container));
					continue;
				}
				entries.Add(new LayoutEntry(childName));
			}

			foreach (var node in Nodes)
			{
				if (node.Placement.Kind == PlacementKind.Default && emitted.Add(node))
					entries.Add(new LayoutEntry(node));
			}

			// Explicit placements, fields first and then nodes, each in call order.
			var topInsertIndex = entries.Count > 0 && entries[0].FieldName == ScriptFieldName ? 1 : 0;
			foreach (var rule in Rules)
			{
				if (rule.Placement.Kind == PlacementKind.Default || rule.Container != null)
					continue;
				var index = entries.FindIndex(entry => entry.FieldName == rule.FieldName);
				if (index < 0)
					continue; // Not a visible property of this instance. Instantiate reports it.
				var moved = entries[index];
				entries.RemoveAt(index);
				if (index < topInsertIndex)
					topInsertIndex--;
				Place(entries, moved, rule.Placement, ref topInsertIndex);
			}
			foreach (var node in Nodes)
			{
				if (node.Placement.Kind != PlacementKind.Default)
					Place(entries, new LayoutEntry(node), node.Placement, ref topInsertIndex);
			}

			return entries;
		}

		private void Place(List<LayoutEntry> entries, LayoutEntry moved, Placement placement, ref int topInsertIndex)
		{
			switch (placement.Kind)
			{
				case PlacementKind.AtTop:
					entries.Insert(topInsertIndex++, moved);
					break;
				case PlacementKind.AtBottom:
					entries.Add(moved);
					break;
				case PlacementKind.Before:
				case PlacementKind.After:
				{
					var anchorIndex = FindAnchor(entries, placement.AnchorFieldName);
					if (anchorIndex < 0)
						throw new InvalidOperationException($"The inspector of '{TargetType.Name}' places something relative to field '{placement.AnchorFieldName}', which is not a visible serialized property of the inspected value.");
					var insertIndex = placement.Kind == PlacementKind.Before ? anchorIndex : anchorIndex + 1;
					entries.Insert(insertIndex, moved);
					if (insertIndex < topInsertIndex)
						topInsertIndex++;
					break;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(placement), placement.Kind, null);
			}
		}

		private int FindAnchor(List<LayoutEntry> entries, string anchorFieldName)
		{
			var container = RulesByName.TryGetValue(anchorFieldName, out var rule) ? rule.Container : null;
			for (var i = 0; i < entries.Count; i++)
			{
				if (container != null ? entries[i].Node == container : entries[i].FieldName == anchorFieldName)
					return i;
			}
			return -1;
		}

		internal const string ScriptFieldName = "m_Script";

		#endregion

		#region Instantiate

		/// <summary>
		/// Builds the element tree for an object (<paramref name="propertyPath"/> null) or for one of its
		/// properties. <paramref name="label"/> is the label the parent asks for: null for the property's own
		/// name, empty for no header at all. The tree is not bound: bind an ancestor to the serialized object.
		/// </summary>
		public InspectorInstance Instantiate(SerializedObject serializedObject, string propertyPath, string label = null)
		{
			if (!IsSealed)
				throw new InvalidOperationException($"The inspector of '{TargetType.Name}' is not built yet.");
			if (serializedObject == null)
				throw new ArgumentNullException(nameof(serializedObject));
			if (IsObjectInspector != string.IsNullOrEmpty(propertyPath))
			{
				throw new ArgumentException(IsObjectInspector
					? $"'{TargetType.Name}' is an object type. Its inspector draws the whole object, not the property '{propertyPath}'."
					: $"'{TargetType.Name}' is a nested type. Its inspector needs the path of the property to draw.");
			}
			InstantiationCount++;
			return new InspectorInstance(this, serializedObject, propertyPath, label);
		}

		#endregion
	}

}
