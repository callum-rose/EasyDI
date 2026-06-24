using REContainer.LifecycleHooks;

namespace REContainer.Godot.EntryPoints;

public interface IReadyable : ILifecycleHook
{
	void Ready();
}