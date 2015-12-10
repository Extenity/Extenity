using System;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using System.Collections;

namespace Extenity.DataTypes
{

	public static class TypeTools
	{
		public static bool InheritsOrImplements(this Type child, Type parent)
		{
			parent = ResolveGenericTypeDefinition(parent);

			var currentChild = child.IsGenericType
				? child.GetGenericTypeDefinition()
				: child;

			while (currentChild != typeof(object))
			{
				if (parent == currentChild || HasAnyInterfaces(parent, currentChild))
					return true;

				currentChild = currentChild.BaseType != null && currentChild.BaseType.IsGenericType
						? currentChild.BaseType.GetGenericTypeDefinition()
						: currentChild.BaseType;

				if (currentChild == null)
					return false;
			}
			return false;
		}

		private static bool HasAnyInterfaces(Type parent, Type child)
		{
			//return child.GetInterfaces()
			//	.Any(childInterface =>
			//	{
			//		var currentInterface = childInterface.IsGenericType
			//			? childInterface.GetGenericTypeDefinition()
			//			: childInterface;
			//		return currentInterface == parent;
			//	});

			foreach (Type childInterface in child.GetInterfaces())
			{
				var currentInterface = childInterface.IsGenericType
					? childInterface.GetGenericTypeDefinition()
					: childInterface;

				if (currentInterface == parent)
					return true;
			}
			return false;
		}

		private static Type ResolveGenericTypeDefinition(Type parent)
		{
			bool shouldUseGenericType = !(parent.IsGenericType && parent.GetGenericTypeDefinition() != parent);

			if (parent.IsGenericType && shouldUseGenericType)
				parent = parent.GetGenericTypeDefinition();
			return parent;
		}

		public static string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
		{
			MemberExpression me = propertyLambda.Body as MemberExpression;
			if (me == null)
			{
				throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
			}

			return me.Member.Name;
		}

		public static string GetFullPropertyName<T>(Expression<Func<T>> propertyLambda)
		{
			MemberExpression me = propertyLambda.Body as MemberExpression;
			if (me == null)
			{
				throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
			}

			string result = string.Empty;
			do
			{
				result = me.Member.Name + "." + result;
				me = me.Expression as MemberExpression;
			} while (me != null);

			result = result.Remove(result.Length - 1); // remove the trailing "."
			return result;
		}
	}

}
