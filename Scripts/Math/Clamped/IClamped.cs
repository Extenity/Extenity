using Extenity.Consistency;

public interface IClamped : IConsistencyChecker
{
	float NormalizedValue { get; }
	bool IsMinMaxValid { get; }

	string ToStringLimits();
	string ToStringWithLimits();
	string ToStringDetailed();
}
