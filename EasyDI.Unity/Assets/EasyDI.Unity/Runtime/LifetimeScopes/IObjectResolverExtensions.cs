using EasyDI.Resolving;

namespace EasyDI.Unity.LifetimeScopes
{
	public static class IObjectResolverExtensions
	{
		public static T? ResolveOrDefault<T>(this IObjectResolver resolver, T? defaultValue = default)
		{
			return resolver.TryResolve<T>() is Result<T>.Success success ? success.Value : defaultValue;
		}
	}
}