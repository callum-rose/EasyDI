namespace EasyDI.LifecycleHooks.Games;

public interface IInitialisable : ILifecycleHook
{
	void Initialise();
}