using System.Collections.Concurrent;

namespace EasyDI.Resolving.InstanceProviders;

internal class TypesCachedProvider : IInstanceProvider
{
	private readonly IInstanceProvider _instanceProvider;
	private readonly ConcurrentDictionary<Type, object> _instances = new();

	public TypesCachedProvider(IInstanceProvider instanceProvider)
	{
		_instanceProvider = instanceProvider;
	}

	public object GetInstance(IObjectResolver resolver, Type type, IReadOnlyList<Type> dependencyChain)
	{
		return _instances.GetOrAdd(type, t => _instanceProvider.GetInstance(resolver, t, dependencyChain));
	}

	public IInstanceProvider Clone()
	{
		return new TypesCachedProvider(_instanceProvider.Clone());
	}
}