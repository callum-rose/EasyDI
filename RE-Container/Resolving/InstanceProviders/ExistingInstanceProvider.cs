namespace REContainer.Resolving.InstanceProviders;

internal sealed class ExistingInstanceProvider : IInstanceProvider
{
	private readonly object _instance;

	public ExistingInstanceProvider(object instance)
	{
		_instance = instance;
	}

	public object GetInstance(IObjectResolver _, Type __, IReadOnlyList<Type> ___)
	{
		return _instance;
	}
}