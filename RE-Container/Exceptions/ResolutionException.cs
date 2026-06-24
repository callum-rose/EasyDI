using System.Text;
using REContainer.Resolving;

namespace REContainer.Exceptions;

/// <summary>
/// Exception thrown when a type cannot be resolved from the container.
/// Contains detailed information about the dependency chain and available registrations.
/// </summary>
public class ResolutionException : ContainerException
{
	public ResolutionException(Type requestedType, Exception innerException) : base(
		$"Unable to resolve type '{requestedType.Format()}'. See inner exception for details.",
		innerException) { }

	public ResolutionException(
		Type requestedType,
		Fail fail,
		IReadOnlyList<Type> availableRegistrations)
		: base(FormatMessage(requestedType, fail, availableRegistrations)) { }

	private static string FormatMessage(
		Type requestedType,
		Fail fail,
		IReadOnlyList<Type> availableRegistrations)
	{
		var sb = new StringBuilder();
		sb.Append($"Unable to resolve type '{requestedType.Format()}' - {fail.Reason}.");

		ExceptionMessageFormatter.AppendDependencyChain(sb, fail.DependencyChain);

		if (availableRegistrations.Count > 0)
		{
			var registrationStr = string.Join(", ", availableRegistrations.Select(TypeExtensions.Format));
			sb.Append($"\nAvailable registrations ({availableRegistrations.Count}): {registrationStr}");
		}
		else
		{
			sb.Append("\nNo types are currently registered in the container.");
		}

		ExceptionMessageFormatter.AppendHelpSection(sb,
			$"To resolve this ensure '{requestedType.Format()}' is registered in the container using:",
			$"- IObjectRegistry.Register{{Lifetime}}<{requestedType.Format()}>()",
			$"- IObjectRegistry.Register{{Lifetime}}<{{ImplementationType}}>().As<{requestedType.Format()}>()");

		return sb.ToString();
	}
}