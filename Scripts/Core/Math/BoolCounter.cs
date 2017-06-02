
namespace Extenity.MathToolbox
{

	public struct ActivateCounter
	{
		private int counter;

		public void Activate() { counter++; }
		public void Deactivate() { counter--; }
		public void ActivateIfDeactive() { if (counter < 1) counter++; }
		public void DeactivateIfActive() { if (counter > 0) counter--; }
		public bool IsActive { get { return counter > 0; } }

		public void SetCounterUnsafe(int value) { counter = value; }
		public void AddCounterUnsafe(int value) { counter += value; }
		public int Counter { get { return counter; } }
	}

	public struct EnableCounter
	{
		private int counter;

		public void Enable() { counter++; }
		public void Disable() { counter--; }
		public void EnableIfDisabled() { if (counter < 1) counter++; }
		public void DisableIfEnabled() { if (counter > 0) counter--; }
		public bool IsEnabled { get { return counter > 0; } }

		public void SetCounterUnsafe(int value) { counter = value; }
		public void AddCounterUnsafe(int value) { counter += value; }
		public int Counter { get { return counter; } }
	}

	public struct BoolCounter
	{
		private int counter;

		public void Increase() { counter++; }
		public void Decrease() { counter--; }
		public void IncreaseIfFalse() { if (counter < 1) counter++; }
		public void DecreaseIfTrue() { if (counter > 0) counter--; }
		public bool IsTrue { get { return counter > 0; } }

		public void SetCounterUnsafe(int value) { counter = value; }
		public void AddCounterUnsafe(int value) { counter += value; }
		public int Counter { get { return counter; } }
	}

}
