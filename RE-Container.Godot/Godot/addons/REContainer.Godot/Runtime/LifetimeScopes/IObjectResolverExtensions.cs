using REContainer.Resolving;

namespace REContainer.Godot.LifetimeScopes;

public static class IObjectResolverExtensions
{
	public static T? ResolveOrDefault<T>(this IObjectResolver resolver, T? defaultValue = default)
	{
		return resolver.TryResolve<T>(out var instance) ? instance : defaultValue;
	}
}