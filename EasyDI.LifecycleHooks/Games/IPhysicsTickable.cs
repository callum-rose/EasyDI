namespace EasyDI.LifecycleHooks.Games;

public interface IPhysicsTickable : ILifecycleHook
{
	void PhysicsTick();
}