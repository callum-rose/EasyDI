using REContainer.Exceptions;

namespace REContainer.Resolving.InstanceProviders;

internal sealed class FactoryProvider : IInstanceProvider
{
	private readonly Func<IObjectResolver, Type, object> _factory;
	private readonly List<ArgumentInfo> _arguments;

	public FactoryProvider(Func<IObjectResolver, Type, object> factory, List<ArgumentInfo> arguments)
	{
		_factory = factory;
		_arguments = arguments;
	}

	public object GetInstance(IObjectResolver resolver, Type type, IReadOnlyList<Type> dependencyChain)
	{
		try
		{
			return _factory(resolver.WithAdditionalArguments(_arguments), type);
		}
		catch (Exception ex)
		{
			throw new FactoryException(type, dependencyChain, ex);
		}
	}
}
