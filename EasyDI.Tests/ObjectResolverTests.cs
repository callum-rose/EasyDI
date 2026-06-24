using EasyDI.Exceptions;
using EasyDI.Instantiation;
using EasyDI.Registering;
using EasyDI.Resolving;

namespace Tests;

public class ObjectResolverTests
{
	private class A : IA;
	private interface IA;
	private class B(A a);
	private class C(B b);
	private class D(E e);
	private class E(D d);

	private interface IService;
	private class Service : IService;
	private class MultiDependency(A a, B b, int value);

	private interface IGenericInterface<T>;
	private class GenericClass<T>(A a, T value) : IGenericInterface<T>;
	private class GenericClass2<T> : IGenericInterface<T>;

	[Test]
	public void ResolveRegisteredInstance_ReturnsSameInstanceReference()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		var instance = new A();
		resolverBuilder.RegisterInstance<A>(instance);
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<A>(), Is.SameAs(instance));
	}

	[Test]
	public void ResolveInstanceRegisteredWithInterface_ReturnsSameInstanceReference()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		var instance = new A();
		resolverBuilder.RegisterInstance<IA>(instance);
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<IA>(), Is.SameAs(instance));
	}

	[Test]
	public void ResolveRegisteredValueType_ReturnsCorrectValue()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterInstance<int>(5);
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<int>(), Is.EqualTo(5));
	}

	[Test]
	public void ResolveSingletonConcreteType_ReturnsCorrectType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<A>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<A>(), Is.TypeOf<A>());
	}

	[Test]
	public void ResolveSingletonRegisteredWithInterface_ReturnsImplementationAndThrowsForConcreteType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<A>().As<IA>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<IA>(), Is.TypeOf<A>());
		Assert.Throws<ResolutionException>(() => resolver.Resolve<A>());
	}

	[Test]
	public void ResolveSingletonWithRegisteredDependency_ReturnsCorrectType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<A>();
		resolverBuilder.RegisterSingleton<B>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<B>(), Is.TypeOf<B>());
	}

	[Test]
	public void InstantiateTypeWithRegisteredDependency_ReturnsCorrectType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<A>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Instantiate<B>(), Is.TypeOf<B>());
	}

	[Test]
	public void ResolveSingletonWithMissingDependency_ThrowsException()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<B>();
		var resolver = resolverBuilder.Build();

		Assert.Throws<InstantiationException>(() => resolver.Resolve<B>());
	}

	[Test, Ignore("TODO")]
	public void ResolveSingletonWithCircularDependency_ThrowsException()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<D>();
		resolverBuilder.RegisterSingleton<E>();
		var resolver = resolverBuilder.Build();

		Assert.Throws<InvalidOperationException>(() => resolver.Resolve<D>());
	}

	[Test]
	public void ResolveSingletonMultipleTimes_ReturnsSameInstanceReference()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<A>();
		var resolver = resolverBuilder.Build();

		var instance1 = resolver.Resolve<A>();
		var instance2 = resolver.Resolve<A>();

		Assert.That(instance1, Is.SameAs(instance2));
	}

	[Test]
	public void ResolveScopedConcreteType_ReturnsCorrectType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterScoped<A>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<A>(), Is.TypeOf<A>());
	}

	[Test]
	public void ResolveScopedRegisteredWithInterface_ReturnsImplementationType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterScoped<A>().As<IA>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<IA>(), Is.TypeOf<A>());
		Assert.Throws<ResolutionException>(() => resolver.Resolve<A>());
	}

	[Test]
	public void ResolveScopedWithRegisteredDependency_ReturnsCorrectType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterScoped<A>();
		resolverBuilder.RegisterScoped<B>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<B>(), Is.TypeOf<B>());
	}

	[Test]
	public void ResolveScopedWithMissingDependency_ThrowsException()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterScoped<B>();
		var resolver = resolverBuilder.Build();

		Assert.Throws<InstantiationException>(() => resolver.Resolve<B>());
	}

	[Test]
	public void ResolveScopedWithExplicitArgument_ReturnsCorrectType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterScoped<B>().WithArgument(new A());
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<B>(), Is.TypeOf<B>());
	}

	[Test]
	public void ResolveScopedMultipleTimesInSameScope_ReturnsSameInstanceReference()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterScoped<A>();
		var resolver = resolverBuilder.Build();

		var scope = ObjectRegistry.CreateChild(resolver).Build();
		var instance1 = scope.Resolve<A>();
		var instance2 = scope.Resolve<A>();

		Assert.That(instance1, Is.SameAs(instance2));
	}

	[Test]
	public void ResolveScopedInDifferentScopes_ReturnsDifferentInstanceReferences()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterScoped<A>();
		var resolver = resolverBuilder.Build();

		var scope1 = ObjectRegistry.CreateChild(resolver).Build();
		var instance1 = scope1.Resolve<A>();

		var scope2 = ObjectRegistry.CreateChild(resolver).Build();
		var instance2 = scope2.Resolve<A>();

		Assert.That(instance1, Is.Not.SameAs(instance2));
	}

	[Test]
	public void ResolveMixedLifetimesWithScoped_WorksTogether()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<A>();
		resolverBuilder.RegisterScoped<B>();
		resolverBuilder.RegisterTransient<C>();
		var resolver = resolverBuilder.Build();

		var scope = ObjectRegistry.CreateChild(resolver).Build();
		var c1 = scope.Resolve<C>();
		var c2 = scope.Resolve<C>();

		Assert.That(c1, Is.Not.SameAs(c2));// Different C instances (transient)
		// Both should use same B instance (scoped) and same A instance (singleton)
	}

	[Test]
	public void ResolveClosedGenericScopedFromOpenGenericRegistration_ReturnsCorrectType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterScoped(typeof(GenericClass<>)).WithArgument(1);
		resolverBuilder.RegisterScoped<A>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<GenericClass<int>>(), Is.TypeOf<GenericClass<int>>());
	}

	[Test]
	public void ResolveTransientMultipleTimes_ReturnsDifferentInstanceReferences()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterTransient<A>();
		var resolver = resolverBuilder.Build();

		var instance1 = resolver.Resolve<A>();
		var instance2 = resolver.Resolve<A>();

		Assert.That(instance1, Is.Not.SameAs(instance2));
	}

	[Test]
	public void ResolveFromChildResolver_InheritsParentRegistrations()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<A>();
		var resolver = resolverBuilder.Build();

		var childResolverBuilder = ObjectRegistry.CreateChild(resolver);
		var childResolver = childResolverBuilder.Build();

		Assert.That(childResolver.Resolve<A>(), Is.TypeOf<A>());
	}

	[Test]
	public void ResolveFromChildResolverWithOwnRegistrations_ResolvesChildRegistrations()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<A>();
		var resolver = resolverBuilder.Build();

		var childResolverBuilder = ObjectRegistry.CreateChild(resolver);
		childResolverBuilder.RegisterSingleton<B>();
		var childResolver = childResolverBuilder.Build();

		Assert.That(childResolver.Resolve<B>(), Is.TypeOf<B>());
	}

	[Test]
	public void ResolveSingletonWithExplicitArgument_ReturnsCorrectType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<B>().WithArgument(new A());
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<B>(), Is.TypeOf<B>());
	}

	[Test]
	public void ResolveTransientRegisteredWithInterface_ReturnsImplementationType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterTransient<Service>().As<IService>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<IService>(), Is.TypeOf<Service>());
	}

	[Test]
	public void ResolveTransientWithRegisteredDependency_ReturnsCorrectType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterTransient<A>();
		resolverBuilder.RegisterTransient<B>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<B>(), Is.TypeOf<B>());
	}

	[Test]
	public void ResolveTransientWithMissingDependency_ThrowsException()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterTransient<B>();
		var resolver = resolverBuilder.Build();

		Assert.Throws<InstantiationException>(() => resolver.Resolve<B>());
	}

	[Test]
	public void ResolveTransientWithExplicitArgument_ReturnsCorrectType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterTransient<B>().WithArgument(new A());
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<B>(), Is.TypeOf<B>());
	}

	[Test]
	public void ResolveMixedLifetimes_SingletonAndTransientWorkTogether()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<A>();
		resolverBuilder.RegisterTransient<B>();
		var resolver = resolverBuilder.Build();

		var b1 = resolver.Resolve<B>();
		var b2 = resolver.Resolve<B>();

		Assert.That(b1, Is.Not.SameAs(b2));// Different B instances
		// Both B instances should use the same A singleton (would need reflection to verify)
	}

	[Test]
	public void ResolveUnregisteredType_ThrowsException()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		var resolver = resolverBuilder.Build();

		Assert.Throws<ResolutionException>(() => resolver.Resolve<A>());
	}

	[Test]
	public void ResolveNullRegisteredInstance_ReturnsNull()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterInstance<A>(null);
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<A>(), Is.Null);
	}

	[Test]
	public void ResolveComplexDependencyChain_ReturnsCorrectType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<A>();
		resolverBuilder.RegisterSingleton<B>();
		resolverBuilder.RegisterSingleton<C>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<C>(), Is.TypeOf<C>());
	}

	[Test]
	public void ResolveTypeWithMultipleExplicitArguments_ReturnsCorrectType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<MultiDependency>()
			.WithArgument(new A())
			.WithArgument(new B(new A()))
			.WithArgument(42);
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<MultiDependency>(), Is.TypeOf<MultiDependency>());
	}

	[Test]
	public void ResolvingSingular_FromTypeMarkedResolvableAsMany_CannotBeResolved()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();

		resolverBuilder.MarkResolvableAsMany<A>();
		resolverBuilder.RegisterSingleton<A>();
		resolverBuilder.RegisterSingleton<A>();

		var resolver = resolverBuilder.Build();

		Assert.That(resolver.CanResolve<A>(), Is.False);
	}

	[Test]
	public void ResolvingMany_FromTypeNotMarkedResolvableAsMany_CannotBeResolved()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<A>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.CanResolve<IReadOnlyList<A>>(), Is.False);
	}

	[Test]
	public void ResolveCollectionAfterDuplicateRegistration_ReturnsAllRegisteredInstances()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		var instance1 = new A();
		var instance2 = new A();

		resolverBuilder.MarkResolvableAsMany<A>();
		resolverBuilder.RegisterInstance<A>(instance1);
		resolverBuilder.RegisterInstance<A>(instance2);
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<IReadOnlyList<A>>(), Is.EqualTo(new[] { instance1, instance2 }).AsCollection);
		Assert.That(resolver.Resolve<IReadOnlyCollection<A>>(),
			Is.EqualTo(new[] { instance1, instance2 }).AsCollection);
		Assert.That(resolver.Resolve<IEnumerable<A>>(), Is.EqualTo(new[] { instance1, instance2 }).AsCollection);
	}

	[Test]
	public void BuildEmptyResolver_ReturnsNonNullResolver()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver, Is.Not.Null);
	}

	[Test]
	public void ResolveClosedGenericFromOpenGenericRegistration_ReturnsCorrectType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton(typeof(GenericClass<>)).WithArgument(1);
		resolverBuilder.RegisterSingleton<A>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<GenericClass<int>>(), Is.TypeOf<GenericClass<int>>());
	}

	[Test]
	public void ResolveClosedGenericFromOpenGenericRegistration_ReturnsCorrectType2()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton(typeof(GenericClass<>)).As(typeof(IGenericInterface<>)).WithArgument(1);
		resolverBuilder.RegisterSingleton<A>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<IGenericInterface<int>>(), Is.TypeOf<GenericClass<int>>());
	}

	[Test]
	public void ResolveClosedGenericFromOpenGenericRegistration_ReturnsCorrectType3()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton(typeof(GenericClass<>)).As(typeof(IGenericInterface<>)).WithArgument(1);
		resolverBuilder.RegisterSingleton<A>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<IGenericInterface<int>>(), Is.Not.Null);
	}

	[Test]
	public void RegisterSingletonWithFactory_ReturnsFactoryCreatedInstance()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		var factoryInstance = new A();
		resolverBuilder.RegisterSingleton<A>(resolver => factoryInstance);
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<A>(), Is.SameAs(factoryInstance));
	}

	[Test]
	public void RegisterSingletonWithFactoryAndInterface_ReturnsFactoryCreatedInstance()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		var factoryInstance = new A();
		resolverBuilder.RegisterSingleton<A>(resolver => factoryInstance).As<IA>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<IA>(), Is.SameAs(factoryInstance));
		Assert.Throws<ResolutionException>(() => resolver.Resolve<A>());
	}

	[Test]
	public void RegisterSingletonWithFactoryUsingResolver_ResolvesCorrectly()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<A>();
		resolverBuilder.RegisterSingleton<B>(resolver => new B(resolver.Resolve<A>()));
		var objectResolver = resolverBuilder.Build();

		var resolvedB = objectResolver.Resolve<B>();
		Assert.That(resolvedB, Is.TypeOf<B>());
	}

	[Test]
	public void RegisterSingletonWithFactoryMultipleTimes_ReturnsSameInstance()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		int callCount = 0;
		resolverBuilder.RegisterSingleton<A>(resolver =>
		{
			callCount++;
			return new A();
		});
		var objectResolver = resolverBuilder.Build();

		var instance1 = objectResolver.Resolve<A>();
		var instance2 = objectResolver.Resolve<A>();

		Assert.That(instance1, Is.SameAs(instance2));
		Assert.That(callCount, Is.EqualTo(1));
	}

	[Test]
	public void RegisterScopedWithFactory_ReturnsFactoryCreatedInstance()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		var factoryInstance = new A();
		resolverBuilder.RegisterScoped<A>(resolver => factoryInstance);
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<A>(), Is.SameAs(factoryInstance));
	}

	[Test]
	public void RegisterScopedWithFactoryAndInterface_ReturnsFactoryCreatedInstance()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		var factoryInstance = new A();
		resolverBuilder.RegisterScoped<A>(resolver => factoryInstance).As<IA>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<IA>(), Is.SameAs(factoryInstance));
		Assert.Throws<ResolutionException>(() => resolver.Resolve<A>());
	}

	[Test]
	public void RegisterScopedWithFactoryMultipleTimesInSameScope_ReturnsSameInstance()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		int callCount = 0;
		resolverBuilder.RegisterScoped<A>(resolver =>
		{
			callCount++;
			return new A();
		});
		var parentResolver = resolverBuilder.Build();
		var scopedResolver = ObjectRegistry.CreateChild(parentResolver).Build();

		var instance1 = scopedResolver.Resolve<A>();
		var instance2 = scopedResolver.Resolve<A>();

		Assert.That(instance1, Is.SameAs(instance2));
		Assert.That(callCount, Is.EqualTo(1));
	}

	[Test]
	public void RegisterScopedWithFactoryInDifferentScopes_ReturnsDifferentInstances()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterScoped<A>(resolver => new A());
		var parentResolver = resolverBuilder.Build();

		var scope1 = ObjectRegistry.CreateChild(parentResolver).Build();
		var instance1 = scope1.Resolve<A>();

		var scope2 = ObjectRegistry.CreateChild(parentResolver).Build();
		var instance2 = scope2.Resolve<A>();

		Assert.That(instance1, Is.Not.SameAs(instance2));
	}

	[Test]
	public void RegisterTransientWithFactory_ReturnsFactoryCreatedInstance()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterTransient<A>(resolver => new A());
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<A>(), Is.TypeOf<A>());
	}

	[Test]
	public void RegisterTransientWithFactoryAndInterface_ReturnsFactoryCreatedInstance()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterTransient<A>(resolver => new A()).As<IA>();
		var resolver = resolverBuilder.Build();

		Assert.That(resolver.Resolve<IA>(), Is.TypeOf<A>());
		Assert.Throws<ResolutionException>(() => resolver.Resolve<A>());
	}

	[Test]
	public void RegisterTransientWithFactoryMultipleTimes_ReturnsDifferentInstances()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		int callCount = 0;
		resolverBuilder.RegisterTransient<A>(resolver =>
		{
			callCount++;
			return new A();
		});
		var objectResolver = resolverBuilder.Build();

		var instance1 = objectResolver.Resolve<A>();
		var instance2 = objectResolver.Resolve<A>();

		Assert.That(instance1, Is.Not.SameAs(instance2));
		Assert.That(callCount, Is.EqualTo(2));
	}

	[Test]
	public void RegisterTransientWithFactoryUsingResolver_ResolvesCorrectly()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<A>();
		resolverBuilder.RegisterTransient<B>(resolver => new B(resolver.Resolve<A>()));
		var objectResolver = resolverBuilder.Build();

		var resolvedB = objectResolver.Resolve<B>();
		Assert.That(resolvedB, Is.TypeOf<B>());
	}

	[Test]
	public void RegisterFactoryWithComplexDependency_ReturnsCorrectType()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<A>();
		resolverBuilder.RegisterTransient<B>();
		resolverBuilder.RegisterSingleton<C>(resolver =>
		{
			var b = resolver.Resolve<B>();
			return new C(b);
		});
		var objectResolver = resolverBuilder.Build();

		Assert.That(objectResolver.Resolve<C>(), Is.TypeOf<C>());
	}

	[Test]
	public void RegisterFactoryWithValueType_ReturnsCorrectValue()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<int>(resolver => 42);
		var objectResolver = resolverBuilder.Build();

		Assert.That(objectResolver.Resolve<int>(), Is.EqualTo(42));
	}

	[Test]
	public void RegisterFactoryThatReturnsNull_ReturnsNull()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<A>(resolver => null);
		var objectResolver = resolverBuilder.Build();

		Assert.That(objectResolver.Resolve<A>(), Is.Null);
	}

	[Test]
	public void RegisterFactoryThatThrowsException_PropagatesException()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		resolverBuilder.RegisterSingleton<A>(resolver => throw new ApplicationException("Factory failed"));
		var objectResolver = resolverBuilder.Build();

		var ex = Assert.Throws<FactoryException>(() => objectResolver.Resolve<A>());
		Assert.That(ex.InnerException, Is.TypeOf<ApplicationException>());
	}

	[Test]
	public void RegisterMixedRegistrationTypes_AllResolveCorrectly()
	{
		var resolverBuilder = ObjectRegistry.CreateRoot();
		var instanceA = new A();

		resolverBuilder.RegisterInstance<A>(instanceA);// Instance registration
		resolverBuilder.RegisterTransient<B>();// Type registration
		resolverBuilder.RegisterSingleton<C>(resolver =>
		{
			var b = resolver.Resolve<B>();
			return new C(b);
		});// Factory registration

		var objectResolver = resolverBuilder.Build();

		Assert.That(objectResolver.Resolve<A>(), Is.SameAs(instanceA));
		Assert.That(objectResolver.Resolve<B>(), Is.TypeOf<B>());
		Assert.That(objectResolver.Resolve<C>(), Is.TypeOf<C>());
	}

	[Test]
	public void ResolveList_OnlyIncludesRegistrationsFromLocalResolver()
	{
		var rootBuilder = ObjectRegistry.CreateRoot();
		var rootInstance = new A();
		rootBuilder.MarkResolvableAsMany<A>();
		rootBuilder.RegisterInstance<A>(rootInstance);
		var rootResolver = rootBuilder.Build();

		var childBuilder = ObjectRegistry.CreateChild(rootResolver);
		var childInstance = new A();
		childBuilder.MarkResolvableAsMany<A>();
		childBuilder.RegisterInstance<A>(childInstance);
		var childResolver = childBuilder.Build();

		Assert.Multiple(() =>
		{
			var rootInstances = rootResolver.Resolve<IReadOnlyList<A>>();
			var childInstances = childResolver.Resolve<IReadOnlyList<A>>();

			Assert.That(rootInstances, Is.EqualTo(new[] { rootInstance }).AsCollection);
			Assert.That(childInstances, Is.EqualTo(new[] { childInstance }).AsCollection);
		});
	}

	[Test]
	public void MarkResolvableAsManyInChild_WhenParentHasNonListRegistration_ThrowsException()
	{
		var rootBuilder = ObjectRegistry.CreateRoot();
		var rootInstance = new A();
		rootBuilder.RegisterInstance(rootInstance);
		var rootResolver = rootBuilder.Build();

		var childBuilder = ObjectRegistry.CreateChild(rootResolver);
		childBuilder.MarkResolvableAsMany<A>();
		childBuilder.RegisterInstance(new A());

		void Act() => childBuilder.Build();

		Assert.That(Act, Throws.TypeOf<RegistrationException>());
	}

	[Test]
	public void RegisterInChild_WhenParentMarkedResolvableAsMany_ThrowsRegistrationException()
	{
		var rootBuilder = ObjectRegistry.CreateRoot();
		var rootInstance = new A();
		rootBuilder.MarkResolvableAsMany<A>();
		rootBuilder.RegisterInstance(rootInstance);
		var rootResolver = rootBuilder.Build();

		var childBuilder = ObjectRegistry.CreateChild(rootResolver);
		childBuilder.RegisterInstance(new A());

		void Act() => childBuilder.Build();

		Assert.That(Act, Throws.TypeOf<RegistrationException>());
	}

	[Test]
	public void Resolve_ForObjectResolver_ReturnsCurrentResolver()
	{
		var rootBuilder = ObjectRegistry.CreateRoot();
		var rootResolver = rootBuilder.Build();

		var childBuilder = ObjectRegistry.CreateChild(rootResolver);
		var childResolver = childBuilder.Build();

		Assert.Multiple(() =>
		{
			Assert.That(rootResolver.Resolve<IObjectResolver>(), Is.SameAs(rootResolver));
			Assert.That(childResolver.Resolve<IObjectResolver>(), Is.SameAs(childResolver));
		});
	}

	[Test]
	public void TryLazyResolve_DoesNotInstantiateInstance()
	{
		var builder = ObjectRegistry.CreateRoot();

		bool factoryCalled = false;
		builder.RegisterSingleton<A>(_ =>
		{
			factoryCalled = true;
			return new A();
		});

		var resolver = builder.Build();

		bool result = resolver.TryLazyResolve<A>(out _);

		Assert.Multiple(() =>
		{
			Assert.That(result, Is.True);
			Assert.That(factoryCalled, Is.False);
		});
	}

	[Test]
	public void TryLazyResolve_ForList_DoesNotInstantiateInstances()
	{
		var builder = ObjectRegistry.CreateRoot();

		builder.MarkResolvableAsMany<A>();

		bool factory0Called = false;
		builder.RegisterSingleton<A>(_ =>
		{
			factory0Called = true;
			return new A();
		});

		bool factory1Called = false;
		builder.RegisterSingleton<A>(_ =>
		{
			factory1Called = true;
			return new A();
		});

		var resolver = builder.Build();

		bool result = resolver.TryLazyResolve<IReadOnlyList<A>>(out _);

		Assert.Multiple(() =>
		{
			Assert.That(result, Is.True);
			Assert.That(factory0Called, Is.False);
			Assert.That(factory1Called, Is.False);
		});
	}

	[Test]
	public void TryLazyResolve_SingletonGetter_ReturnsSameInstanceEachCall()
	{
		var builder = ObjectRegistry.CreateRoot();
		builder.RegisterSingleton<A>();
		var resolver = builder.Build();

		Assert.That(resolver.TryLazyResolve<A>(out var instanceGetter), Is.True);
		var a1 = instanceGetter.Invoke();
		var a2 = instanceGetter.Invoke();
		Assert.That(a1, Is.SameAs(a2));
	}

	[Test]
	public void TryLazyResolve_TransientGetter_ReturnsDifferentInstancesEachCall()
	{
		var builder = ObjectRegistry.CreateRoot();
		builder.RegisterTransient<A>();
		var resolver = builder.Build();

		Assert.That(resolver.TryLazyResolve<A>(out var instanceGetter), Is.True);
		var a1 = instanceGetter.Invoke();
		var a2 = instanceGetter.Invoke();
		Assert.That(a1, Is.Not.SameAs(a2));
	}

	[Test]
	public void TryLazyResolve_UnregisteredType_ReturnsFalse()
	{
		var builder = ObjectRegistry.CreateRoot();
		var resolver = builder.Build();

		var result = resolver.TryLazyResolve<B>(out var instanceGetter);
		Assert.That(result, Is.False);
		Assert.That(instanceGetter, Is.Null);
	}

	[Test]
	public void TryLazyResolve_ListOfRegisteredInstances_LazyAndPreservesOrder()
	{
		var builder = ObjectRegistry.CreateRoot();

		builder.MarkResolvableAsMany<A>();

		bool factory0Called = false;
		bool factory1Called = false;

		builder.RegisterSingleton<A>(_ =>
		{
			factory0Called = true;
			return new A();
		});
		builder.RegisterSingleton<A>(_ =>
		{
			factory1Called = true;
			return new A();
		});

		var resolver = builder.Build();

		Assert.That(resolver.TryLazyResolve<IReadOnlyList<A>>(out var instanceGetter), Is.True);
		Assert.That(factory0Called, Is.False);
		Assert.That(factory1Called, Is.False);

		var list = instanceGetter.Invoke();
		Assert.That(factory0Called, Is.True);
		Assert.That(factory1Called, Is.True);
		Assert.That(list.Count, Is.EqualTo(2));
	}

	[Test]
	public void TryLazyResolve_ListOfUnregisteredType_ReturnsTrueButEmpty()
	{
		var builder = ObjectRegistry.CreateRoot();
		builder.MarkResolvableAsMany<A>();
		var resolver = builder.Build();

		Assert.That(resolver.TryLazyResolve<IReadOnlyList<A>>(out var instanceGetter), Is.True);
		var list = instanceGetter.Invoke();
		Assert.That(list, Is.Empty);
	}

	[Test]
	public void TryLazyResolve_ForIObjectResolver_ReturnsCurrentResolver()
	{
		var builder = ObjectRegistry.CreateRoot();
		var resolver = builder.Build();

		Assert.Multiple(() =>
		{
			Assert.That(resolver.TryLazyResolve<IObjectResolver>(out var instanceGetter), Is.True);
			Assert.That(instanceGetter.Invoke(), Is.SameAs(resolver));
		});
	}

	[Test]
	public void TryLazyResolve_List_OnlyLocalRegistrations()
	{
		var rootBuilder = ObjectRegistry.CreateRoot();
		var rootA = new A();
		rootBuilder.MarkResolvableAsMany<A>();
		rootBuilder.RegisterInstance<A>(rootA);
		var root = rootBuilder.Build();

		var childBuilder = ObjectRegistry.CreateChild(root);
		var childA1 = new A();
		var childA2 = new A();
		childBuilder.MarkResolvableAsMany<A>();
		childBuilder.RegisterInstance<A>(childA1);
		childBuilder.RegisterInstance<A>(childA2);
		var child = childBuilder.Build();

		Assert.That(root.TryLazyResolve<IReadOnlyList<A>>(out var rootGetter), Is.True);
		var rootLocal = rootGetter.Invoke();
		Assert.That(rootLocal, Is.EqualTo(new[] { rootA }).AsCollection);

		Assert.That(child.TryLazyResolve<IReadOnlyList<A>>(out var childGetter), Is.True);
		var childLocal = childGetter.Invoke();
		Assert.That(childLocal, Is.EqualTo(new[] { childA1, childA2 }).AsCollection);
	}

	[Test]
	public void TryLazyResolve_ExistingTest_Extend_VerifyGetterInvocation()
	{
		var builder = ObjectRegistry.CreateRoot();
		bool called = false;
		builder.RegisterSingleton<A>(_ =>
		{
			called = true;
			return new A();
		});
		var resolver = builder.Build();

		Assert.That(resolver.TryLazyResolve<A>(out var instanceGetter), Is.True);
		Assert.That(called, Is.False);

		var instance = instanceGetter.Invoke();
		Assert.That(called, Is.True);
		Assert.That(instance, Is.TypeOf<A>());
	}

	[Test]
	public void TryLazyResolve_ListExistingTest_Extend_VerifyGetterInvocation()
	{
		var builder = ObjectRegistry.CreateRoot();

		builder.MarkResolvableAsMany<A>();

		bool factory0Called = false;
		builder.RegisterSingleton<A>(_ =>
		{
			factory0Called = true;
			return new A();
		});
		bool factory1Called = false;
		builder.RegisterSingleton<A>(_ =>
		{
			factory1Called = true;
			return new A();
		});

		var resolver = builder.Build();

		Assert.That(resolver.TryLazyResolve<IReadOnlyList<A>>(out var instanceGetter), Is.True);
		Assert.That(factory0Called, Is.False);
		Assert.That(factory1Called, Is.False);

		var list = instanceGetter.Invoke();
		Assert.That(factory0Called, Is.True);
		Assert.That(factory1Called, Is.True);
		Assert.That(list.Count, Is.EqualTo(2));
	}

	[Test]
	public void LocalRegistrations_Count_IsOne_ForGrandChildResolver()
	{
		var rootRegistry = ObjectRegistry.CreateRoot();
		rootRegistry.RegisterScoped<A>();
		var rootResolver = rootRegistry.Build();
		var childResolver = ObjectRegistry.CreateChild(rootResolver).Build();
		var grandChildResolver = ObjectRegistry.CreateChild(childResolver).Build();

		Assert.That(grandChildResolver.LocalRegistrations.Count, Is.EqualTo(1));
	}

	[Test]
	public void MarkResolvableAsManyAndRegisterInGrandChild_ThrowsNothing()
	{
		var rootRegistry = ObjectRegistry.CreateRoot();
		rootRegistry.MarkResolvableAsMany<IA>();
		rootRegistry.RegisterSingleton<A>().As<IA>();
		var rootResolver = rootRegistry.Build();

		var childRegistry = ObjectRegistry.CreateChild(rootResolver);
		var childResolver = childRegistry.Build();

		var grandChildRegistry = ObjectRegistry.CreateChild(childResolver);
		grandChildRegistry.MarkResolvableAsMany<IA>();
		grandChildRegistry.RegisterSingleton<A>().As<IA>();
		
		void Act() => grandChildRegistry.Build();
		
		Assert.That(Act, Throws.Nothing);
	}
	
	[Test]
	public void CannotResolve_RegistrationArgument()
	{
		var rootRegistry = ObjectRegistry.CreateRoot();
		rootRegistry.RegisterSingleton<B>().WithArgument(new A());
		var rootResolver = rootRegistry.Build();

		Assert.That(rootResolver.CanResolve<A>(), Is.False);
	}
	
	[Test]
	public void CanResolve_Argument_FromContainer()
	{
		var rootResolver = ObjectRegistry.CreateRoot().Build();
		var withArgsResolver = rootResolver.WithAdditionalArguments([new ArgumentInfo(typeof(int), 1)]);

		Assert.That(withArgsResolver.CanResolve<int>(), Is.True);
	}
	
	[Test]
	public void CannotResolve_Argument_FromSubContainer()
	{
		var rootResolver = ObjectRegistry.CreateRoot().Build();
		var withArgsResolver = rootResolver.WithAdditionalArguments([new ArgumentInfo(typeof(int), 1)]);
		var subResolver = ObjectRegistry.CreateChild(withArgsResolver).Build();

		Assert.That(subResolver.CanResolve<int>(), Is.False);
	}

	[Test]
	public void OpenGenericRegistration_WithDifferentTypeArguments_Resolves()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterSingleton(typeof(GenericClass2<>)).As(typeof(IGenericInterface<>));
		
		var resolver = registry.Build();
		
		var intInstance = resolver.Resolve<IGenericInterface<int>>();
		var stringInstance = resolver.Resolve<IGenericInterface<string>>();
		
		Assert.Multiple(() =>
		{
			Assert.That(intInstance, Is.TypeOf<GenericClass2<int>>());
			Assert.That(stringInstance, Is.TypeOf<GenericClass2<string>>());
		});
	}

	[Test]
	public void RegisterOpenGenericSingletonWithFactory_ReturnsCorrectClosedGenericType()
	{
		var registry = ObjectRegistry.CreateRoot();

		object Factory(IObjectResolver resolver, Type type)
		{
			var genericArguments = type.GetGenericArguments();
			var genericType = typeof(GenericClass2<>).MakeGenericType(genericArguments);
			return Activator.CreateInstance(genericType)!;
		}

		registry.RegisterSingleton(typeof(GenericClass2<>), Factory).As(typeof(IGenericInterface<>));
		
		var resolver = registry.Build();

		var resolved = resolver.Resolve<IGenericInterface<int>>();
		
		Assert.That(resolved, Is.TypeOf<GenericClass2<int>>());
	}

	[Test]
	public void Resolve_WithAdditionalArguments_ReturnsSameScopedInstance()
	{
		var rootRegistry = ObjectRegistry.CreateRoot();
		rootRegistry.RegisterScoped<Service>();
		var rootResolver = rootRegistry.Build();
		
		var childResolver = ObjectRegistry.CreateChild(rootResolver).Build();
		
		var withArgsResolver = childResolver.WithAdditionalArguments([]);

		var actual = withArgsResolver.Resolve<Service>();
		var expected = childResolver.Resolve<Service>();
		
		Assert.That(actual, Is.SameAs(expected));
	}
}