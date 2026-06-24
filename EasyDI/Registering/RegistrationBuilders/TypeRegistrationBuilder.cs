using EasyDI.Resolving;
using EasyDI.Resolving.InstanceProviders;
using EasyDI.Exceptions;
namespace EasyDI.Registering.RegistrationBuilders;

public sealed class TypeRegistrationBuilder : RegistrationBuilder
{
	public TypeRegistrationBuilder(Type implementationType, Lifetime lifetime)
		: base(implementationType, lifetime) { }

	protected override IInstanceProvider CreateInstanceProvider() => (ImplementationType, Lifetime) switch
	{
		({ IsGenericTypeDefinition: true }, Lifetime.Singleton or Lifetime.Scoped) => new TypesCachedProvider(new InstantiatedProvider(Arguments)),
		(_, Lifetime.Transient) => new InstantiatedProvider(Arguments),
		(_, Lifetime.Singleton or Lifetime.Scoped) => new CachedProvider(new InstantiatedProvider(Arguments)),
		_ => throw new RegistrationException($"Unsupported lifetime '{Lifetime}' for type registration")
	};
}