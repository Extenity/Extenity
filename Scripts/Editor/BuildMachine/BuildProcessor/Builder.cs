using System.Collections;
using System.Linq;
using UnityEditor;
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
	/// just before triggering the actual Unity build. Do any <see cref="AssetDatabase.Refresh"/>
	/// operations there. Then we start Unity build with all assets ready to be built.
	/// </remarks>
	public abstract class Builder
	{
		#region Definition

		private BuildProcessorDefinition _Definition;
		public BuildProcessorDefinition Definition
		{
			get
			{
				if (!_Definition.IsValid)
				{
					var type = GetType();
					_Definition = BuilderManager.BuildProcessors.First(entry => entry.Type == type);
					Debug.Assert(_Definition.IsValid);
				}
				return _Definition;
			}
		}

		#endregion

		#region Process

		protected abstract IEnumerator Finalize(); // TODO:

		#endregion
	}

}
