using System;
using System.Diagnostics.CodeAnalysis;
using Godot;
using EasyDI.Godot.EntryPoints;
using EasyDI.LifecycleHooks;
using EasyDI.LifecycleHooks.Games;
using EasyDI.Registering;
using EasyDI.Resolving;

namespace EasyDI.Godot.LifetimeScopes;

public abstract partial class LifetimeScope : Node
{
	internal IObjectResolver Resolver => _resolver;

	protected virtual bool DoParentTransformToParentScope => true;

	private IObjectResolver _resolver = null!;
	private ILifecycleHookManager? _entryPointManager;

	public override void _EnterTree()
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
                CallDeferred(Node.MethodName.Reparent, parentScope);
			}

			registry = ObjectRegistry.CreateChild(parentScope._resolver);
		}

		if (LifetimeScope.EnqueuedInstallers.TryPeek(out var installer))
		{
			installer(registry);
		}

		Configure(registry);

		_resolver = registry.Build();

		_entryPointManager = _resolver.ResolveOrDefault<ILifecycleHookManager>();
		_entryPointManager?.InvokeInitialisables();
	}

	public override void _Ready()
	{
		_entryPointManager?.InvokeReadyables();
	}

	public override void _Process(double delta)
	{
		_entryPointManager?.InvokeTickables();
	}

	public override void _PhysicsProcess(double delta)
	{
		_entryPointManager?.InvokePhysicsTickables();
	}

	public override void _ExitTree()
	{
		_entryPointManager?.Dispose();
	}

	protected abstract bool RequiresParentScope([NotNullWhen(true)] out Type? type);

	protected bool IsMissingParentScope()
	{
		return RequiresParentScope(out _) && !TryFindParentScope(out _);
	}

	protected virtual void Configure(IObjectRegistry registry) {}

	private bool TryFindParentScope([NotNullWhen(true)] out LifetimeScope? parentScope)
	{
		if (RequiresParentScope(out Type? parentScopeType))
		{
			parentScope = FindParentScopeRecursive(GetTree().Root, parentScopeType);
			return parentScope != null && parentScope != this;
		}

		parentScope = null;
		return false;
	}

	private LifetimeScope? FindParentScopeRecursive(Node node, Type scopeType)
	{
		foreach (Node child in node.GetChildren())
		{
			if (scopeType.IsInstanceOfType(child) && child != this)
				return child as LifetimeScope;

			var found = FindParentScopeRecursive(child, scopeType);
			
			if (found != null)
			{
				return found;
			}
		}
		return null;
	}
}