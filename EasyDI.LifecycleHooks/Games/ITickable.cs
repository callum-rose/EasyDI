namespace EasyDI.LifecycleHooks.Games;

public interface ITickable : ILifecycleHook
{
	void Tick();
}