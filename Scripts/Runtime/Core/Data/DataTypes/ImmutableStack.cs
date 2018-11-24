using System;
using System.Collections;
using System.Collections.Generic;

namespace Extenity.DataToolbox
{

	/// <summary>
	/// An immutable stack.
	///
	/// Note that the class is abstract with a private constructor.
	/// This ensures that only nested classes may be derived classes.
	///
	/// Source: https://gist.github.com/ericlippert/69c9e93b366f8cc5d6ac
	/// </summary>
	public abstract class ImmutableStack<T> : IEnumerable<T>
	{
		public static readonly ImmutableStack<T> Empty = new EmptyStack();

		private ImmutableStack() { }

		public abstract ImmutableStack<T> Pop();
		public abstract T Top { get; }
		public abstract bool IsEmpty { get; }

		public IEnumerator<T> GetEnumerator()
		{
			var current = this;
			while (!current.IsEmpty)
			{
				yield return current.Top;
				current = current.Pop();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public ImmutableStack<T> Push(T value)
		{
			return new NonEmptyStack(value, this);
		}

		private class EmptyStack : ImmutableStack<T>
		{
			public override ImmutableStack<T> Pop()
			{
				throw new InvalidOperationException();
			}

			public override T Top => throw new InvalidOperationException();
			public override bool IsEmpty => true;
		}

		private class NonEmptyStack : ImmutableStack<T>
		{
			private readonly T Head;
			private readonly ImmutableStack<T> Tail;

			public NonEmptyStack(T head, ImmutableStack<T> tail)
			{
				Head = head;
				Tail = tail;
			}

			public override ImmutableStack<T> Pop()
			{
				return Tail;
			}

			public override T Top => Head;
			public override bool IsEmpty => false;
		}

		public static ImmutableStack<T> Create(params T[] values)
		{
			var stack = Empty;
			if (values != null && values.Length != 0)
			{
				for (int i = 0; i < values.Length; i++)
				{
					stack = stack.Push(values[i]);
				}
			}
			return stack;
		}

		public static ImmutableStack<T> CreateInverse(params T[] values)
		{
			var stack = Empty;
			if (values != null && values.Length != 0)
			{
				for (int i = values.Length - 1; i >= 0; i--)
				{
					stack = stack.Push(values[i]);
				}
			}
			return stack;
		}
	}

}
