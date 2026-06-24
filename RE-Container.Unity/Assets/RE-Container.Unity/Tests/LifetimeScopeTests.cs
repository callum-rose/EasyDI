using System;
using NUnit.Framework;
using REContainer.LifecycleHooks;
using REContainer.LifecycleHooks.Games;
using REContainer.Resolving;
using REContainer.Unity.LifetimeScopes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REContainer.Unity.Tests
{
	public class LifetimeScopeTests
	{
		private sealed class TestLifetimeScope : LifetimeScope
		{
			protected override bool RequiresParentScope(out Type type)
			{
				type = null!;
				return false;
			}
		}

		private sealed class TestChildLifetimeScope : LifetimeScope
		{
			protected override bool DoParentTransformToParentScope => true;

			protected override bool RequiresParentScope(out Type? type)
			{
				type = typeof(TestLifetimeScope);
				return true;
			}
		}

		private class TestLifecycleHook : IInitialisable, IDisposable
		{
			public static int InitialisedCount { get; set; }
			public static int DisposedCount { get; set; }

			public void Initialise()
			{
				InitialisedCount++;
			}

			public void Dispose()
			{
				DisposedCount++;
			}
		}
		
		[SetUp]
		public void SetUp()
		{
			TestLifecycleHook.InitialisedCount = 0;
			TestLifecycleHook.DisposedCount = 0;
		}

		[TearDown]
		public void TearDown()
		{
			foreach (var lifetimeScope in Object.FindObjectsByType<LifetimeScope>(
				         FindObjectsInactive.Include,
				         FindObjectsSortMode.None))
			{
				if (lifetimeScope != null && lifetimeScope.gameObject != null)
				{
					Object.DestroyImmediate(lifetimeScope.gameObject);
				}
			}
		}

		[Test]
		public void LifecycleHook_IsInitialised_WhenResolved()
		{
			using (LifetimeScope.EnqueueInstaller(registry => registry.RegisterLifecycleHook<TestLifecycleHook>()))
			{
				_ = new GameObject().AddComponent<TestLifetimeScope>();
			}

			Assert.AreEqual(1, TestLifecycleHook.InitialisedCount);
		}

		[Test]
		public void LifecycleHook_IsDisposed_WhenLifetimeScopeDestroyed()
		{
			TestLifetimeScope lifetimeScope;

			using (LifetimeScope.EnqueueInstaller(registry => registry.RegisterLifecycleHook<TestLifecycleHook>()))
			{
				lifetimeScope = new GameObject().AddComponent<TestLifetimeScope>();
			}

			Object.DestroyImmediate(lifetimeScope.gameObject);

			Assert.AreEqual(1, TestLifecycleHook.DisposedCount);
		}

		[Test]
		public void ChildLifetimeScope_ParentTransform_IsSetToParentScope()
		{
			TestLifetimeScope lifetimeScope;

			using (LifetimeScope.EnqueueInstaller(registry => registry.RegisterLifecycleHook<TestLifecycleHook>()))
			{
				lifetimeScope = new GameObject().AddComponent<TestLifetimeScope>();
			}

			TestChildLifetimeScope childLifetimeScope;

			using (LifetimeScope.EnqueueInstaller(registry => registry.RegisterLifecycleHook<TestLifecycleHook>()))
			{
				childLifetimeScope = new GameObject().AddComponent<TestChildLifetimeScope>();
			}

			Assert.That(childLifetimeScope.transform.parent, Is.SameAs(lifetimeScope.transform));
		}

		[Test]
		public void ChildLifetimeScope_CannotResolve_ParentLifecycleHook()
		{
			using (LifetimeScope.EnqueueInstaller(registry => registry.RegisterLifecycleHook<TestLifecycleHook>()))
			{
				_ = new GameObject().AddComponent<TestLifetimeScope>();
			}

			var childLifetimeScope = new GameObject().AddComponent<TestChildLifetimeScope>();

			Assert.That(childLifetimeScope.Resolver.CanResolve<IInitialisable>(), Is.False);
		}

		[Test]
		public void LifecycleHook_IsNotDisposed_WhenLifetimeScopeNotDestroyed()
		{
			using (LifetimeScope.EnqueueInstaller(registry => registry.RegisterLifecycleHook<TestLifecycleHook>()))
			{
				_ = new GameObject().AddComponent<TestLifetimeScope>();
			}

			Assert.AreEqual(0, TestLifecycleHook.DisposedCount);
		}
	}
}