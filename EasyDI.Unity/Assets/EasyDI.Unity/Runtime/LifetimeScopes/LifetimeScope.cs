using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EasyDI.LifecycleHooks;
using EasyDI.LifecycleHooks.Games;
using EasyDI.Registering;
using EasyDI.Resolving;
using EasyDI.Unity.LifecycleHooks;
using UnityEngine;

namespace EasyDI.Unity.LifetimeScopes
{
	[DefaultExecutionOrder(-9999)]
	public abstract partial class LifetimeScope : MonoBehaviour
	{
		internal IObjectResolver Resolver => _resolver;

		protected virtual bool DoParentTransformToParentScope => true;

		/// <summary>
		/// The type of scope this scope is nested inside, or null when this is a root scope.
		/// </summary>
		/// <remarks>
		/// Prefer deriving from <see cref="RootLifetimeScope"/> or <see cref="LifetimeScope{TParent}"/> instead of
		/// overriding this. Overrides declared outside of this assembly must be declared <c>protected</c>.
		/// </remarks>
		protected internal virtual Type? ParentScopeType => null;

		[SerializeField] private MonoInstaller? installer;

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

			if (EnqueuedInstallers.TryPeek(out var enqueuedInstaller))
			{
				enqueuedInstaller(registry);
			}

			if (installer != null)
			{
				installer.Install(registry);
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

		protected bool IsMissingParentScope()
		{
			return ParentScopeType != null && !TryFindParentScope(out _);
		}

		protected virtual void Configure(IObjectRegistry registry){}

		/// <summary>
		/// Reads <see cref="ParentScopeType"/> from code that doesn't derive from this class.
		/// </summary>
		internal Type? GetParentScopeType()
		{
			return ParentScopeType;
		}

		private bool TryFindParentScope([NotNullWhen(true)] out LifetimeScope? parentScope)
		{
			var parentScopeType = ParentScopeType;

			if (parentScopeType == null)
			{
				parentScope = null;
				return false;
			}

			if (!typeof(LifetimeScope).IsAssignableFrom(parentScopeType))
			{
				throw new InvalidOperationException(
					$"'{name}' declares a parent scope type of {parentScopeType.FullName}, which does not derive " +
					$"from {nameof(LifetimeScope)}.");
			}

			parentScope = FindObjectsByType(parentScopeType, FindObjectsInactive.Exclude, FindObjectsSortMode.None)
				.Cast<LifetimeScope>()
				.FirstOrDefault(ls => ls != this);

			return parentScope != null;
		}
	}
}
