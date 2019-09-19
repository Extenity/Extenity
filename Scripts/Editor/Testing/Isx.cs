using NUnit.Framework.Constraints;

namespace Extenity.Testing
{

	public partial class Isx
	{
		/// <summary>
		/// Returns a constraint that tests for equality with one
		/// </summary>
		public static EqualConstraint One => new EqualConstraint(1);

		/// <summary>
		/// Returns a constraint that tests for equality with one
		/// </summary>
		public static EqualConstraint MinusOne => new EqualConstraint(-1);

		/// <summary>
		/// Returns a constraint that tests for equality with one
		/// </summary>
		public static EqualConstraint Two => new EqualConstraint(2);

		/// <summary>
		/// Returns a constraint that tests for equality with one
		/// </summary>
		public static EqualConstraint MinusTwo => new EqualConstraint(-2);
	}

}
