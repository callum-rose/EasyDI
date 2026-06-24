using System.Diagnostics.CodeAnalysis;
using EasyDI.Exceptions;

namespace EasyDI.Resolving;

internal static class Instantiator
{
	public static bool TryCreateInstance(Type type,
		IObjectResolver resolver,
		IReadOnlyList<Type> dependencyChain,
		[NotNullWhen(true)] out object? instance)
	{
		if (!ConstructorHelper.GetMostRelevant(type, resolver, out var constructorInfo))
		{
			instance = null;
			return false;
		}

		var arguments = constructorInfo.GetParameters()
			.Select(p => p.ParameterType)
			.Select(t => !dependencyChain.Contains(t) ?
				resolver.Resolve(ResolutionQuery.Create(t) with { DependencyChain = [..dependencyChain, t] }) :
				throw new CircularDependencyException([..dependencyChain, t]))
			.ToArray();

		instance = constructorInfo.Invoke(arguments);
		return true;
	}
}
