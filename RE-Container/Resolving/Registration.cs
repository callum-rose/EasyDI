using System.Diagnostics;
using REContainer.Resolving.InstanceProviders;

namespace REContainer.Resolving;

[DebuggerDisplay("{ImplementationType} {Lifetime} [{string.Join(\", \", ResolvableTypes)}]")]
internal sealed record Registration(
	Type ImplementationType,
	Lifetime Lifetime,
	ICollection<Type> ResolvableTypes,
	IInstanceProvider Provider);