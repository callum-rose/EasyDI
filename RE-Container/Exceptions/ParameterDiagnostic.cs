namespace REContainer.Exceptions;

/// <summary>
/// Diagnostic information about a constructor parameter.
/// </summary>
internal class ParameterDiagnostic
{
	public Type ParameterType { get; }
	public bool CanResolve { get; }
	public string? Reason { get; }

	public ParameterDiagnostic(Type parameterType, bool canResolve, string? reason = null)
	{
		ParameterType = parameterType;
		CanResolve = canResolve;
		Reason = reason;
	}

	public string Format()
	{
		var typeName = ParameterType.Format();
		return CanResolve ? typeName : $"{typeName} ✗";
	}
}