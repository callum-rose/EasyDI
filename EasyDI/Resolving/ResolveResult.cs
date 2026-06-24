namespace EasyDI.Resolving;

public abstract record ResolveResult;

public sealed record Success(Func<object> InstanceGetter) : ResolveResult
{
	public static Success Create(Func<object> instanceGetter) => new(instanceGetter);
}

public sealed record Fail : ResolveResult
{
	public FailureReason Reason { get; init; }
	public IReadOnlyList<Type> DependencyChain { get; init; } = [];

	public static Fail Create(FailureReason reason,
		IReadOnlyList<Type> dependencyChain) =>
		new() { Reason = reason, DependencyChain = dependencyChain };
}

public enum FailureReason
{
	UsingNullResolver,
	NotRegistered,
	MarkedAsMany,
	NotMarkedAsMany
}