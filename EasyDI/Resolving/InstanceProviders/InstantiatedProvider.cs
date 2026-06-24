using EasyDI.Exceptions;

namespace EasyDI.Resolving.InstanceProviders;

internal sealed class InstantiatedProvider : IInstanceProvider
{
	private readonly IReadOnlyList<ArgumentInfo> _additionalArguments;

	public InstantiatedProvider(params IReadOnlyList<ArgumentInfo> additionalArguments)
	{
		_additionalArguments = additionalArguments;
	}

	public object GetInstance(IObjectResolver resolver, Type type, IReadOnlyList<Type> dependencyChain)
	{
		var resolverWithArgs = resolver.WithAdditionalArguments(_additionalArguments);
		
		if (Instantiator.TryCreateInstance(type, resolverWithArgs, dependencyChain, out object? instance))
		{
			return instance;
		}

		var diagnostics = ConstructorHelper.GetAllConstructorDiagnostics(type, resolverWithArgs);
		throw new InstantiationException(type, dependencyChain, diagnostics);
	}
}
