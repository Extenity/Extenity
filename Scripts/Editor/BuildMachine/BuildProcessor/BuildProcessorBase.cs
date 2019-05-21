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
	public abstract class BuildProcessorBase
	{
		#region Metadata

		private BuildProcessorMetadata _Metadata;
		public BuildProcessorMetadata Metadata
		{
			get
			{
				if (_Metadata == null)
				{
					var type = GetType();
					_Metadata = BuildProcessorManager.BuildProcessors.First(entry => entry.Type == type);
					Debug.Assert(_Metadata != null);
				}
				return _Metadata;
			}
		}

		#endregion
	}

}
