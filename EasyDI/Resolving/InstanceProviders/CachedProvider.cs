namespace EasyDI.Resolving.InstanceProviders;

internal sealed class CachedProvider : IInstanceProvider
{
	private readonly IInstanceProvider _instanceProvider;
	private readonly object _lock = new();
	private volatile object? _instance;

	public CachedProvider(IInstanceProvider instanceProvider)
	{
		_instanceProvider = instanceProvider;
	}

	public object GetInstance(IObjectResolver resolver, Type type, IReadOnlyList<Type> dependencyChain)
	{
		if (_instance is not null)
		{
			return _instance;
		}

		lock (_lock)
		{
			return _instance ??= _instanceProvider.GetInstance(resolver, type, dependencyChain);
		}
	}

	public IInstanceProvider Clone()
	{
		return new CachedProvider(_instanceProvider.Clone());
	}
}