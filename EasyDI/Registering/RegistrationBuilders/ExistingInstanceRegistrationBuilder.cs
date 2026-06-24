using EasyDI.Resolving;
using EasyDI.Resolving.InstanceProviders;

namespace EasyDI.Registering.RegistrationBuilders;

public class ExistingInstanceRegistrationBuilder : RegistrationBuilder
{
	protected override IEnumerable<Type> ValidPotentiallyResolvableTypes => DeclaredPotentiallyResolvableTypes
		.Where(t => t.IsInstanceOfType(_instance));

	private readonly object _instance;

	public ExistingInstanceRegistrationBuilder(Type implementationType, object instance)
		: base(implementationType, Lifetime.Singleton)
	{
		_instance = instance;
	}

	protected override IInstanceProvider CreateInstanceProvider() => new ExistingInstanceProvider(_instance);
}