using System;
using Extenity.ApplicationToolbox;
using Extenity.ConsistencyToolbox;

namespace Extenity.UnityProjectTemplateToolbox.Editor
{

	[Serializable]
	public class TemplateMetadata : IConsistencyChecker
	{
		public string name = "com.yourcompany.template.yourtemplatename";
		public string displayName = "Test Template";
		public string version = "1.0.0";

		// Note that some fields below are commented out but not removed. These are the ones that exist in
		// Unity Hub Project templates. Keep them for future needs.

		// public string type = "template";
		// public string host = "hub";
		// public string unity = "2020.1";
		// public string description = "A brief description that will be displayed when creating a new project from this template.";

		// Not sure how to handle dependencies field. It's key names are dynamic, which makes things difficult.
		// Example:
		// "dependencies": {
		// 	"com.unity.2d.animation": "4.2.4",
		// 	"com.unity.2d.pixel-perfect": "3.0.2",
		// 	"com.unity.2d.psdimporter": "3.1.4",
		// 	"com.unity.2d.sprite": "1.0.0",
		// 	"com.unity.2d.spriteshape": "4.1.1",
		// 	"com.unity.2d.tilemap": "1.0.0"
		// },
		//
		// public ... dependencies;

		public void CheckConsistency(ConsistencyChecker checker)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				checker.AddError("Name was not specified.");
			}
			foreach (var c in name)
			{
				if (!(c >= 'a' && c <= 'z') && !(c >= '0' && c <= '9') && (c != '.'))
				{
					checker.AddError($"Name contains invalid character: '{c}'");
				}
			}

			try
			{
				new ApplicationVersion(version);
			}
			catch
			{
				checker.AddError($"Failed to parse version '{version}'");
			}
		}
	}

}
