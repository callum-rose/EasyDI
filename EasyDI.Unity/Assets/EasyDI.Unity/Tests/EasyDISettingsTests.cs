using System;
using System.Collections.Generic;
using EasyDI.Unity.LifetimeScopes;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyDI.Unity.Tests
{
	public class EasyDISettingsTests
	{
		private sealed class TestSettingsRootLifetimeScope : RootLifetimeScope
		{
		}

		private sealed class TestSettingsChildLifetimeScope : LifetimeScope<TestSettingsRootLifetimeScope>
		{
		}

		private sealed class TestSettingsOtherChildLifetimeScope : LifetimeScope<TestSettingsRootLifetimeScope>
		{
		}

		private readonly List<GameObject> _createdGameObjects = new();

		private EasyDISettings _settings;

		[SetUp]
		public void SetUp()
		{
			EasyDISettings.ResetForTesting();

			_settings = ScriptableObject.CreateInstance<EasyDISettings>();
		}

		[TearDown]
		public void TearDown()
		{
			foreach (var gameObject in _createdGameObjects)
			{
				Object.DestroyImmediate(gameObject);
			}

			_createdGameObjects.Clear();

			Object.DestroyImmediate(_settings);

			EasyDISettings.ResetForTesting();
		}

		[Test]
		public void GetScopePrefab_ReturnsRegisteredScope()
		{
			var childScope = CreateScope<TestSettingsChildLifetimeScope>();

			_settings.InitialiseForTesting(CreateScope<TestSettingsRootLifetimeScope>(), childScope);

			Assert.That(EasyDISettings.GetScopePrefab<TestSettingsChildLifetimeScope>(), Is.SameAs(childScope));
		}

		[Test]
		public void GetScopePrefab_ReturnsRootScope()
		{
			var rootScope = CreateScope<TestSettingsRootLifetimeScope>();

			_settings.InitialiseForTesting(rootScope);

			Assert.That(EasyDISettings.GetScopePrefab<TestSettingsRootLifetimeScope>(), Is.SameAs(rootScope));
		}

		[Test]
		public void GetScopePrefab_Throws_WhenScopeIsNotRegistered()
		{
			_settings.InitialiseForTesting(
				CreateScope<TestSettingsRootLifetimeScope>(),
				CreateScope<TestSettingsOtherChildLifetimeScope>());

			Assert.That(
				() => EasyDISettings.GetScopePrefab<TestSettingsChildLifetimeScope>(),
				Throws.InstanceOf<InvalidOperationException>()
					.With.Message.Contains(nameof(TestSettingsChildLifetimeScope)));
		}

		[Test]
		public void GetScopePrefab_Throws_WhenMoreThanOneScopeOfTheTypeIsRegistered()
		{
			_settings.InitialiseForTesting(
				CreateScope<TestSettingsRootLifetimeScope>(),
				CreateScope<TestSettingsChildLifetimeScope>(),
				CreateScope<TestSettingsChildLifetimeScope>());

			Assert.That(
				() => EasyDISettings.GetScopePrefab<TestSettingsChildLifetimeScope>(),
				Throws.InstanceOf<InvalidOperationException>()
					.With.Message.Contains(nameof(TestSettingsChildLifetimeScope)));
		}

		[Test]
		public void TryGetScopePrefab_ReturnsTrue_WhenScopeIsRegistered()
		{
			var childScope = CreateScope<TestSettingsChildLifetimeScope>();

			_settings.InitialiseForTesting(CreateScope<TestSettingsRootLifetimeScope>(), childScope);

			Assert.That(EasyDISettings.TryGetScopePrefab<TestSettingsChildLifetimeScope>(out var prefab), Is.True);
			Assert.That(prefab, Is.SameAs(childScope));
		}

		[Test]
		public void TryGetScopePrefab_ReturnsFalse_WhenScopeIsNotRegistered()
		{
			_settings.InitialiseForTesting(CreateScope<TestSettingsRootLifetimeScope>());

			Assert.That(EasyDISettings.TryGetScopePrefab<TestSettingsChildLifetimeScope>(out var prefab), Is.False);
			Assert.That(prefab, Is.Null);
		}

		[Test]
		public void TryGetScopePrefab_Throws_WhenMoreThanOneScopeOfTheTypeIsRegistered()
		{
			_settings.InitialiseForTesting(
				CreateScope<TestSettingsRootLifetimeScope>(),
				CreateScope<TestSettingsChildLifetimeScope>(),
				CreateScope<TestSettingsChildLifetimeScope>());

			Assert.That(
				() => EasyDISettings.TryGetScopePrefab<TestSettingsChildLifetimeScope>(out _),
				Throws.InstanceOf<InvalidOperationException>());
		}

		[Test]
		public void RootLifetimeScope_Throws_WhenUnassigned()
		{
			_settings.InitialiseForTesting(null, CreateScope<TestSettingsChildLifetimeScope>());

			Assert.That(() => EasyDISettings.RootLifetimeScope, Throws.InstanceOf<InvalidOperationException>());
		}

		private T CreateScope<T>() where T : LifetimeScope
		{
			// Scopes are kept inactive so that they behave like prefabs: Awake doesn't run and no registry is built.
			var gameObject = new GameObject(typeof(T).Name);
			gameObject.SetActive(false);

			_createdGameObjects.Add(gameObject);

			return gameObject.AddComponent<T>();
		}
	}
}
