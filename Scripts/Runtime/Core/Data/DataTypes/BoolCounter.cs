using System;
using UnityEngine;

namespace Extenity.DataToolbox
{

	/// <summary>
	/// BoolCounter basically acts like a switch. Keeps track of Switch-On and Switch-Off requests and tells
	/// if Switch-On requests are made more than Switch-Off requests.
	///
	/// A regular Bool variable is used by setting it to true or false, which does not provide a way to tell
	/// how many times it was set. In contrary, BoolCounter is used by calling Increase and Decrease. That
	/// allows telling how many times the switch requests were made, and allows telling if the switch has just
	/// been turned on or off.
	/// 
	/// BoolCounter default value acts like a bool. IsTrue returns false, and IsFalse returns true.
	/// </summary>
	[Serializable]
	public class BoolCounter
	{
		/// <summary>
		/// Tells if the counter is greater than zero. This means Increase was called more times than Decrease.
		///
		/// BoolCounter default value acts like a bool. IsTrue is false when first initialized.
		/// </summary>
		public bool IsTrue => counter > 0;
		/// <summary>
		/// Tells if the counter is zero or negative. This means Increase was NOT called more times than Decrease.
		/// Does not necessarily mean Decrease called more times than Increase.
		///
		/// BoolCounter default value acts like a bool. IsFalse is true when first initialized.
		/// </summary>
		public bool IsFalse => counter <= 0;

		/// <summary>
		/// Tells if the counter is exactly 1. This may mean it has just been increased from zero, which means
		/// the bool is switched on. This may also mean the counter decreased and only one request left.
		/// 
		/// See also Increase and Decrease for getting singularity info in more reliable way, if it's used for
		/// keeping track of switching the bool on or off.
		/// </summary>
		public bool IsSingularTrue => counter == 1;
		/// <summary>
		/// Tells if the counter is exactly 0. This may mean it has just been decreased from one, which means
		/// the bool is switched off. This may also mean the counter was increased from negative to zero.
		/// 
		/// See also Increase and Decrease for getting singularity info in more reliable way, if it's used for
		/// keeping track of switching the bool on or off.
		/// </summary>
		public bool IsSingularFalse => counter == 0;

		/// <summary>
		/// The counter that is used to keep track of how many times Increase and Decrease called.
		/// 
		/// Note that the counter goes negative if Decrease called more than Increase.
		/// </summary>
		public int Counter => counter;
		[SerializeField]
		private int counter;

		/// <summary>
		/// Increases the counter and tells if the bool has just been switched on.
		/// </summary>
		public bool Increase()
		{
			var previous = counter;
			counter++;
			return previous == 0;
		}

		/// <summary>
		/// Decreases the counter and tells if the bool has just been switched off.
		/// </summary>
		public bool Decrease()
		{
			var previous = counter;
			counter--;
			return previous == 1;
		}

		public void SetCounterUnsafe(int value) { counter = value; }
		public void AddCounterUnsafe(int value) { counter += value; }
	}

}
