namespace EasyDI.LifecycleHooks;

internal class DisposablesHandler : LifecycleHooksHandler, IDisposable
{
	private readonly IReadOnlyList<IDisposable> _disposables;

	public DisposablesHandler(IReadOnlyList<IDisposable> disposables)
	{
		_disposables = disposables;
	}

	public void Dispose()
	{
		InvokeAll();
	}

	public override void InvokeAll()
	{
		foreach (var disposable in _disposables)
		{
			disposable.Dispose();
		}
	}
}