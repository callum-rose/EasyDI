namespace EasyDI.Resolving;

public abstract record ResolutionQuery(Type Type)
{
	internal IReadOnlyList<Type> DependencyChain { get; init; } = [];

	public static ResolutionQuery Create(Type type) =>
		type.IsListable(out var elementType) ? new ManyInstancesQuery(elementType) : new SingleInstanceQuery(type);
}

public sealed record SingleInstanceQuery(Type Type) : ResolutionQuery(Type)
{
	internal bool FromChildResolver { get; init; }
}

public sealed record ManyInstancesQuery(Type ElementType) : ResolutionQuery(ElementType);