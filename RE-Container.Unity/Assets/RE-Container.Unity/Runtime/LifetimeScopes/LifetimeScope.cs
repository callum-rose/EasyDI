using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using REContainer.LifecycleHooks;
using REContainer.LifecycleHooks.Games;
using REContainer.Registering;
using REContainer.Resolving;
using REContainer.Unity.LifecycleHooks;
using UnityEngine;

namespace REContainer.Unity.LifetimeScopes
{
	[DefaultExecutionOrder(-9999)]
	public abstract partial class LifetimeScope : MonoBehaviour
	{
		internal IObjectResolver Resolver => _resolver;
		
		protected virtual bool DoParentTransformToParentScope => true;

		private IObjectResolver _resolver = null!;
		private ILifecycleHookManager? _lifecycleHookManager;

		protected void Awake()
		{
			ObjectRegistry registry;

			if (!TryFindParentScope(out var parentScope))
			{
				registry = ObjectRegistry.CreateRoot();
			}
			else
			{
				if (DoParentTransformToParentScope)
				{
					transform.SetParent(parentScope.transform);
				}

				registry = ObjectRegistry.CreateChild(parentScope._resolver);
			}

			if (EnqueuedInstallers.TryPeek(out var installer))
			{
				installer(registry);
			}

			Configure(registry);

			_resolver = registry.Build();

			_lifecycleHookManager = _resolver.ResolveOrDefault<ILifecycleHookManager>();
			_lifecycleHookManager?.InvokeInitialisables();
		}

		protected void Start()
		{
			_lifecycleHookManager?.InvokeStartables();
		}

		protected void Update()
		{
			_lifecycleHookManager?.InvokeTickables();
		}

		protected void FixedUpdate()
		{
			_lifecycleHookManager?.InvokePhysicsTickables();
		}

		protected void OnDestroy()
		{
			_lifecycleHookManager?.Dispose();
		}

		protected abstract bool RequiresParentScope([NotNullWhen(true)] out Type? type);

		protected bool IsMissingParentScope()
		{
			return RequiresParentScope(out _) && !TryFindParentScope(out _);
		}
		
		protected virtual void Configure(IObjectRegistry registry){}

		private bool TryFindParentScope([NotNullWhen(true)] out LifetimeScope? parentScope)
		{
			if (RequiresParentScope(out Type? parentScopeType))
			{
				parentScope = FindObjectsByType(parentScopeType, FindObjectsInactive.Exclude, FindObjectsSortMode.None)
					.Cast<LifetimeScope>()
					.FirstOrDefault(ls => ls != this);
				return parentScope != null;
			}

			parentScope = null;
			return false;
		}
	}
}