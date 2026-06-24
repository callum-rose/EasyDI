using System.Collections.Concurrent;
using System.Reflection;

namespace EasyDI.LifecycleHooks;

internal static class TypeCache
{
	public static IReadOnlyList<Type> LifecycleHookInterfaces { get; } = AppDomain.CurrentDomain.GetAssemblies()
		.SelectMany(GetLoadableTypes)
		.Where(t => t.IsInterface)
		.Where(t => typeof(ILifecycleHook).IsAssignableFrom(t))
		.Append(typeof(IDisposable))
		.ToArray();

	private static readonly ConcurrentDictionary<Type, IReadOnlyList<Type>> Interfaces = new();
	private static readonly ConcurrentDictionary<Type, IReadOnlyList<Type>> Hooks = new();

	public static IReadOnlyList<Type> GetHooksFor<T>()
	{
		return Hooks.GetOrAdd(typeof(T),
			_ => GetInterfacesFor<T>().Intersect(LifecycleHookInterfaces).ToArray());
	}

	public static IReadOnlyList<Type> GetInterfacesFor<T>()
	{
		return Interfaces.GetOrAdd(typeof(T), t => t.GetInterfaces());
	}

	private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			return ex.Types.Where(t => t is not null)!;
		}
	}
}
