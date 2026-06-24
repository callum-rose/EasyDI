namespace REContainer.Resolving;

internal class NullObjectResolver : IObjectResolver
{
	public static readonly NullObjectResolver Instance = new();

	public IObjectResolver Parent => Instance;
	public IReadOnlyList<Registration> LocalRegistrations { get; } = [];
	public IReadOnlyCollection<Type> TypesResolvableAsMany { get; } = [];

	private NullObjectResolver() { }

	public ResolveResult TryLazyResolve(ResolutionQuery query)
	{
		return Fail.Create(FailureReason.UsingNullResolver, query.DependencyChain);
	}
}