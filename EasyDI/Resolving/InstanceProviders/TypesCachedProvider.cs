using System.Collections.Concurrent;
using System.Threading;

namespace EasyDI.Resolving.InstanceProviders;

internal class TypesCachedProvider : IInstanceProvider
{
	private readonly IInstanceProvider _instanceProvider;
	private readonly ConcurrentDictionary<Type, Lazy<object>> _instances = new();

	public TypesCachedProvider(IInstanceProvider instanceProvider)
	{
		_instanceProvider = instanceProvider;
	}

	public object GetInstance(IObjectResolver resolver, Type type, IReadOnlyList<Type> dependencyChain)
	{
		return _instances.GetOrAdd(
			type,
			t => new Lazy<object>(
				() => _instanceProvider.GetInstance(resolver, t, dependencyChain),
				LazyThreadSafetyMode.ExecutionAndPublication)).Value;
	}

	public IInstanceProvider Clone()
	{
		return new TypesCachedProvider(_instanceProvider.Clone());
	}
}