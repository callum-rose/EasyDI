using EasyDI.LifecycleHooks;

namespace EasyDI.Godot.EntryPoints;

public interface IReadyable : ILifecycleHook
{
	void Ready();
}