namespace EasyDI.LifecycleHooks;

public abstract class LifecycleHooksHandler
{
	public abstract void InvokeAll();
}

public class LifecycleHooksHandler<T> : LifecycleHooksHandler
{
	private readonly IReadOnlyList<T> _items;

	public LifecycleHooksHandler(IReadOnlyList<T> items)
	{
		_items = items;
	}

	public void InvokeAll(Action<T> invokeItem)
	{
		foreach (var item in _items)
		{
			invokeItem(item);
		}
	}

	public override void InvokeAll() => InvokeAll(static _ => { });
}