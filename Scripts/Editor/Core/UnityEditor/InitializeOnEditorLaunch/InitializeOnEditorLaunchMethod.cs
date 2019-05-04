using System;

namespace Extenity.UnityEditorToolbox.Editor
{

	/// <summary>
	/// Works just like <see cref="UnityEditor.InitializeOnLoadMethodAttribute"/>, with only the difference
	/// that it will only be called on Editor's first launch (on first assembly loading to be precise) and
	/// not the consecutive assembly reloads which happens when going into Play mode or recompiling scripts.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class InitializeOnEditorLaunchMethodAttribute : Attribute
	{
	}

}
