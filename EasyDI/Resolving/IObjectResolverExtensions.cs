using System.Diagnostics.CodeAnalysis;
using EasyDI.Exceptions;

namespace EasyDI.Resolving;

public static class IObjectResolverExtensions
{
	public static bool CanResolveMany<T>(this IObjectResolver resolver)
	{
		return resolver.CanResolve<IReadOnlyList<T>>();
	}

	public static bool CanResolveMany(this IObjectResolver resolver, Type type)
	{
		return resolver.CanResolve(typeof(IReadOnlyList<>).MakeGenericType(type));
	}

	public static bool CanResolve<T>(this IObjectResolver resolver)
	{
		return resolver.CanResolve(ResolutionQuery.Create(typeof(T)));
	}

	public static bool CanResolve(this IObjectResolver resolver, Type type)
	{
		return resolver.CanResolve(ResolutionQuery.Create(type));
	}

	public static bool CanResolve(this IObjectResolver resolver, ResolutionQuery resolutionQuery)
	{
		return resolver.TryLazyResolve(resolutionQuery) is Success;
	}

	public static T Resolve<T>(this IObjectResolver resolver)
	{
		return (T)resolver.Resolve(typeof(T));
	}

	public static object Resolve(this IObjectResolver resolver, Type type)
	{
		return resolver.Resolve(ResolutionQuery.Create(type) with { DependencyChain = [type] });
	}

	public static object Resolve(this IObjectResolver resolver, ResolutionQuery query)
	{
		try
		{
			return resolver.TryLazyResolve(query) switch
			{
				Success success => success.InstanceGetter.Invoke(),
				Fail fail => throw new ResolutionException(query.Type, fail, resolver.GetAllResolvableTypes()),
				_ => throw new ArgumentOutOfRangeException()
			};
		}
		catch (Exception e)
		{
			throw new ResolutionException(query.Type, e);
		}
	}

	public static T? ResolveOrFallback<T>(this IObjectResolver resolver, T? fallbackValue)
	{
		return resolver.TryResolve<T>(out var instance) ? instance : fallbackValue;
	}

	public static bool TryResolve<T>(this IObjectResolver resolver, [NotNullWhen(true)] out T? instance)
	{
		if (resolver.TryLazyResolve<T>(out var lazyInstance))
		{
			try
			{
				instance = lazyInstance.Invoke()!;
				return true;
			}
			catch
			{
				// A missing transitive dependency or a throwing constructor surfaces only when the
				// graph is lazily built. Honour the Try contract and report failure instead of throwing.
			}
		}

		instance = default;
		return false;
	}

	public static bool TryResolve(this IObjectResolver resolver,
		ResolutionQuery resolutionQuery,
		[NotNullWhen(true)] out object? instance)
	{
		if (resolver.TryLazyResolve(resolutionQuery) is Success success)
		{
			try
			{
				instance = success.InstanceGetter.Invoke();
				return true;
			}
			catch
			{
				// A missing transitive dependency or a throwing constructor surfaces only when the
				// graph is lazily built. Honour the Try contract and report failure instead of throwing.
			}
		}

		instance = null;
		return false;
	}

	public static bool TryLazyResolve<T>(this IObjectResolver resolver, [NotNullWhen(true)] out Func<T>? instanceGetter)
	{
		if (resolver.TryLazyResolve(ResolutionQuery.Create(typeof(T))) is Success success)
		{
			instanceGetter = () => (T)success.InstanceGetter.Invoke();
			return true;
		}

		instanceGetter = null;
		return false;
	}
	
	public static IReadOnlyList<Type> GetAllResolvableTypes(this IObjectResolver resolver)
	{
		return
		[
			..resolver.Parent is not NullObjectResolver ? GetAllResolvableTypes(resolver.Parent) : [],
			..resolver.LocalRegistrations.SelectMany(r => r.ResolvableTypes)
		];
	}
}