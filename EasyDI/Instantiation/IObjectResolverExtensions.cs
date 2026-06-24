using System.Diagnostics.CodeAnalysis;
using EasyDI.Resolving;
using EasyDI.Exceptions;

namespace EasyDI.Instantiation;

public static class IObjectResolverExtensions
{
	public static T Instantiate<T>(this IObjectResolver resolver,
		params IReadOnlyList<ArgumentInfo> additionalArguments)
	{
		return (T)resolver.Instantiate(typeof(T), additionalArguments);
	}

	public static object Instantiate(this IObjectResolver resolver,
		Type type,
		params IReadOnlyList<ArgumentInfo> additionalArguments)
	{
		if (TryInstantiateInternal(resolver,
			    type,
			    out object? instance,
			    additionalArguments,
			    out var diagnosticsGetter))
		{
			return instance;
		}

		throw new InstantiationException(type, [type], diagnosticsGetter.Invoke());
	}

	public static bool TryInstantiate<T>(this IObjectResolver resolver,
		[NotNullWhen(true)] out T? instance,
		params IReadOnlyList<ArgumentInfo> additionalArguments)
	{
		if (TryInstantiateInternal(resolver,
			    typeof(T),
			    out object? objInstance,
			    additionalArguments,
			    out _))
		{
			instance = (T)objInstance!;
			return true;
		}

		instance = default;
		return false;
	}

	public static bool TryInstantiate(this IObjectResolver resolver,
		Type type,
		[NotNullWhen(true)] out object? instance,
		params IReadOnlyList<ArgumentInfo> additionalArguments)
	{
		return TryInstantiateInternal(resolver,
			type,
			out instance,
			additionalArguments,
			out _);
	}

	private static bool TryInstantiateInternal(this IObjectResolver resolver,
		Type type,
		[NotNullWhen(true)] out object? instance,
		IReadOnlyList<ArgumentInfo> additionalArguments,
		out Func<IReadOnlyList<ConstructorDiagnostic>> diagnosticsGetter)
	{
		try
		{
			var resolverWithArgs = resolver.WithAdditionalArguments(additionalArguments);

            if (Instantiator.TryCreateInstance(type, resolverWithArgs, [type], out instance))
			{
				diagnosticsGetter = () => [];
				return true;
			}

			diagnosticsGetter = () => ConstructorHelper.GetAllConstructorDiagnostics(type, resolverWithArgs);
			return false;
		}
		catch (Exception e)
		{
			throw new InstantiationException(type, e);
		}
	}
}