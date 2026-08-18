using System;
using NUnit.Framework;
using EasyDI.LifecycleHooks;
using EasyDI.LifecycleHooks.Games;
using EasyDI.Resolving;
using EasyDI.Unity.LifetimeScopes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyDI.Unity.Tests
{
	public class LifetimeScopeTests
	{
		private sealed class TestRootLifetimeScope : RootLifetimeScope
		{
		}

		private sealed class TestChildLifetimeScope : LifetimeScope<TestRootLifetimeScope>
		{
		}

		private sealed class TestGrandchildLifetimeScope : LifetimeScope<TestChildLifetimeScope>
		{
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
				_ = new GameObject().AddComponent<TestRootLifetimeScope>();
			}

			Assert.AreEqual(1, TestLifecycleHook.InitialisedCount);
		}

		[Test]
		public void LifecycleHook_IsDisposed_WhenLifetimeScopeDestroyed()
		{
			TestRootLifetimeScope lifetimeScope;

			using (LifetimeScope.EnqueueInstaller(registry => registry.RegisterLifecycleHook<TestLifecycleHook>()))
			{
				lifetimeScope = new GameObject().AddComponent<TestRootLifetimeScope>();
			}

			Object.DestroyImmediate(lifetimeScope.gameObject);

			Assert.AreEqual(1, TestLifecycleHook.DisposedCount);
		}

		[Test]
		public void ChildLifetimeScope_ParentTransform_IsSetToParentScope()
		{
			TestRootLifetimeScope lifetimeScope;

			using (LifetimeScope.EnqueueInstaller(registry => registry.RegisterLifecycleHook<TestLifecycleHook>()))
			{
				lifetimeScope = new GameObject().AddComponent<TestRootLifetimeScope>();
			}

			TestChildLifetimeScope childLifetimeScope;

			using (LifetimeScope.EnqueueInstaller(registry => registry.RegisterLifecycleHook<TestLifecycleHook>()))
			{
				childLifetimeScope = new GameObject().AddComponent<TestChildLifetimeScope>();
			}

			Assert.That(childLifetimeScope.transform.parent, Is.SameAs(lifetimeScope.transform));
		}

		[Test]
		public void GrandchildLifetimeScope_ParentTransform_IsSetToItsOwnParentScope()
		{
			var rootLifetimeScope = new GameObject().AddComponent<TestRootLifetimeScope>();
			var childLifetimeScope = new GameObject().AddComponent<TestChildLifetimeScope>();
			var grandchildLifetimeScope = new GameObject().AddComponent<TestGrandchildLifetimeScope>();

			Assert.That(childLifetimeScope.transform.parent, Is.SameAs(rootLifetimeScope.transform));
			Assert.That(grandchildLifetimeScope.transform.parent, Is.SameAs(childLifetimeScope.transform));
		}

		[Test]
		public void ChildLifetimeScope_CannotResolve_ParentLifecycleHook()
		{
			using (LifetimeScope.EnqueueInstaller(registry => registry.RegisterLifecycleHook<TestLifecycleHook>()))
			{
				_ = new GameObject().AddComponent<TestRootLifetimeScope>();
			}

			var childLifetimeScope = new GameObject().AddComponent<TestChildLifetimeScope>();

			Assert.That(childLifetimeScope.Resolver.CanResolve<IInitialisable>(), Is.False);
		}

		[Test]
		public void LifecycleHook_IsNotDisposed_WhenLifetimeScopeNotDestroyed()
		{
			using (LifetimeScope.EnqueueInstaller(registry => registry.RegisterLifecycleHook<TestLifecycleHook>()))
			{
				_ = new GameObject().AddComponent<TestRootLifetimeScope>();
			}

			Assert.AreEqual(0, TestLifecycleHook.DisposedCount);
		}
	}
}
