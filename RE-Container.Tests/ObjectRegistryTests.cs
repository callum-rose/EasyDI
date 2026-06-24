using NSubstitute;
using REContainer.Registering;
using REContainer.Registering.RegistrationBuilders;
using REContainer.Resolving;
using REContainer.Resolving.InstanceProviders;
using REContainer.Exceptions;

namespace Tests;

public class ObjectRegistryTests
{
	private class TestRegistrationBuilder : RegistrationBuilder
	{
		public TestRegistrationBuilder(Type implementationType, Lifetime lifetime) :
			base(implementationType, lifetime) { }

		protected override IInstanceProvider CreateInstanceProvider()
		{
			return Substitute.For<IInstanceProvider>();
		}
	}

	private interface ITestService;

	private class TestServiceA : ITestService, IDisposable
	{
		public void Dispose() { }
	}

	private class TestServiceB : ITestService, IDisposable
	{
		public void Dispose() { }
	}

	private class TestServiceC(TestServiceA A);
	private class TestServiceD(TestServiceA A);

	[Test]
	public void Build_CreateRoot_SetsParentToNullObjectResolver()
	{
		var resolver = ObjectRegistry.CreateRoot().Build();

		Assert.That(resolver.Parent, Is.TypeOf<NullObjectResolver>());
	}

	[Test]
	public void Build_CreateChild_SetsParentToProvidedResolver()
	{
		var parentResolver = ObjectRegistry.CreateRoot().Build();
		var childResolver = ObjectRegistry.CreateChild(parentResolver).Build();

		Assert.That(childResolver.Parent, Is.SameAs(parentResolver));
	}

	[Test]
	public void WithAdditionalArguments_CopiesParentAndRegistrations()
	{
		var parentRegistry = ObjectRegistry.CreateRoot();
		parentRegistry.RegisterInstance<int>(5);
		var parentResolver = parentRegistry.Build();

		var childRegistry = ObjectRegistry.CreateChild(parentResolver);
		childRegistry.RegisterInstance<string>("Hello");
		childRegistry.RegisterTransient<TestServiceA>().As<ITestService>();
		var childResolver = childRegistry.Build();

		var copiedResolver = childResolver.WithAdditionalArguments([]);

		Assert.That(copiedResolver.Resolve<int>(), Is.EqualTo(5));
		Assert.That(copiedResolver.Resolve<string>(), Is.EqualTo("Hello"));
		Assert.That(copiedResolver.CanResolve<ITestService>(), Is.True);
	}

	[Test]
	public void ArgumentsGiven_WithAdditionalArguments_AreResolvable()
	{
		var registry = ObjectRegistry.CreateRoot();
		var resolver = registry.Build();

		var newResolver = resolver.WithAdditionalArguments(["Hello".ToArgumentInfo(), 5.ToArgumentInfo()]);

		Assert.That(newResolver.Resolve<string>(), Is.EqualTo("Hello"));
		Assert.That(newResolver.Resolve<int>(), Is.EqualTo(5));
	}

	[Test]
	public void ArgumentsWithSameTypeAsRegistration_WithAdditionalArguments_Throws()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterInstance<string>("Hello");
		var resolver = registry.Build();

		void Act() => resolver.WithAdditionalArguments(["Goodbye".ToArgumentInfo()]);

		Assert.That(Act, Throws.Exception);
	}

	[Test]
	public void TwoArgumentsWithSameType_WithAdditionalArguments_Throws()
	{
		var registry = ObjectRegistry.CreateRoot();
		var resolver = registry.Build();

		void Act() => resolver.WithAdditionalArguments(["Goodbye".ToArgumentInfo(), "Goodbye".ToArgumentInfo()]);

		Assert.That(Act, Throws.Exception);
	}

	[Test]
	public void WithAdditionalArguments_DoesNotInvokeCopiedBuildCallbacks()
	{
		var registry = ObjectRegistry.CreateRoot();
		bool buildCallbackInvoked = false;
		registry.RegisterBuildCallback(_ => buildCallbackInvoked = true);
		var resolver = registry.Build();

		buildCallbackInvoked = false;
		resolver.WithAdditionalArguments([]);

		Assert.That(buildCallbackInvoked, Is.False);
	}

	[Test]
	public void Build_WithMultipleRegistrations_ContainsAllRegistrations()
	{
		var registry = ObjectRegistry.CreateRoot();

		var simpleRegistration = new TestRegistrationBuilder(typeof(object), Lifetime.Singleton);
		var complexRegistration = new TestRegistrationBuilder(typeof(TestServiceA), Lifetime.Transient)
		{
			DeclaredCertainResolvableTypes = [typeof(ITestService), typeof(IDisposable)]
		};

		registry.Register(simpleRegistration);
		registry.Register(complexRegistration);

		var resolver = registry.Build();

		Assert.That(resolver.LocalRegistrations, Has.Count.EqualTo(2));
		Assert.That(resolver.LocalRegistrations,
			Does.Contain(simpleRegistration.Build())
				.Using((Registration element, Registration expected) => Compare(element, expected)));
		Assert.That(resolver.LocalRegistrations,
			Does.Contain(complexRegistration.Build())
				.Using((Registration element, Registration expected) => Compare(element, expected)));

		bool Compare(Registration a, Registration b) =>
			a.ImplementationType == b.ImplementationType &&
			a.Lifetime == b.Lifetime &&
			a.ResolvableTypes.SequenceEqual(b.ResolvableTypes);
	}

	[Test]
	public void Build_Throws_WhenDuplicateSingletonRegistered()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterSingleton<TestServiceA>();
		registry.RegisterSingleton<TestServiceA>();

		void Act() => registry.Build();

		Assert.That(Act, Throws.TypeOf<RegistrationException>());
	}

	[Test]
	public void Build_DoesNotThrow_WhenDuplicateSingletonRegisteredAndMarkedAsMany()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.MarkResolvableAsMany<TestServiceA>();
		registry.RegisterSingleton<TestServiceA>();
		registry.RegisterSingleton<TestServiceA>();

		void Act() => registry.Build();

		Assert.That(Act, Throws.Nothing);
	}

	[Test]
	public void Build_Throws_WhenChildRegistryDuplicatesParentRegistration()
	{
		var rootRegistry = ObjectRegistry.CreateRoot();
		rootRegistry.RegisterSingleton<TestServiceA>();
		var rootResolver = rootRegistry.Build();

		var childRegistry = ObjectRegistry.CreateChild(rootResolver);
		childRegistry.RegisterSingleton<TestServiceA>();

		void Act() => childRegistry.Build();

		Assert.That(Act, Throws.TypeOf<RegistrationException>());
	}

	[Test]
	public void Build_Throws_WhenChildRegistryOverridesParentRegistration()
	{
		var rootRegistry = ObjectRegistry.CreateRoot();
		rootRegistry.RegisterSingleton<TestServiceA>();
		var rootResolver = rootRegistry.Build();

		var childRegistry = ObjectRegistry.CreateChild(rootResolver);
		childRegistry.RegisterSingleton<TestServiceA>();

		void Act() => childRegistry.Build();

		Assert.That(Act, Throws.Exception);
	}

	[Test]
	public void Build_InvokesAllRegisteredBuildCallbacks()
	{
		var registry = ObjectRegistry.CreateRoot();
		var callback0 = Substitute.For<ResolverBuiltCallback>();
		var callback1 = Substitute.For<ResolverBuiltCallback>();
		registry.RegisterBuildCallback(callback0);
		registry.RegisterBuildCallback(callback1);

		var resolver = registry.Build();

		callback0.Received(1).Invoke(Arg.Is(resolver));
		callback1.Received(1).Invoke(Arg.Is(resolver));
	}

	[Test]
	public void Build_InvokesBuildCallbacksOncePerResolverInHierarchy()
	{
		var rootRegistry = ObjectRegistry.CreateRoot();
		var rootCallback = Substitute.For<ResolverBuiltCallback>();
		rootRegistry.RegisterBuildCallback(rootCallback);
		var rootResolver = rootRegistry.Build();

		var childRegistry = ObjectRegistry.CreateChild(rootResolver);
		var childCallback = Substitute.For<ResolverBuiltCallback>();
		childRegistry.RegisterBuildCallback(childCallback);
		var childResolver = childRegistry.Build();

		rootCallback.Received(1).Invoke(Arg.Is(rootResolver));
		childCallback.Received(1).Invoke(Arg.Is(childResolver));
	}

	[Test]
	public void Build_Throws_WhenDuplicateImplementationTypeRegistered()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterSingleton<TestServiceA>();
		registry.RegisterSingleton<TestServiceA>();

		void Act() => registry.Build();

		Assert.That(Act, Throws.TypeOf<RegistrationException>());
	}

	[Test]
	public void Build_Throws_WhenDuplicateResolvableTypeRegistered()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterSingleton<TestServiceA>().As<ITestService>();
		registry.RegisterSingleton<TestServiceB>().As<ITestService>();

		void Act() => registry.Build();

		Assert.That(Act, Throws.TypeOf<RegistrationException>());
	}

	[Test]
	public void Build_DoesNotThrow_WhenDuplicateResolvableTypeMarkedAsMany()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.MarkResolvableAsMany<ITestService>();
		registry.RegisterSingleton<TestServiceA>().As<ITestService>();
		registry.RegisterSingleton<TestServiceB>().As<ITestService>();

		void Act() => registry.Build();

		Assert.That(Act, Throws.Nothing);
	}

	[Test]
	public void Build_DoesNotThrow_WhenDuplicateArgumentTypeUsedInDifferentRegistrations()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.MarkResolvableAsMany<ITestService>();
		registry.RegisterSingleton<TestServiceC>().WithArgument(new TestServiceA());
		registry.RegisterSingleton<TestServiceD>().WithArgument(new TestServiceA());

		void Act() => registry.Build();

		Assert.That(Act, Throws.Nothing);
	}

	public class A;
	public interface IB;
	public interface IC;
	public class B : A, IB;

	[Test]
	public void Build_WithTryAsOnUnrelatedInterface_DoesNotRegisterType()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterSingleton<B>().TryAs<IC>();

		var resolver = registry.Build();

		Assert.That(resolver.CanResolve<IC>(), Is.False);
	}

	[Test]
	public void Build_WithTryAsOnImplementedInterface_RegistersType()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterSingleton<B>().TryAs<IB>();

		var resolver = registry.Build();

		Assert.That(resolver.CanResolve<IB>(), Is.True);
	}

	[Test]
	public void Build_WithTryAsOnInstanceOfImplementedInterface_RegistersType()
	{
		var registry = ObjectRegistry.CreateRoot();
		A instance = new B();
		registry.RegisterInstance<A>(instance).TryAs<IB>();

		var resolver = registry.Build();

		Assert.That(resolver.CanResolve<IB>(), Is.True);
	}

	[Test]
	public void Build_WithTryAsOnInstanceOfUnrelatedInterface_DoesNotRegisterType()
	{
		var registry = ObjectRegistry.CreateRoot();
		A instance = new B();
		registry.RegisterInstance<A>(instance).TryAs<IC>();

		var resolver = registry.Build();

		Assert.That(resolver.CanResolve<IC>(), Is.False);
	}

	private class One;
	private class Two(One one);
	private class Three(One one, Two two);

	[Test]
	public void ResolvingService_WithSharedArgumentAcrossDependentDependencies_ResolvesSuccessfully()
	{
		var registry = ObjectRegistry.CreateRoot();
		var serviceA = new One();

		registry.RegisterSingleton<Two>().WithArgument(serviceA);
		registry.RegisterSingleton<Three>().WithArgument(serviceA);

		var resolver = registry.Build();

		void Act() => resolver.Resolve<Three>();

		Assert.That(Act, Throws.Nothing);
	}
}