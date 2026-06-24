using System.Reflection;

namespace EasyDI.Exceptions;

/// <summary>
/// Diagnostic information about a constructor and why it cannot be used.
/// </summary>
internal class ConstructorDiagnostic
{
	public ConstructorInfo Constructor { get; }
	public IReadOnlyList<ParameterDiagnostic> ParameterDiagnostics { get; }
	public bool IsViable => ParameterDiagnostics.All(p => p.CanResolve);

	public ConstructorDiagnostic(ConstructorInfo constructor, IReadOnlyList<ParameterDiagnostic> parameterDiagnostics)
	{
		Constructor = constructor;
		ParameterDiagnostics = parameterDiagnostics;
	}

	public string Format()
	{
		var parameters = string.Join(", ", ParameterDiagnostics.Select(p => p.Format()));

		if (IsViable)
		{
			return $"Constructor({parameters}) - All parameters resolvable";
		}

		var unresolvableParams = ParameterDiagnostics.Where(p => !p.CanResolve).ToList();
		var reasons = string.Join(", ", unresolvableParams.Select(p => $"{FormatTypeName(p.ParameterType)}: {p.Reason}"));
		return $"Constructor({parameters}) - Cannot use: {reasons}";
	}

	private static string FormatTypeName(Type type)
	{
		if (!type.IsGenericType)
		{
			return type.Name;
		}

		var genericArgs = string.Join(", ", type.GetGenericArguments().Select(FormatTypeName));
		var typeName = type.Name[..type.Name.IndexOf('`')];
		return $"{typeName}<{genericArgs}>";
	}
}