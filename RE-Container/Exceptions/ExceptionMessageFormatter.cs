using System.Text;

namespace REContainer.Exceptions;

/// <summary>
/// Helper class for formatting consistent exception messages.
/// </summary>
internal static class ExceptionMessageFormatter
{
	/// <summary>
	/// Appends a formatted dependency chain to the message if it contains multiple types.
	/// </summary>
	/// <param name="sb">The StringBuilder to append to.</param>
	/// <param name="dependencyChain">The chain of dependent types.</param>
	public static void AppendDependencyChain(StringBuilder sb, IReadOnlyList<Type> dependencyChain)
	{
		if (dependencyChain.Count > 1)
		{
			sb.Append("\nDependency chain:\n  ");
			sb.Append(string.Join(" -> ", dependencyChain.Select(TypeExtensions.Format)));
		}
	}

	/// <summary>
	/// Appends a help section with advice for resolving the issue.
	/// </summary>
	/// <param name="sb">The StringBuilder to append to.</param>
	/// <param name="advice">Lines of advice to display.</param>
	public static void AppendHelpSection(StringBuilder sb, params string[] advice)
	{
		if (advice.Length == 0)
		{
			return;
		}

		sb.AppendLine();
		sb.AppendLine();

		foreach (var line in advice)
		{
			sb.Append("  ");
			sb.AppendLine(line);
		}
	}
}

