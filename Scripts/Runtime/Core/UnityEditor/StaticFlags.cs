using System;

namespace Extenity.UnityEditorToolbox
{

	/// <summary>
	/// Exact copy of UnityEditor.StaticEditorFlags.
	/// </summary>
	[Flags]
	public enum StaticFlags
	{
		LightmapStatic = 1,
		OccluderStatic = 2,
		OccludeeStatic = 16, // 0x00000010
		BatchingStatic = 4,
		NavigationStatic = 8,
		OffMeshLinkGeneration = 32, // 0x00000020
		ReflectionProbeStatic = 64, // 0x00000040
	}

}
