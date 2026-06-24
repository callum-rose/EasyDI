namespace REContainer.LifecycleHooks;

internal static class TypeCache
{
	public static IReadOnlyList<Type> LifecycleHookInterfaces { get; } = AppDomain.CurrentDomain.GetAssemblies()
		.SelectMany(a => a.GetTypes())
		.Where(t => t.IsInterface)
		.Where(t => typeof(ILifecycleHook).IsAssignableFrom(t))
		.Append(typeof(IDisposable))
		.ToArray();

	private static readonly Dictionary<Type, IReadOnlyList<Type>> Interfaces = new();
	private static readonly Dictionary<Type, IReadOnlyList<Type>> Hooks = new();

	public static IReadOnlyList<Type> GetHooksFor<T>()
	{
		if (Hooks.TryGetValue(typeof(T), out var hooks))
		{
			return hooks;
		}

		var hooksOfT = GetInterfacesFor<T>().Intersect(LifecycleHookInterfaces).ToArray();
		Hooks.Add(typeof(T), hooksOfT);
		
		return hooksOfT;
	}
	
	public static IReadOnlyList<Type> GetInterfacesFor<T>()
	{
		if (Interfaces.TryGetValue(typeof(T), out var interfaces))
		{
			return interfaces;
		}
		
		var interfacesOfT = typeof(T).GetInterfaces();
		Interfaces.Add(typeof(T), interfacesOfT);
		
		return interfacesOfT;
	}
}