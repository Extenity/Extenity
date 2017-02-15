
namespace System.Collections.Generic
{

	/// <summary>
	/// Defines methods to support the comparison of objects for equality.
	/// </summary>
	/// <typeparam name="T1">The type of objects to compare.This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
	/// <typeparam name="T2">The type of objects to compare.This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
	public interface IEqualityComparer<T1, T2>
	{
		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// 
		/// <returns>
		/// true if the specified objects are equal; otherwise, false.
		/// </returns>
		/// <param name="x">The first object of type <paramref name="T1"/> to compare.</param><param name="y">The second object of type <paramref name="T2"/> to compare.</param>
		bool Equals(T1 x, T2 y);
	}

	public class EqualityComparer<T1, T2> : IEqualityComparer<T1, T2>
	{
		public static readonly IEqualityComparer<T1, T2> Default = new EqualityComparer<T1, T2>();

		public bool Equals(T1 x, T2 y)
		{
			return x.Equals(y);
		}
	}

}
