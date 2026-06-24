namespace REContainer.Exceptions;

/// <summary>
/// Base exception for all container-related errors.
/// </summary>
public abstract class ContainerException : Exception
{
	protected ContainerException(string message) : base(message)
	{
	}

	protected ContainerException(string message, Exception innerException) : base(message, innerException)
	{
	}
}

