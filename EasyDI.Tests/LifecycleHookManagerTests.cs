using NSubstitute;
using EasyDI.LifecycleHooks;
using EasyDI.Registering;
using EasyDI.Registering.RegistrationBuilders;
using EasyDI.Resolving;
using EasyDI.Resolving.InstanceProviders;

namespace Tests;

public class LifecycleHookManagerTests
{
	public interface ITestLifecycleHook : ILifecycleHook
	{
		void Invoke();
	}

	private class TestInstanceRegistrationBuilder<T> : RegistrationBuilder
	{
		private readonly T _instance;

		public TestInstanceRegistrationBuilder(T instance) : base(typeof(T), Lifetime.Singleton)
		{
			_instance = instance;
			DeclaredCertainResolvableTypes = [typeof(T)];
		}

		protected override IInstanceProvider CreateInstanceProvider()
		{
			return new ExistingInstanceProvider(_instance!);
		}
	}

	[Test]
	public void InitialiseAll_InstantiatesAllRegisteredLifecycleHooks()
	{
		bool wasInstantiated = false;
		var registration = new FactoryRegistrationBuilder(
				typeof(ITestLifecycleHook),
				Lifetime.Singleton,
				(_, _) =>
				{
					wasInstantiated = true;
					return Substitute.For<ITestLifecycleHook>();
				})
			.As<ITestLifecycleHook>()
			.As<ILifecycleHook>()
			.Build();
		var resolver = new ObjectResolver([registration], NullObjectResolver.Instance, [typeof(ILifecycleHook)], []);
		var lifecycleHookManager = new LifecycleHookManager(new LifecycleHookHandlerFactory(resolver));

		lifecycleHookManager.InstantiateAll();

		Assert.That(wasInstantiated, Is.True);
	}

	[Test]
	public void InstantiateAll_DoesNotThrow_WhenNoHooksRegistered()
	{
		var resolver = new ObjectResolver([], NullObjectResolver.Instance, [], []);
		var lifecycleHookManager = new LifecycleHookManager(new LifecycleHookHandlerFactory(resolver));

		void Act() => lifecycleHookManager.InstantiateAll();
		
		Assert.That(Act, Throws.Nothing);
	}

	[Test]
	public void InvokeAll_WhenCalledWithLifecycleHookType_InvokesAllRegisteredLifecycleHooks()
	{
		var lifecycleHook = Substitute.For<ITestLifecycleHook>();
		var registration = new TestInstanceRegistrationBuilder<ITestLifecycleHook>(lifecycleHook).Build();
		var resolver = new ObjectResolver([registration], NullObjectResolver.Instance, [typeof(ITestLifecycleHook)], []);
		var lifecycleHookManager = new LifecycleHookManager(new LifecycleHookHandlerFactory(resolver));

		lifecycleHookManager.InvokeAll<ITestLifecycleHook>(t => t.Invoke());

		lifecycleHook.Received(1).Invoke();
	}
	
	[Test]
	public void InvokeAll_WhenCalledRepeatedlyWithDifferentActions_UsesActionFromEachCall()
	{
		var lifecycleHook = Substitute.For<ITestLifecycleHook>();
		var registration = new TestInstanceRegistrationBuilder<ITestLifecycleHook>(lifecycleHook).Build();
		var resolver = new ObjectResolver([registration], NullObjectResolver.Instance, [typeof(ITestLifecycleHook)], []);
		var lifecycleHookManager = new LifecycleHookManager(new LifecycleHookHandlerFactory(resolver));

		var firstActionInvoked = false;
		var secondActionInvoked = false;
		lifecycleHookManager.InvokeAll<ITestLifecycleHook>(_ => firstActionInvoked = true);
		lifecycleHookManager.InvokeAll<ITestLifecycleHook>(_ => secondActionInvoked = true);

		Assert.Multiple(() =>
		{
			Assert.That(firstActionInvoked, Is.True);
			Assert.That(secondActionInvoked, Is.True);
		});
	}

	[Test]
	public void InvokeAll_DoesNotThrow_WhenNoHooksRegistered()
	{
		var resolver = new ObjectResolver([], NullObjectResolver.Instance, [], []);
		var lifecycleHookManager = new LifecycleHookManager(new LifecycleHookHandlerFactory(resolver));

		void Act() => lifecycleHookManager.InvokeAll<ITestLifecycleHook>(t => t.Invoke());
		
		Assert.That(Act, Throws.Nothing);
	}

	[Test]
	public void Dispose_WhenCalled_DisposesAllRegisteredDisposables()
	{
		var disposable = Substitute.For<IDisposable>();
		var registration = new TestInstanceRegistrationBuilder<IDisposable>(disposable).Build();
		var resolver = new ObjectResolver([registration], NullObjectResolver.Instance, [typeof(IDisposable)], []);
		var lifecycleHookManager = new LifecycleHookManager(new LifecycleHookHandlerFactory(resolver));

		lifecycleHookManager.Dispose();

		disposable.Received(1).Dispose();
	}
	
	[Test]
	public void Dispose_DoesNotThrow_WhenNoHooksRegistered()
	{
		var resolver = new ObjectResolver([], NullObjectResolver.Instance, [], []);
		var lifecycleHookManager = new LifecycleHookManager(new LifecycleHookHandlerFactory(resolver));

		void Act() => lifecycleHookManager.Dispose();
		
		Assert.That(Act, Throws.Nothing);
	}
}