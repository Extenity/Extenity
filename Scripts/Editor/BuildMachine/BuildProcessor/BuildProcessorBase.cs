using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extenity.BuildMachine.Editor
{

	/// <summary>
	/// </summary>
	/// <remarks>
	/// Some notes on trying to do all preprocess operations inside Unity build callbacks.
	/// See 713951791.
	/// 
	/// Initial idea was doing all the preprocesses when Unity needs to build the application.
	/// That way, the user would use the regular Build button to build the application.
	/// But then, Unity is such a pain in the arse that we better not use any build callbacks
	/// to do serious modifications to assets, especially scripts.
	///
	/// Instead of processing scenes in OnProcessScene callbacks or processing other assets
	/// in OnPreprocessBuild callback, as a more cleaner approach, we process these assets
	/// just before triggering the actual Unity build. Do any AssetDatabase.Refresh operations
	/// there. Then we start Unity build with all assets ready to be built.
	/// </remarks>
	public abstract class BuildProcessorBase<TBuildProcessor>
		where TBuildProcessor : BuildProcessorBase<TBuildProcessor>
	{

	}

}
