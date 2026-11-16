using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// Turns <c>x => x.Field</c> into the name Unity serializes the field under, and finds serialized fields
	/// by that name.
	/// </summary>
	public static class FieldNames
	{
		private const BindingFlags DeclaredInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

		public static string Get(LambdaExpression expression)
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));

			var body = expression.Body;
			while (body is UnaryExpression unary && (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked || unary.NodeType == ExpressionType.TypeAs))
				body = unary.Operand;

			if (body is MemberExpression member && member.Expression is ParameterExpression)
			{
				if (member.Member is FieldInfo field)
					return field.Name;
				if (member.Member is PropertyInfo property)
				{
					// An auto-property serialized with [field: SerializeField].
					var backingFieldName = $"<{property.Name}>k__BackingField";
					if (property.DeclaringType != null && property.DeclaringType.GetField(backingFieldName, DeclaredInstance) != null)
						return backingFieldName;
					throw new ArgumentException($"'{property.DeclaringType?.Name}.{property.Name}' is a property without a serialized backing field. Name the serialized field instead: {expression}");
				}
			}
			throw new ArgumentException($"The expression must name one field of the target directly, like 'x => x.Field'. Got: {expression}");
		}

		/// <summary>
		/// Finds an instance field by its serialized name on the type or one of its base types.
		/// Returns false when there is none.
		/// </summary>
		public static bool TryFind(Type type, string fieldName, out FieldInfo field)
		{
			for (var current = type; current != null && current != typeof(object); current = current.BaseType)
			{
				field = current.GetField(fieldName, DeclaredInstance);
				if (field != null)
					return true;
			}
			field = null;
			return false;
		}

		/// <summary>True when Unity can serialize the field as far as its declaration tells.</summary>
		public static bool IsDeclaredSerialized(FieldInfo field)
		{
			if (field.IsStatic || field.IsInitOnly || field.IsLiteral || field.IsNotSerialized)
				return false;
			return field.IsPublic
			       || field.IsDefined(typeof(SerializeField), true)
			       || field.IsDefined(typeof(SerializeReference), true);
		}
	}

}
