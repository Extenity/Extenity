using Extenity.CodingToolbox;

[assembly: EnsuredNamespace("Extenity")]

#if UNITY_EDITOR
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Extenity.Core.Editor")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Extenity.Tests.Editor")]
#endif
