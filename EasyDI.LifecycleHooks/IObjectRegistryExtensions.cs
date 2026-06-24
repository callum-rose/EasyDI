using EasyDI.Registering;
using EasyDI.Registering.RegistrationBuilders;
using EasyDI.Resolving;

namespace EasyDI.LifecycleHooks;

public static class IObjectRegistryExtensions
{
	public static RegistrationBuilder RegisterLifecycleHook<TLifecycleHook>(this IObjectRegistry registry)
	{
		registry.EnsureLifecycleHookManagerRegistered();

		registry.MarkResolvableAsMany(TypeCache.LifecycleHookInterfaces);

		return registry.RegisterSingleton<TLifecycleHook>()
			.As(TypeCache.GetHooksFor<TLifecycleHook>());
	}

	public static RegistrationBuilder RegisterLifecycleHook<TLifecycleHook>(this IObjectRegistry registry, 
		Func<IObjectResolver, TLifecycleHook> factory)
	{
		registry.EnsureLifecycleHookManagerRegistered();

		registry.MarkResolvableAsMany(TypeCache.LifecycleHookInterfaces);

		return registry.RegisterSingleton<TLifecycleHook>(factory)
			.As(TypeCache.GetHooksFor<TLifecycleHook>());
	}

	private static void MarkResolvableAsMany(this IObjectRegistry registry, IEnumerable<Type> types)
	{
		foreach (Type type in types)
		{
			registry.MarkResolvableAsMany(type);
		}
	}

	private static void EnsureLifecycleHookManagerRegistered(this IObjectRegistry registry)
	{
		if (registry.HasRegistrationResolvableAs<ILifecycleHookManager>())
		{
			return;
		}

		registry.RegisterScoped<LifecycleHookManager>().As<ILifecycleHookManager>();
		registry.RegisterScoped<LifecycleHookHandlerFactory>();
	}
}