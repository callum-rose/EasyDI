using System;
using EasyDI.Unity.LifetimeScopes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyDI.Unity
{
	internal static class Initialiser
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialise()
		{
			var rootScopePrefab = EasyDISettings.RootLifetimeScope;

			// SceneLifetimeScope reports its parent from a serialised name, and throws when that name is unset.
			if (rootScopePrefab is SceneLifetimeScope)
			{
				throw new InvalidOperationException(
					$"The root lifetime scope prefab '{rootScopePrefab.name}' is a {nameof(SceneLifetimeScope)}, " +
					$"which always parents to another scope. Assign a {nameof(RootLifetimeScope)} instead.");
			}

			var parentScopeType = rootScopePrefab.GetParentScopeType();

			if (parentScopeType != null)
			{
				throw new InvalidOperationException(
					$"The root lifetime scope prefab '{rootScopePrefab.name}' parents to {parentScopeType.Name}, so " +
					$"it can't be instantiated as a root. Assign a {nameof(RootLifetimeScope)} instead.");
			}

			var rootScope = Object.Instantiate(rootScopePrefab);
			Object.DontDestroyOnLoad(rootScope);
		}
	}
}
