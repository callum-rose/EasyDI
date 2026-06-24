using System;
using System.Collections.Generic;
using EasyDI.Registering;

namespace EasyDI.Godot.LifetimeScopes;

public abstract partial class LifetimeScope
{
	private static readonly Stack<Action<IObjectRegistry>> EnqueuedInstallers = new();

	/// <summary>
	/// Call this before instantiating a LifetimeScope from code to have it use the given installer.
	/// </summary>
	/// <returns>An IDisposable to dispose of after instantiation.</returns>
	public static IDisposable EnqueueInstaller(Action<IObjectRegistry> installer)
	{
		EnqueuedInstallers.Push(installer);
		return new ActionDisposable(() => EnqueuedInstallers.Pop());
	}
}