using System;

namespace EasyDI.Godot.LifetimeScopes;

internal class ActionDisposable(Action onDispose) : IDisposable
{
	public void Dispose()
	{
		onDispose();
	}
}