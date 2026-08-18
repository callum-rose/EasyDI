using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EasyDI.Unity.LifetimeScopes;
using UnityEngine;

namespace EasyDI.Unity
{
	public class EasyDISettings : ScriptableObject
	{
		[SerializeField, Tooltip("Instantiated before the first scene loads. Must be a scope with no parent.")]
		private LifetimeScope rootLifetimeScope = null!;

		[SerializeField, Tooltip("Scope prefabs to look up by type with EasyDISettings.GetScopePrefab<T>().")]
		private List<LifetimeScope> scopePrefabs = new();

	#if UNITY_EDITOR
		internal static string RootLifetimeScopePropertyName => nameof(rootLifetimeScope);
		internal static string ScopePrefabsPropertyName => nameof(scopePrefabs);
	#endif

		/// <summary>
		/// The prefab of the scope at the top of the project's scope chain.
		/// </summary>
		public static LifetimeScope RootLifetimeScope => Instance.rootLifetimeScope != null
			? Instance.rootLifetimeScope
			: throw new InvalidOperationException(
				$"No root lifetime scope is assigned on the {nameof(EasyDISettings)} asset. Assign the prefab of the " +
				"scope that should be instantiated before the first scene loads.");

		/// <summary>
		/// Finds the registered prefab for a scope type, searching the root scope and the scope prefab list.
		/// </summary>
		public static T GetScopePrefab<T>() where T : LifetimeScope
		{
			var matches = Instance.FindScopePrefabs<T>();

			return matches.Count switch
			{
				1 => matches[0],
				0 => throw new InvalidOperationException(
					$"No {typeof(T).Name} prefab is registered on the {nameof(EasyDISettings)} asset. Add it to the " +
					$"{ScopePrefabsDisplayName} list, or assign it as the root scope."),
				_ => throw new InvalidOperationException(AmbiguousPrefabsMessage(matches))
			};
		}

		/// <summary>
		/// Finds the registered prefab for a scope type, returning false when none is registered.
		/// </summary>
		/// <exception cref="InvalidOperationException">More than one prefab of the type is registered.</exception>
		public static bool TryGetScopePrefab<T>([NotNullWhen(true)] out T? prefab) where T : LifetimeScope
		{
			var matches = Instance.FindScopePrefabs<T>();

			if (matches.Count > 1)
			{
				throw new InvalidOperationException(AmbiguousPrefabsMessage(matches));
			}

			prefab = matches.Count == 1 ? matches[0] : null;

			return prefab != null;
		}

		private static string ScopePrefabsDisplayName => "Scope Prefabs";

		private static EasyDISettings Instance => _instance != null
			? _hasSingleInstance
				? _instance
				: throw new Exception($"Multiple instances of {nameof(EasyDISettings)} found. There must be only one instance of {nameof(EasyDISettings)} in the project.")
			: throw new Exception($"{nameof(EasyDISettings)} asset not found. Please create one via [Assets -> Create -> EasyDI -> Settings], or via right click in the Project Window [Create -> EasyDI -> Settings].");

		private static EasyDISettings _instance = null!;
		private static bool _hasSingleInstance;

		private void OnEnable()
		{
			if (_instance == null)
			{
				_instance = this;
				_hasSingleInstance = true;
			}
			else if (_instance != this)
			{
				_hasSingleInstance = false;
#if UNITY_EDITOR
				Debug.LogError($"Instance of {nameof(EasyDISettings)} already exists at {UnityEditor.AssetDatabase.GetAssetPath(_instance)}.");
#else
				Debug.LogError($"Multiple instances of {nameof(EasyDISettings)} found. There must be only one instance of {nameof(EasyDISettings)} in the project.");
#endif
			}
		}

		private List<T> FindScopePrefabs<T>() where T : LifetimeScope
		{
			var matches = new List<T>();

			if (rootLifetimeScope != null && rootLifetimeScope is T rootMatch)
			{
				matches.Add(rootMatch);
			}

			foreach (var scopePrefab in scopePrefabs)
			{
				// Entries can be null when a registered prefab has been deleted from the project.
				if (scopePrefab == null || scopePrefab is not T match || matches.Contains(match))
				{
					continue;
				}

				matches.Add(match);
			}

			return matches;
		}

		private static string AmbiguousPrefabsMessage<T>(IReadOnlyCollection<T> matches) where T : LifetimeScope
		{
			return $"{matches.Count} {typeof(T).Name} prefabs are registered on the {nameof(EasyDISettings)} asset " +
			       $"({string.Join(", ", matches.Select(m => m.name))}), so one can't be chosen by type. Remove all " +
			       "but one of them.";
		}

		internal void InitialiseForTesting(LifetimeScope? rootScope, params LifetimeScope[] childScopes)
		{
			rootLifetimeScope = rootScope!;
			scopePrefabs = childScopes.ToList();
			_instance = this;
			_hasSingleInstance = true;
		}

		internal static void ResetForTesting()
		{
			_instance = null!;
			_hasSingleInstance = false;
		}
	}
}
