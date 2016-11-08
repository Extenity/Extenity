
namespace Extenity.SystemToolbox
{

	public delegate void InstanceAction<TInstance>(TInstance instance);
	public delegate void InstanceAction<TInstance, T1>(TInstance instance, T1 arg1);
	public delegate void InstanceAction<TInstance, T1, T2>(TInstance instance, T1 arg1, T2 arg2);
	public delegate void InstanceAction<TInstance, T1, T2, T3>(TInstance instance, T1 arg1, T2 arg2, T3 arg3);
	public delegate void InstanceAction<TInstance, T1, T2, T3, T4>(TInstance instance, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
	public delegate void InstanceAction<TInstance, T1, T2, T3, T4, T5>(TInstance instance, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
	public delegate void InstanceAction<TInstance, T1, T2, T3, T4, T5, T6>(TInstance instance, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
	public delegate void InstanceAction<TInstance, T1, T2, T3, T4, T5, T6, T7>(TInstance instance, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
	public delegate void InstanceAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8>(TInstance instance, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
	public delegate void InstanceAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9>(TInstance instance, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);

	public delegate TResult InstanceFunc<TInstance, TResult>(TInstance instance);
	public delegate TResult InstanceFunc<TInstance, T1, TResult>(TInstance instance, T1 arg1);
	public delegate TResult InstanceFunc<TInstance, T1, T2, TResult>(TInstance instance, T1 arg1, T2 arg2);
	public delegate TResult InstanceFunc<TInstance, T1, T2, T3, TResult>(TInstance instance, T1 arg1, T2 arg2, T3 arg3);
	public delegate TResult InstanceFunc<TInstance, T1, T2, T3, T4, TResult>(TInstance instance, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
	public delegate TResult InstanceFunc<TInstance, T1, T2, T3, T4, T5, TResult>(TInstance instance, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
	public delegate TResult InstanceFunc<TInstance, T1, T2, T3, T4, T5, T6, TResult>(TInstance instance, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
	public delegate TResult InstanceFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, TResult>(TInstance instance, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
	public delegate TResult InstanceFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, TResult>(TInstance instance, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
	public delegate TResult InstanceFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(TInstance instance, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);

}
