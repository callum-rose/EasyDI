namespace EasyDI.Exceptions;

internal class ResolverBuildCallbackException : ContainerException
{
	public ResolverBuildCallbackException(string message) : base(message) { }
	public ResolverBuildCallbackException(string message, Exception innerException) : base(message, innerException) { }
}