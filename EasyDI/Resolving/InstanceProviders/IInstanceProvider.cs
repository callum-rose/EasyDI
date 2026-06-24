namespace EasyDI.Resolving.InstanceProviders;

public interface IInstanceProvider
{
	object GetInstance(IObjectResolver resolver, Type type, IReadOnlyList<Type> dependencyChain);

	IInstanceProvider Clone()
	{
		// Most providers are immutable / stateless so just return this
		return this;
	}
}