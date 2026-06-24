namespace REContainer.LifecycleHooks;

public sealed class NullLifecycleHooksHandler : LifecycleHooksHandler
{
	public static NullLifecycleHooksHandler Instance { get; } = new();

	private NullLifecycleHooksHandler() { }

	public override void InvokeAll() { }
}