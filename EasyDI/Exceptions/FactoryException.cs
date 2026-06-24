using System.Text;

namespace EasyDI.Exceptions;

/// <summary>
/// Exception thrown when a factory function throws an exception during resolution.
/// </summary>
internal class FactoryException : ContainerException
{
	/// <summary>
	/// The type that was being created by the factory.
	/// </summary>
	public Type TargetType { get; }

	/// <summary>
	/// The chain of types that led to this factory invocation (innermost first).
	/// </summary>
	public IReadOnlyList<Type> DependencyChain { get; }

	public FactoryException(
		Type targetType,
		IReadOnlyList<Type> dependencyChain,
		Exception innerException)
		: base(FormatMessage(targetType, dependencyChain, innerException), innerException)
	{
		TargetType = targetType;
		DependencyChain = dependencyChain;
	}

	private static string FormatMessage(
		Type targetType,
		IReadOnlyList<Type> dependencyChain,
		Exception innerException)
	{
		var sb = new StringBuilder();
		sb.Append($"Factory function threw an exception while creating type '{targetType.Format()}'.");

		ExceptionMessageFormatter.AppendDependencyChain(sb, dependencyChain);

		sb.Append($"\nInner exception: {innerException.GetType().Name}: {innerException.Message}");
		
		ExceptionMessageFormatter.AppendHelpSection(sb,
			"General advice:",
			"- Check the inner exception for details on what went wrong inside the factory function",
			"- Ensure any dependencies required by the factory are registered in the container");
		return sb.ToString();
	}
}