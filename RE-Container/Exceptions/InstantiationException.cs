using System.Text;
using REContainer.Registering;

namespace REContainer.Exceptions;

/// <summary>
/// Exception thrown when a type cannot be instantiated.
/// Contains detailed information about available constructors and why they cannot be used.
/// </summary>
internal class InstantiationException : ContainerException
{
	public InstantiationException(
		Type targetType,
		IReadOnlyList<Type> dependencyChain,
		IReadOnlyList<ConstructorDiagnostic> constructorDiagnostics)
		: base(FormatMessage(targetType, dependencyChain, constructorDiagnostics)) { }

	public InstantiationException(Type targetType, Exception innerException) : base(
		$"Unable to instantiate type '{targetType.Format()}'. See inner exception for details.",
		innerException) { }

	private static string FormatMessage(
		Type targetType,
		IReadOnlyList<Type> dependencyChain,
		IReadOnlyList<ConstructorDiagnostic> constructorDiagnostics)
	{
		var sb = new StringBuilder();
		sb.Append($"Unable to instantiate type '{targetType.Format()}'.");

		ExceptionMessageFormatter.AppendDependencyChain(sb, dependencyChain);

		if (constructorDiagnostics.Count == 0)
		{
			sb.Append($"\nNo public constructors found on type '{targetType.Format()}'.");
		}
		else
		{
			sb.Append($"\nConstructor analysis ({constructorDiagnostics.Count} constructor(s) found):");

			foreach (var diagnostic in constructorDiagnostics)
			{
				sb.Append($"\n  - {diagnostic.Format()}");
			}
		}

		ExceptionMessageFormatter.AppendHelpSection(sb,
			"General advice:",
			"- Ensure the type has a public constructor",
			$"- Ensure all parameter types are registered or use .{nameof(RegistrationBuilderExtensions.WithArgument)}() to provide specific parameters");

		return sb.ToString();
	}
}