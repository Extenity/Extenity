namespace Extenity.KernelToolbox
{

	// TODO: Find a way to prevent KernelObject to be instantiated via 'new'.

	public abstract class KernelObject
	{
		#region ID

		public ID ID;

		#endregion
	}

	public abstract class KernelObject<TKernel> : KernelObject
		where TKernel : KernelBase
	{
		#region Kernel Link

		private TKernel _Kernel;
		public TKernel Kernel
		{
			get
			{
				if (_Kernel == null)
				{
					_Kernel = (TKernel)KernelBase._TempInstance;
				}
				return _Kernel;
			}
		}

		#endregion

		#region Invalidate

		public void Invalidate()
		{
			Kernel.Invalidate(ID);
		}

		#endregion
	}

}
