using Extenity.CodingToolbox;

[assembly: EnsuredNamespace("Extenity.DebugFlow")]

#if UNITY_EDITOR
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Extenity.DebugFlow.Editor")]
#endif
