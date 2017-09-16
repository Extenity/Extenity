
namespace Extenity.DLLBuilder
{

	public enum ObfuscateResult
	{
		Unspecified,
		Failed,
		Succeeded,
		Skipped,
	}

	public abstract class ObfuscationJob
	{
		public abstract bool Finished { get; }

		public ObfuscateResult RuntimeDLLSucceeded;
		public ObfuscateResult EditorDLLSucceeded;
	}

}
