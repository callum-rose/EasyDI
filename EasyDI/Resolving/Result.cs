namespace EasyDI.Resolving;

/// <summary>
/// Outcome of a non-throwing resolve attempt. Either <see cref="Success"/>, carrying the resolved
/// instance, or <see cref="Failure"/>, carrying the exception that prevented resolution (e.g. the
/// type was not registered, a transitive dependency was missing, or a constructor threw).
/// </summary>
/// <remarks>
/// This is a closed union: the only two cases are <see cref="Success"/> and <see cref="Failure"/>,
/// so a <c>switch</c> over them is exhaustive.
/// </remarks>
public abstract record Result<T>
{
	// Private constructor closes the hierarchy — only the nested cases below can derive from it.
	private Result() { }

	/// <summary>A successfully resolved instance.</summary>
	public sealed record Success(T Value) : Result<T>;

	/// <summary>A failed resolve attempt, carrying the exception that caused it.</summary>
	public sealed record Failure(Exception Exception) : Result<T>;
}
