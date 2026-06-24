namespace REContainer.Resolving.InstanceProviders;

internal sealed class CachedProvider : IInstanceProvider
{
	private readonly IInstanceProvider _instanceProvider;
	private object? _instance;

	public CachedProvider(IInstanceProvider instanceProvider)
	{
		_instanceProvider = instanceProvider;
	}

	public object GetInstance(IObjectResolver resolver, Type type, IReadOnlyList<Type> dependencyChain)
	{
		return _instance ??= _instanceProvider.GetInstance(resolver, type, dependencyChain);
	}

	public IInstanceProvider Clone()
	{
		return new CachedProvider(_instanceProvider.Clone());
	}
}