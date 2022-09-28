#if ExtenityKernel

using Extenity.CodingToolbox;

[assembly: EnsuredNamespace("Extenity.KernelToolbox")]

#if UNITY_EDITOR
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Extenity.Kernel.Editor")]
#endif

#endif
