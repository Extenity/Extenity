using System;
using System.Collections.Generic;
using Extenity.ApplicationToolbox.Editor;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
using Guid = System.Guid;

namespace Extenity.DLLBuilder
{

	[Serializable]
	public class BuildRequest : IConsistencyChecker
	{
		/// <summary>
		/// Paths of projects that requested build.
		/// </summary>
		public string[] RequesterProjectChain;
		public Guid BuildJobID;

		public void AddCurrentProjectToRequesterProjectChain()
		{
			RequesterProjectChain.Add(EditorApplicationTools.UnityProjectPath);
		}

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
			if (RequesterProjectChain == null || RequesterProjectChain.Length == 0)
			{
				errors.Add(new ConsistencyError(this, "Requester project path list is empty."));
			}
			else
			{
				for (var i = 0; i < RequesterProjectChain.Length; i++)
				{
					var projectPath = RequesterProjectChain[i];
					if (string.IsNullOrEmpty(projectPath))
					{
						errors.Add(new ConsistencyError(this, "Requester project path at index '" + i + "' is empty."));
					}
					//if (!Directory.Exists(projectPath)) No need to check for this, since we don't actually use these folders other than logging purposes.
					//{
					//}
				}
			}

			if (BuildJobID == Guid.Empty)
			{
				errors.Add(new ConsistencyError(this, "Build Job ID is not specified."));
			}
		}

		public override string ToString()
		{
			return "Build Job '" + BuildJobID + "' as requested by following projects:\n" + StringTools.Serialize(RequesterProjectChain, '\n');
		}
	}

}
