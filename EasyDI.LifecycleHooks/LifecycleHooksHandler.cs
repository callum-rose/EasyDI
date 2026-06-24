namespace EasyDI.LifecycleHooks;

public abstract class LifecycleHooksHandler
{
	public abstract void InvokeAll();
}

public class LifecycleHooksHandler<T> : LifecycleHooksHandler
{
	private readonly Action<T> _invokeItem;
	private readonly IReadOnlyList<T> _items;

	public LifecycleHooksHandler(Action<T> invokeItem, IReadOnlyList<T> items)
	{
		_invokeItem = invokeItem;
		_items = items;
	}

	public override void InvokeAll()
	{
		foreach (var item in _items)
		{
			_invokeItem(item);
		}
	}
}