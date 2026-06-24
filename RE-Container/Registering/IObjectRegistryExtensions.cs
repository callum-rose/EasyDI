using REContainer.Registering.RegistrationBuilders;
using REContainer.Resolving;

namespace REContainer.Registering;

public static class IObjectRegistryExtensions
{
	public static void MarkResolvableAsMany<TResolvable>(this IObjectRegistry registry)
	{
		registry.MarkResolvableAsMany(typeof(TResolvable));
	}

	public static void RegisterBuildCallback(this IObjectRegistry registry, ResolverBuiltCallback callback)
	{
		registry.RegisterInstance(callback);
	}

	public static ExistingInstanceRegistrationBuilder RegisterInstance<TInstance>(this IObjectRegistry registry,
		TInstance instance)
	{
		return registry.RegisterInstance(typeof(TInstance), instance);
	}

	public static ExistingInstanceRegistrationBuilder RegisterInstance(this IObjectRegistry registry,
		Type type,
		object instance)
	{
		return registry.Register(new ExistingInstanceRegistrationBuilder(type, instance));
	}

	public static TypeRegistrationBuilder RegisterSingleton<TImplementation>(this IObjectRegistry registry)
	{
		return registry.Register<TImplementation>(Lifetime.Singleton);
	}

	public static TypeRegistrationBuilder RegisterSingleton(this IObjectRegistry registry, Type implementationType)
	{
		return registry.Register(implementationType, Lifetime.Singleton);
	}

	public static FactoryRegistrationBuilder RegisterSingleton<TImplementation>(this IObjectRegistry registry,
		Func<IObjectResolver, TImplementation> factory)
	{
		return registry.Register((resolver, _) => factory(resolver), Lifetime.Singleton);
	}

	public static FactoryRegistrationBuilder RegisterSingleton<TImplementation>(this IObjectRegistry registry,
		Func<IObjectResolver, Type, TImplementation> factory)
	{
		return registry.Register(factory, Lifetime.Singleton);
	}
	
	public static FactoryRegistrationBuilder RegisterSingleton(this IObjectRegistry registry,
		Type implementationType,
		Func<IObjectResolver, object> factory)
	{
		return registry.Register(implementationType, (resolver, _) => factory(resolver), Lifetime.Singleton);
	}

	public static FactoryRegistrationBuilder RegisterSingleton(this IObjectRegistry registry,
		Type implementationType,
		Func<IObjectResolver, Type, object> factory)
	{
		return registry.Register(implementationType, factory, Lifetime.Singleton);
	}

	public static TypeRegistrationBuilder RegisterScoped<TImplementation>(this IObjectRegistry registry)
	{
		return registry.Register<TImplementation>(Lifetime.Scoped);
	}

	public static TypeRegistrationBuilder RegisterScoped(this IObjectRegistry registry, Type implementationType)
	{
		return registry.Register(implementationType, Lifetime.Scoped);
	}
	
	public static FactoryRegistrationBuilder RegisterScoped<TImplementation>(this IObjectRegistry registry,
		Func<IObjectResolver, TImplementation> factory)
	{
		return registry.Register((resolver, _) => factory(resolver), Lifetime.Scoped);
	}

	public static FactoryRegistrationBuilder RegisterScoped<TImplementation>(this IObjectRegistry registry,
		Func<IObjectResolver, Type, TImplementation> factory)
	{
		return registry.Register(factory, Lifetime.Scoped);
	}
	
	public static FactoryRegistrationBuilder RegisterScoped(this IObjectRegistry registry,
		Type implementationType,
		Func<IObjectResolver, object> factory)
	{
		return registry.Register(implementationType, (resolver, _) => factory(resolver), Lifetime.Scoped);
	}

	public static FactoryRegistrationBuilder RegisterScoped(this IObjectRegistry registry,
		Type implementationType,
		Func<IObjectResolver, Type, object> factory)
	{
		return registry.Register(implementationType, factory, Lifetime.Scoped);
	}

	public static TypeRegistrationBuilder RegisterTransient<TImplementation>(this IObjectRegistry registry)
	{
		return registry.Register<TImplementation>(Lifetime.Transient);
	}

	public static TypeRegistrationBuilder RegisterTransient(this IObjectRegistry registry, Type implementationType)
	{
		return registry.Register(implementationType, Lifetime.Transient);
	}
	
	public static FactoryRegistrationBuilder RegisterTransient<TImplementation>(this IObjectRegistry registry,
		Func<IObjectResolver, TImplementation> factory)
	{
		return registry.Register((resolver, _) => factory(resolver), Lifetime.Transient);
	}

	public static FactoryRegistrationBuilder RegisterTransient<TImplementation>(this IObjectRegistry registry,
		Func<IObjectResolver, Type, TImplementation> factory)
	{
		return registry.Register(factory, Lifetime.Transient);
	}
	
	public static FactoryRegistrationBuilder RegisterTransient(this IObjectRegistry registry,
		Type implementationType,
		Func<IObjectResolver, object> factory)
	{
		return registry.Register(implementationType, (resolver, _) => factory(resolver), Lifetime.Transient);
	}

	public static FactoryRegistrationBuilder RegisterTransient(this IObjectRegistry registry,
		Type implementationType,
		Func<IObjectResolver, Type, object> factory)
	{
		return registry.Register(implementationType, factory, Lifetime.Transient);
	}

	private static TypeRegistrationBuilder Register<TImplementation>(this IObjectRegistry registry,
		Lifetime lifetime)
	{
		return registry.Register(typeof(TImplementation), lifetime);
	}

	private static TypeRegistrationBuilder Register(this IObjectRegistry registry,
		Type implementationType,
		Lifetime lifetime)
	{
		return registry.Register(new TypeRegistrationBuilder(implementationType, lifetime));
	}

	private static FactoryRegistrationBuilder Register<TImplementation>(this IObjectRegistry registry,
		Func<IObjectResolver, Type, TImplementation> factory,
		Lifetime lifetime)
	{
		return registry.Register(typeof(TImplementation), (resolver, type) => factory(resolver, type)!, lifetime);
	}

	private static FactoryRegistrationBuilder Register(this IObjectRegistry registry,
		Type implementationType,
		Func<IObjectResolver, Type, object> factory,
		Lifetime lifetime)
	{
		return registry.Register(new FactoryRegistrationBuilder(implementationType, lifetime, factory));
	}
}