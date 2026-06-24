using EasyDI.Resolving;
using EasyDI.Resolving.InstanceProviders;
using EasyDI.Exceptions;
namespace EasyDI.Registering.RegistrationBuilders;

public sealed class FactoryRegistrationBuilder : RegistrationBuilder
{
	private readonly Func<IObjectResolver, Type, object> _factory;

	public FactoryRegistrationBuilder(Type implementationType,
		Lifetime lifetime,
		Func<IObjectResolver, Type, object> factory)
		: base(implementationType, lifetime)
	{
		_factory = factory;
	}

	protected override IInstanceProvider CreateInstanceProvider() => Lifetime switch
	{
		Lifetime.Transient => new FactoryProvider(_factory, Arguments),
		Lifetime.Singleton or Lifetime.Scoped => new CachedProvider(new FactoryProvider(_factory, Arguments)),
		_ => throw new RegistrationException($"Unsupported lifetime '{Lifetime}' for factory registration")
	};
}