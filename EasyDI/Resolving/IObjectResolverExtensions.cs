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
		return resolver.TryResolve<T>() is Result<T>.Success success ? success.Value : fallbackValue;
	}

	/// <summary>
	/// Attempts to resolve <typeparamref name="T"/> without throwing, returning a <see cref="Result{T}"/>
	/// that is either a <see cref="Result{T}.Success"/> with the instance or a <see cref="Result{T}.Failure"/>
	/// carrying the exception that prevented resolution (not registered, missing transitive dependency, or a
	/// throwing constructor).
	/// </summary>
	public static Result<T> TryResolve<T>(this IObjectResolver resolver)
	{
		var query = ResolutionQuery.Create(typeof(T)) with { DependencyChain = [typeof(T)] };
		return resolver.TryResolve(query) switch
		{
			Result<object>.Success success => new Result<T>.Success((T)success.Value),
			Result<object>.Failure failure => new Result<T>.Failure(failure.Exception),
			_ => new Result<T>.Failure(new ArgumentOutOfRangeException(nameof(resolver)))
		};
	}

	/// <summary>
	/// Attempts to resolve the given <paramref name="query"/> without throwing, returning a
	/// <see cref="Result{T}"/> that is either a <see cref="Result{T}.Success"/> with the instance or a
	/// <see cref="Result{T}.Failure"/> carrying the exception that prevented resolution.
	/// </summary>
	public static Result<object> TryResolve(this IObjectResolver resolver, ResolutionQuery query)
	{
		switch (resolver.TryLazyResolve(query))
		{
			case Success success:
				try
				{
					// The getter lazily builds the whole graph, so a missing transitive dependency or a
					// throwing constructor surfaces here rather than at registration time.
					return new Result<object>.Success(success.InstanceGetter.Invoke());
				}
				catch (Exception e)
				{
					return new Result<object>.Failure(new ResolutionException(query.Type, e));
				}
			case Fail fail:
				return new Result<object>.Failure(
					new ResolutionException(query.Type, fail, resolver.GetAllResolvableTypes()));
			default:
				return new Result<object>.Failure(new ArgumentOutOfRangeException(nameof(resolver)));
		}
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