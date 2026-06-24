using REContainer.Resolving;
using REContainer.Resolving.InstanceProviders;

namespace REContainer.Registering.RegistrationBuilders;

public abstract class RegistrationBuilder
{
	internal Type ImplementationType { get; }
	internal Lifetime Lifetime { get; }
	internal HashSet<Type> DeclaredCertainResolvableTypes { get; init; } = [];
	internal HashSet<Type> DeclaredPotentiallyResolvableTypes { get; init; } = [];
	internal List<ArgumentInfo> Arguments { get; init; } = [];

	internal IEnumerable<Type> AllValidResolvableTypes => [..CertainResolvableTypes, ..ValidPotentiallyResolvableTypes];

	protected virtual IEnumerable<Type> ValidPotentiallyResolvableTypes => DeclaredPotentiallyResolvableTypes
		.Where(t => t.IsAssignableFrom(ImplementationType));

	internal IEnumerable<Type> CertainResolvableTypes => DeclaredCertainResolvableTypes.Count is 0 ?
		[ImplementationType] :
		[..DeclaredCertainResolvableTypes];

	protected RegistrationBuilder(Type implementationType, Lifetime lifetime)
	{
		ImplementationType = implementationType;
		Lifetime = lifetime;
	}

	internal Registration Build() =>
		new(ImplementationType,
			Lifetime,
			AllValidResolvableTypes.ToHashSet(),
			CreateInstanceProvider());

	protected abstract IInstanceProvider CreateInstanceProvider();
}