using System.Text;

namespace EasyDI.Exceptions;

/// <summary>
/// Exception thrown when a circular dependency is detected during resolution.
/// Without this guard the resolver would recurse until the stack overflows,
/// and <see cref="System.StackOverflowException"/> cannot be caught.
/// </summary>
public class CircularDependencyException : ContainerException
{
	/// <summary>
	/// The chain of types that led back to an already-resolving type, ending with the repeated type.
	/// </summary>
	public IReadOnlyList<Type> DependencyChain { get; }

	public CircularDependencyException(IReadOnlyList<Type> dependencyChain)
		: base(FormatMessage(dependencyChain))
	{
		DependencyChain = dependencyChain;
	}

	private static string FormatMessage(IReadOnlyList<Type> dependencyChain)
	{
		var sb = new StringBuilder();

		Type repeatedType = dependencyChain[^1];
		sb.Append($"Circular dependency detected while resolving '{repeatedType.Format()}'.");

		ExceptionMessageFormatter.AppendDependencyChain(sb, dependencyChain);

		ExceptionMessageFormatter.AppendHelpSection(sb,
			"To resolve this break the cycle, for example by:",
			"- Removing one of the dependencies",
			"- Injecting a factory (Func<T>) or IObjectResolver and resolving lazily instead");

		return sb.ToString();
	}
}
