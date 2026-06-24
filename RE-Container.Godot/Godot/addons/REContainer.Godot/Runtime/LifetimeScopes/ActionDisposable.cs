using System;

namespace REContainer.Godot.LifetimeScopes;

internal class ActionDisposable(Action onDispose) : IDisposable
{
	public void Dispose()
	{
		onDispose();
	}
}