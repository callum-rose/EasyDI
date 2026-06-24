using System.Diagnostics;
using EasyDI.Resolving.InstanceProviders;

namespace EasyDI.Resolving;

[DebuggerDisplay("{ImplementationType} {Lifetime} [{string.Join(\", \", ResolvableTypes)}]")]
internal sealed record Registration(
	Type ImplementationType,
	Lifetime Lifetime,
	ICollection<Type> ResolvableTypes,
	IInstanceProvider Provider);