using REContainer.LifecycleHooks;
using REContainer.Registering;
using REContainer.Resolving;

namespace Tests;

public class LifecycleHookIntegrationTests
{
	public interface ITestLifecycleHook : ILifecycleHook
	{
		public void Invoke();
	}

	public sealed class TestLifecycleHookA : ITestLifecycleHook, IDisposable
	{
		public static int InstantiatedCount { get; set; }
		public static int InvokeCount { get; set; }
		public static int DisposeCount { get; set; }

		public TestLifecycleHookA()
		{
			InstantiatedCount++;
		}

		public void Invoke()
		{
			InvokeCount++;
		}

		public void Dispose()
		{
			DisposeCount++;
		}
	}

	public sealed class TestLifecycleHookB : ITestLifecycleHook, IDisposable
	{
		public static int InstantiatedCount { get; set; }
		public static int InvokeCount { get; set; }
		public static int DisposeCount { get; set; }

		public TestLifecycleHookB()
		{
			InstantiatedCount++;
		}

		public void Invoke()
		{
			InvokeCount++;
		}

		public void Dispose()
		{
			DisposeCount++;
		}
	}

	[TearDown]
	public void TearDown()
	{
		TestLifecycleHookA.InstantiatedCount = 0;
		TestLifecycleHookA.InvokeCount = 0;
		TestLifecycleHookA.DisposeCount = 0;

		TestLifecycleHookB.InstantiatedCount = 0;
		TestLifecycleHookB.InvokeCount = 0;
		TestLifecycleHookB.DisposeCount = 0;
	}

	[Test]
	public void RegisteredLifecycleHooks_InstantiatedCorrectly()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterLifecycleHook<TestLifecycleHookA>();
		registry.RegisterLifecycleHook<TestLifecycleHookB>();
		var resolver = registry.Build();

		var lifecycleHookManager = resolver.Resolve<ILifecycleHookManager>();

		lifecycleHookManager.InstantiateAll();

		Assert.Multiple(() =>
		{
			Assert.That(TestLifecycleHookA.InstantiatedCount, Is.EqualTo(1));
			Assert.That(TestLifecycleHookB.InstantiatedCount, Is.EqualTo(1));

			Assert.That(TestLifecycleHookA.InvokeCount, Is.EqualTo(0));
			Assert.That(TestLifecycleHookB.InvokeCount, Is.EqualTo(0));
		});
	}

	[Test]
	public void RegisteredLifecycleHooks_InvokedCorrectly()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterLifecycleHook<TestLifecycleHookA>();
		registry.RegisterLifecycleHook<TestLifecycleHookB>();
		var resolver = registry.Build();

		var lifecycleHookManager = resolver.Resolve<ILifecycleHookManager>();

		lifecycleHookManager.InstantiateAll();

		lifecycleHookManager.InvokeAll<ITestLifecycleHook>(t => t.Invoke());

		Assert.Multiple(() =>
		{
			Assert.That(TestLifecycleHookA.InstantiatedCount, Is.EqualTo(1));
			Assert.That(TestLifecycleHookB.InstantiatedCount, Is.EqualTo(1));
			Assert.That(TestLifecycleHookA.InvokeCount, Is.EqualTo(1));
			Assert.That(TestLifecycleHookB.InvokeCount, Is.EqualTo(1));
		});
	}

	[Test]
	public void RegisteredLifecycleHooks_DisposedCorrectly()
	{
		var registry = ObjectRegistry.CreateRoot();
		registry.RegisterLifecycleHook<TestLifecycleHookA>();
		registry.RegisterLifecycleHook<TestLifecycleHookB>();
		var resolver = registry.Build();

		var lifecycleHookManager = resolver.Resolve<ILifecycleHookManager>();

		lifecycleHookManager.InstantiateAll();

		lifecycleHookManager.Dispose();

		Assert.Multiple(() =>
		{
			Assert.That(TestLifecycleHookA.InstantiatedCount, Is.EqualTo(1));
			Assert.That(TestLifecycleHookB.InstantiatedCount, Is.EqualTo(1));
			Assert.That(TestLifecycleHookA.DisposeCount, Is.EqualTo(1));
			Assert.That(TestLifecycleHookB.DisposeCount, Is.EqualTo(1));
		});
	}

	[Test]
	public void LifecycleHooks_AreScopedCorrectly()
	{
		var parentRegistry = ObjectRegistry.CreateRoot();
		parentRegistry.RegisterLifecycleHook<TestLifecycleHookA>();
		var parentResolver = parentRegistry.Build();

		var childRegistry = ObjectRegistry.CreateChild(parentResolver);
		childRegistry.RegisterLifecycleHook<TestLifecycleHookB>();
		var childResolver = childRegistry.Build();

		using (var parentLifecycleHookManager = parentResolver.Resolve<ILifecycleHookManager>())
		{
			parentLifecycleHookManager.InstantiateAll();
			parentLifecycleHookManager.InvokeAll<ITestLifecycleHook>(t => t.Invoke());

			Assert.That(TestLifecycleHookA.InvokeCount, Is.EqualTo(1));
			Assert.That(TestLifecycleHookB.InvokeCount, Is.EqualTo(0));

			using (var childLifecycleHookManager = childResolver.Resolve<ILifecycleHookManager>())
			{
				childLifecycleHookManager.InstantiateAll();
				childLifecycleHookManager.InvokeAll<ITestLifecycleHook>(t => t.Invoke());

				Assert.That(TestLifecycleHookA.InvokeCount, Is.EqualTo(1));
				Assert.That(TestLifecycleHookB.InvokeCount, Is.EqualTo(1));
			}

			Assert.That(TestLifecycleHookA.DisposeCount, Is.EqualTo(0));
			Assert.That(TestLifecycleHookB.DisposeCount, Is.EqualTo(1));
		}

		Assert.That(TestLifecycleHookA.InstantiatedCount, Is.EqualTo(1));
		Assert.That(TestLifecycleHookB.InstantiatedCount, Is.EqualTo(1));
		Assert.That(TestLifecycleHookA.DisposeCount, Is.EqualTo(1));
		Assert.That(TestLifecycleHookB.DisposeCount, Is.EqualTo(1));
	}
}