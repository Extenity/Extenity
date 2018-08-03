namespace Extenity.DataToolbox
{

	public abstract class ChangeDetectionHelper<T>
	{
		private bool Initialized = false;

		/// <summary>
		/// Make sure this is called at the beginning.
		/// </summary>
		public void Initialize(T obj)
		{
			SetValues(obj);
			Initialized = true;
		}

		/// <summary>
		/// Override this and return true if something has changed.
		/// </summary>
		protected abstract bool OnCheckChanges(T obj);

		public bool CheckChanges(T obj)
		{
			if (!Initialized)
				return false;

			if (OnCheckChanges(obj))
			{
				SetValues(obj);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Override this when values needed to be set.
		/// </summary>
		protected abstract void OnSetValues(T obj);

		private void SetValues(T obj)
		{
			OnSetValues(obj);
		}
	}

}
