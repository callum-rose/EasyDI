namespace REContainer.LifecycleHooks.Games;

public interface ITickable : ILifecycleHook
{
	void Tick();
}