
namespace Extenity.MathToolbox
{
	
	public class BoolCounter
	{
		public bool IsTrue => counter > 0;
		public bool IsFalse => counter <= 0;
		/// <summary>
		/// Tells if the counter is 1. This may mean it has just been increased from zero. This may also mean the counter decreased and only one request left.
		/// </summary>
		public bool IsSingular => counter == 1;

		/// <summary>
		/// Note that the counter goes negative if Decrease called more than Increase.
		/// </summary>
		public int Counter => counter;
		private int counter;

		public void Increase() { counter++; }
		public void Decrease() { counter--; }
		public void IncreaseIfFalse() { if (counter < 1) counter++; }
		public void DecreaseIfTrue() { if (counter > 0) counter--; }

		public void SetCounterUnsafe(int value) { counter = value; }
		public void AddCounterUnsafe(int value) { counter += value; }
	}

}
